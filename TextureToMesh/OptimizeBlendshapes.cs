#if UNITY_EDITOR

using Myy;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEditor;
using UnityEngine;

namespace Voyage
{
    public static class MaterialExtensions
    {
        public static Color GetColor(this Material mat, string propertyName, Color defaultValue)
        {
            if (mat.HasProperty(propertyName))
            {
                return mat.GetColor(propertyName);
            }
            else
            {
                Debug.LogError(
                    $"Material {mat.name} has no property {propertyName}.\n" +
                    $"Using the default value {defaultValue}");
                return defaultValue;
            }
        }

        public static float GetFloat(this Material mat, string propertyName, float defaultValue)
        {
            if (mat.HasProperty(propertyName))
            {
                return mat.GetFloat(propertyName);
            }
            else
            {
                Debug.LogError(
                    $"Material {mat.name} has no property {propertyName}.\n" +
                    $"Using the default value {defaultValue}");
                return defaultValue;
            }
        }
    }

    class OptimizedBlendshapes : EditorWindow
    {
        SerializedObject serialO;

        SerializedProperty meshRendererSerialized;
        public GameObject avatar;

        SerializedProperty newMeshNameSerialized;
        public string newMeshName;

        public TextAsset avatarSetupJsonAsset;
        SerializedProperty avatarSetupJsonAssetSerialized;

        private void OnEnable()
        {
            serialO = new SerializedObject(this);
            meshRendererSerialized = serialO.FindProperty("avatar");
            avatarSetupJsonAssetSerialized = serialO.FindProperty("avatarSetupJsonAsset");
            newMeshNameSerialized = serialO.FindProperty("newMeshName");
        }

        [MenuItem("Voyage / Optimize Blendshapes")]
        public static void ShowWindow()
        {
            GetWindow(typeof(OptimizedBlendshapes), false, "Optimize Blendshapes");
        }

        private void PanelError(string message)
        {
            EditorGUILayout.HelpBox(message, MessageType.Error);
        }


        [Serializable]
        public class AvatarSetup
        {
            public int version;
            public Emotions emotions;
        }

        [Serializable]
        public class Emotions
        {
            public Emotion[] data;
        }

        [Serializable]
        public class Emotion
        {
            public string emotion_name;
            public EmotionBlendshape[] blendshapes;
        }

        [Serializable]
        public class EmotionBlendshape
        {
            public string blendshape_name;
            public float current_value;
            public string rel_path; // Godot path
            public string short_path;
        }

        Dictionary<string, HashSet<string>> usedBlendshapes;

        class BlendShapeFrame
        {
            public Vector3[] deltaVertices;
            public Vector3[] deltaNormals;
            public Vector3[] deltaTangents;

            public BlendShapeFrame(int vertexCount)
            {
                deltaVertices = new Vector3[vertexCount];
                deltaNormals = new Vector3[vertexCount];
                deltaTangents = new Vector3[vertexCount];
            }
        }

        class BlendShapeData
        {
            public string name;
            public int frames;
            public BlendShapeFrame[] frame;

            public BlendShapeData(string name, int frames, int vertexCount)
            {
                this.name   = name;
                this.frames = frames;
                frame = new BlendShapeFrame[frames];
                /* Initialize everything nicely */
                for (int i = 0; i < frames; i++)
                {
                    frame[i] = new BlendShapeFrame(vertexCount);
                }
            }

            public void AddTo(Mesh mesh)
            {
                float fMax = frames;
                for (int f = 0; f < frames; f++)
                {
                    var frameData = frame[f];
                    mesh.AddBlendShapeFrame(
                        name, 
                        (f + 1) * 100 / fMax, /* Weight from 0 to 100f */
                        frameData.deltaVertices,
                        frameData.deltaNormals,
                        frameData.deltaTangents);
                }
            }
        }

        /* FIXME Actually, these names should be sampled from
         * the LipSync configuration of the avatar.
         */
        readonly string[] blendShapesToAlwaysSave =
        {
            "vrc.v_sil",
            "vrc.v_pp",
            "vrc.v_ff",
            "vrc.v_th",
            "vrc.v_dd",
            "vrc.v_kk",
            "vrc.v_ch",
            "vrc.v_ss",
            "vrc.v_nn",
            "vrc.v_rr",
            "vrc.v_aa",
            "vrc.v_e",
            "vrc.v_ih",
            "vrc.v_oh",
            "vrc.v_ou"
        };

        private void Optimize(GameObject avatar, TextAsset avatarSetupJsonFile, string generatedMeshFileName)
        {
            AvatarSetup setup;
            try
            {
                 setup = JsonUtility.FromJson<AvatarSetup>(avatarSetupJsonFile.text);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not parse the JSON file correctly !");
                Debug.LogException(e);
                return;
            }

            usedBlendshapes = new Dictionary<string, HashSet<string>>();

            foreach (var emotion in setup.emotions.data)
            {
                foreach (var blendshape in emotion.blendshapes)
                {
                    
                    if (usedBlendshapes.ContainsKey(blendshape.short_path) == false)
                    {
                        usedBlendshapes[blendshape.short_path] = new HashSet<string>();
                    }

                    if (blendshape.current_value > 0)
                    {
                        usedBlendshapes[blendshape.short_path].Add(blendshape.blendshape_name);
                    }
                    
                }
            }

            foreach (var skinData in usedBlendshapes)
            {
                Transform child = avatar.transform.Find(skinData.Key);
                if (child == null)
                {
                    Debug.LogError($"Could not find skin {avatar.name}/{skinData.Key}");
                    continue;
                }

                GameObject childObject = child.gameObject;
                string childName = childObject.name;
                SkinnedMeshRenderer skin = childObject.GetComponent<SkinnedMeshRenderer>();
                if (skin == null)
                {
                    Debug.LogError($"Child {childName} has no SkinnedMeshRenderer !");
                    continue;
                }

                Mesh sharedMesh = Instantiate(skin.sharedMesh);

                HashSet<BlendShapeData> activeBlendShapes = new HashSet<BlendShapeData>();

                /* FIXME Get this name from the avatar */
                string blinkBlendShapeName = "まばたき";

                /* Hack to add protected blendshapes */
                skinData.Value.UnionWith(blendShapesToAlwaysSave);
                skinData.Value.Add(blinkBlendShapeName);

                /* Save the blendshapes */
                foreach (var blendShapeName in skinData.Value)
                {
                    
                    int blendShapeID = sharedMesh.GetBlendShapeIndex(blendShapeName);
                    if (blendShapeID < 0)
                    {
                        Debug.LogError($"{childName} has no blendshape named {blendShapeName}");
                        continue;
                    }

                    int vertexCount = sharedMesh.vertexCount;
                    int blendShapeFrames = sharedMesh.GetBlendShapeFrameCount(blendShapeID);
                    BlendShapeData blendShapeData = new BlendShapeData(blendShapeName, blendShapeFrames, vertexCount);

                    for (int i = 0; i < blendShapeFrames; i++)
                    {
                        BlendShapeFrame frame = blendShapeData.frame[i];
                        sharedMesh.GetBlendShapeFrameVertices(blendShapeID, i, frame.deltaVertices, frame.deltaNormals, frame.deltaTangents);
                    }

                    activeBlendShapes.Add(blendShapeData);


                }

                /* Clear everything */

                sharedMesh.ClearBlendShapes();

                /* Add the useful blendshapes back */

                foreach (var activeBlendShape in activeBlendShapes)
                {
                    activeBlendShape.AddTo(sharedMesh);
                }

                AssetDatabase.CreateAsset(sharedMesh, $"Assets/{generatedMeshFileName}.mesh");
            }
        }

        private void OnGUI()
        {

            serialO.Update();
            EditorGUILayout.PropertyField(meshRendererSerialized);
            EditorGUILayout.PropertyField(avatarSetupJsonAssetSerialized);
            EditorGUILayout.PropertyField(newMeshNameSerialized);
            serialO.ApplyModifiedProperties();

            bool everythingOK = true;

            if (avatar == null)
            {
                PanelError("No avatar provided");
                everythingOK = false;
            }

            if (avatarSetupJsonAsset == null)
            {
                PanelError("No JSON file provided");
                everythingOK = false;
            }

            if (newMeshName == null || newMeshName == "")
            {
                PanelError("Please provide a filename for the new Mesh");
                everythingOK = false;
            }


            if (!everythingOK) return;

            if (GUILayout.Button("Optimize materials"))
            {

                Optimize(avatar, avatarSetupJsonAsset, newMeshName);

            }
        }
    }
}

#endif
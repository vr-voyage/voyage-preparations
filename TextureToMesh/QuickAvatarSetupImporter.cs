#if UNITY_EDITOR

using Myy;
using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

/* FIXME : Set that as a Component, instead of a floating window.
 * There's almost no need for a floating window anyway, beside testing
 */
namespace Voyage
{

    public static class UnityAnimationExtensions
    {
        // entryToState.AddCondition(AnimatorConditionMode.Equals, emoteIndex, emoteParamName);

        public static AnimationClip defaultClip = null;

        public static AnimatorStateTransition MakeInstant(
            this AnimatorStateTransition transition)
        {
            return transition.SetTimings(0, 0);
        }

        public static AnimatorStateTransition AddExitTransition(
            this AnimatorState from,
            string paramName,
            AnimatorConditionMode condition,
            float threshold,
            bool defaultExitTime = false,
            float exitTime = 0,
            float duration = 0)
        {
            var transition = from.AddExitTransition(defaultExitTime);
            transition.AddCondition(paramName, condition, threshold, exitTime, duration);
            return transition;
        }

        public static AnimatorState AddEmptyState(
            this AnimatorStateMachine machine,
            string stateName,
            bool writeDefaults = false)
        {
            return machine.AddState(stateName, defaultClip, writeDefaults);
        }

        public static void SetValueCurve(
            this AnimationClip clip,
            string relativePath,
            Type propertiesType,
            string propertyName,
            float value)
        {

            AnimationCurve curve = new AnimationCurve()
            {
                keys = new Keyframe[]
                {
                    new Keyframe()
                    {
                        time = 0,
                        value = value
                    },
                    new Keyframe()
                    {
                        time = 1/60.0f,
                        value = value
                    }
                }
            };

            clip.SetCurve(relativePath, propertiesType, propertyName, curve);
        }
    }
    public class QuickAvatarSetupImporter : EditorWindow
    {


        SerializedObject serialO;

        public VRCAvatarDescriptor avatar;
        SerializedProperty avatarSerialized;

        public TextAsset avatarSetupJsonAsset;
        SerializedProperty avatarSetupJsonAssetSerialized;

        public string emoteParamName = "MyyEmoteParam";
        SerializedProperty emoteParamNameSerialized;

        /* Note : You cannot move this before variables declaration */
        [MenuItem("Voyage / Quick Avatar Setup importer")]

        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(QuickAvatarSetupImporter));

        }

        private VRCExpressionsMenu VRCMenuCreate()
        {
            return ScriptableObject.CreateInstance<VRCExpressionsMenu>();
        }

        private VRCExpressionParameters VRCExpressionParamsCreate(int nParamsExpected)
        {
            VRCExpressionParameters menuParams = ScriptableObject.CreateInstance<VRCExpressionParameters>();
            menuParams.parameters = new VRCExpressionParameters.Parameter[nParamsExpected];

            return menuParams;
        }

        void VRCExpressionParamsSetParam(
            VRCExpressionParameters menuParams,
            int paramIndex,
            string paramName,
            VRCExpressionParameters.ValueType paramType,
            float paramDefaultValue,
            bool isParamSaved)
        {
            menuParams.parameters[paramIndex] = new VRCExpressionParameters.Parameter
            {
                name = paramName,
                valueType = paramType,
                defaultValue = paramDefaultValue,
                saved = isParamSaved
            };
        }

        private VRCExpressionsMenu[] VRCSubMenusCreate(int amount)
        {
            VRCExpressionsMenu[] subMenus = new VRCExpressionsMenu[amount];
            for (int i = 0; i < amount; i++)
            {
                subMenus[i] = VRCMenuCreate();
            }
            return subMenus;
        }

        private VRCExpressionsMenu.Control VRCMenuAddControl(
            VRCExpressionsMenu menu,
            string label,
            VRCExpressionsMenu.Control.ControlType controlType,
            string parameterName,
            float value)
        {
            VRCExpressionsMenu.Control control = new VRCExpressionsMenu.Control
            {
                name = label,
                value = value,
                type = controlType,
                parameter = new VRCExpressionsMenu.Control.Parameter()
                {
                    name = parameterName
                },
                style = VRCExpressionsMenu.Control.Style.Style1
            };
            menu.controls.Add(control);
            return control;
        }

        private VRCExpressionsMenu.Control VRCMenuAddSubMenu(
            VRCExpressionsMenu mainMenu,
            string label,
            VRCExpressionsMenu subMenu,
            string parameterName,
            float value)
        {
            VRCExpressionsMenu.Control control = VRCMenuAddControl(
                mainMenu, label,
                VRCExpressionsMenu.Control.ControlType.SubMenu, parameterName, value);
            control.subMenu = subMenu;
            return control;
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

        private void OnEnable()
        {
            serialO = new SerializedObject(this);
            avatarSerialized = serialO.FindProperty("avatar");
            emoteParamNameSerialized = serialO.FindProperty("emoteParamName");
            avatarSetupJsonAssetSerialized = serialO.FindProperty("avatarSetupJsonAsset");
        }

        /* From https://stackoverflow.com/questions/309485/c-sharp-sanitize-file-name */
        private string FilesystemFriendlyName(string name)
        {
            var invalids = System.IO.Path.GetInvalidFileNameChars();
            return String.Join("_", name.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }

        private void SetBlinking(AnimationClip clip, bool state)
        {
            float numericState = (state ? (float)VRCAvatarDescriptor.EyelidType.Blendshapes : 0);
            clip.SetCurve("", typeof(VRCAvatarDescriptor), "customEyeLookSettings.eyelidType", numericState);
        }

        private AnimationClip CreateClipDefaultBlendshapes(params SkinnedMeshRenderer[] renderers)
        {
            AnimationClip clip = new AnimationClip();

            //SetBlinking(clip, true);
            foreach (SkinnedMeshRenderer skin in renderers)
            {
                Mesh sharedMesh = skin.sharedMesh;
                for (int i = 0; i < sharedMesh.blendShapeCount; i++)
                {
                    string blendShapeName = sharedMesh.GetBlendShapeName(i);

                    if (blendShapeName.StartsWith("vrc.") || blendShapeName.StartsWith("まばたき"))
                    {
                        continue;
                    }

                    float weight = skin.GetBlendShapeWeight(i);

                    /* FIXME Get the actual path ! */
                    clip.SetValueCurve(skin.name, typeof(SkinnedMeshRenderer), $"blendShape.{blendShapeName}", weight);
                }

            }
            return clip;
        }

        private void OnGUI()
        {

            serialO.Update();
            EditorGUILayout.PropertyField(avatarSerialized);
            EditorGUILayout.PropertyField(avatarSetupJsonAssetSerialized);
            EditorGUILayout.PropertyField(emoteParamNameSerialized);
            serialO.ApplyModifiedProperties();

            if (GUILayout.Button("Add emotions"))
            {
                if (emoteParamName == null || avatarSetupJsonAsset == null)
                {
                    Debug.LogError("[QuickAvatarSetupImporter] Some fields are not setup !");
                    return;
                }

                string avatarSetupJsonFilePath = AssetDatabase.GetAssetPath(avatarSetupJsonAsset);
                string extensionName = ".avatar_setup.json";
                string extensionNameWithGlb = ".glb" + extensionName;

                if (!avatarSetupJsonFilePath.EndsWith(extensionName))
                {
                    Debug.LogError(
                        "The JSON file you provided does not end with " + extensionName + "\n" +
                        "Aborting...");
                    return;
                }

                /* We keep the path, without the .glb.avatar_setup.json or
                 * .avatar_setup.json extension, in order to determine where to save
                 * the various generated files.
                 */
                string baseFilePath;
                if (avatarSetupJsonFilePath.EndsWith(extensionNameWithGlb))
                    baseFilePath = avatarSetupJsonFilePath.Replace(extensionNameWithGlb, "");
                else
                    baseFilePath = avatarSetupJsonFilePath.Replace(extensionName, "");

                /* Controller */
                AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(baseFilePath + "_emotions_controller.controller");
                AnimatorControllerParameter param = new AnimatorControllerParameter
                {
                    type = AnimatorControllerParameterType.Int,
                    defaultInt = 0,
                    name = emoteParamName
                };
                controller.AddParameter(param);

                /* Default animation clip for empty states */
                AnimationClip emptyClip = new AnimationClip();
                AssetDatabase.CreateAsset(
                    emptyClip,
                    $"{baseFilePath}_empty_state_animation.anim");
                UnityAnimationExtensions.defaultClip = emptyClip;

                /* Equivalent of "Write defaults" state */
                AnimationClip currentStateClip = 
                    CreateClipDefaultBlendshapes(avatar.transform.GetComponentsInChildren<SkinnedMeshRenderer>());
                AssetDatabase.CreateAsset(
                    currentStateClip,
                    $"{baseFilePath}_default_state_animation.anim");

                AnimatorStateMachine rootMachine = controller.layers[0].stateMachine;

                AnimatorState entryState = rootMachine.AddState("WriteDefaults", currentStateClip);
                AnimatorState waitDecision = rootMachine.AddEmptyState("WaitDecision");
                AnimatorState decisionState = rootMachine.AddEmptyState("Decision");
                entryState.AddTransition(waitDecision, true).MakeInstant();
                waitDecision.AddTransition(decisionState, emoteParamName, AnimatorConditionMode.NotEqual, 0);
                /* FIXME Try to make this a default transition */
                decisionState.AddTransition(waitDecision, emoteParamName, AnimatorConditionMode.Equals, 0);

                //rootMachine.AddEntryTransition(entryState);
                /* -- Controller -- */

                int emoteIndex = 1;
                AvatarSetup setup = JsonUtility.FromJson<AvatarSetup>(avatarSetupJsonAsset.text);
                Emotions emotions = setup.emotions;
                int nEmotions = emotions.data.Length;
                VRCExpressionsMenu[] subMenus = VRCSubMenusCreate(nEmotions / 8 + 1);

                for (int i = 0; i < nEmotions; i++)
                {
                    /* To create an animation that moves blendshapes from 0 to 100,
                        * you need to :
                        * - Create an animation clip
                        * Then for each blendshape (property) you want to move :
                        * - Create at least two key frames
                        * - Set the key frames values at '100' (meaning that the driven
                        *   values are set at 100 when hitting these keyframes)
                        * - Create a curve that uses these two frames
                        * - Add the curve to the animation clip, and tell the clip
                        *  to use this curve to drive the blendshape property.
                        */
                    Emotion emotion = emotions.data[i];
                    VRCExpressionsMenu subMenu = subMenus[i / 8];
                    AnimationClip clip = new AnimationClip();

                    clip.ClearCurves();
                    //SetBlinking(clip, false);
                    foreach (EmotionBlendshape blendshape in emotion.blendshapes)
                    {

                        if (blendshape.blendshape_name.StartsWith("vrc.")) continue;
                        AnimationCurve curve = new AnimationCurve();
                        Keyframe frame1 = new Keyframe();
                        Keyframe frame2 = new Keyframe();
                        frame1.time = 0;
                        frame1.value = Mathf.Floor(blendshape.current_value * 100.0f);
                        frame2.time = 1 / 60.0f;
                        frame2.value = Mathf.Floor(blendshape.current_value * 100.0f);

                        curve.AddKey(frame1);
                        curve.AddKey(frame2);

                        clip.SetCurve(
                            blendshape.short_path,
                            typeof(SkinnedMeshRenderer),
                            "blendShape." + blendshape.blendshape_name,
                            curve);

                    }
                    /* Create the animation file, so that we can create a state without issues */
                    string animationFilePath =
                        baseFilePath + "_emote_anim_" + FilesystemFriendlyName(emotion.emotion_name) + ".anim";
                    AssetDatabase.CreateAsset(clip, animationFilePath);
                    /* Controller */

                    /* Setup a state, inside the controller, using the generated clip.
                        * Then setup a transition between the entry state (DummyState)
                        * and the created state.
                        * Add a `emoteParamName == emoteIndex (i + 1)` condition for
                        * the transition from Entry to New State, and make it emoteParamName != emoteIndex
                        * when returning back to the main Entry state.
                        * That way, the animation will be triggered correctly when toggling
                        * the right value in the VRChat menu.
                        */
                    var changeState = rootMachine.AddState(FilesystemFriendlyName(emotion.emotion_name), clip);
                    var waitState = rootMachine.AddEmptyState("Wait");

                    decisionState.AddTransition(changeState, emoteParamName, AnimatorConditionMode.Equals, emoteIndex);
                    changeState.AddTransition(waitState, true).MakeInstant();
                    waitState.AddExitTransition(emoteParamName, AnimatorConditionMode.NotEqual, emoteIndex);

                    /* Add an entry to the VRChat menu to toggle this animation,
                        * by setting the animation parameter value to the current
                        * emoteIndex when toggled. */
                    VRCMenuAddControl(
                        subMenu, emotion.emotion_name,
                        VRCExpressionsMenu.Control.ControlType.Toggle,
                        emoteParamName, emoteIndex);
                    emoteIndex += 1;
                }

                /* Generate the VRChat menus and submenu files */
                VRCExpressionsMenu menu = VRCMenuCreate();

                for (int i = 0; i < subMenus.Length; i++)
                {
                    string subMenuFilePath = baseFilePath + "_vrc_sub_menu_" + i + ".asset";
                    AssetDatabase.CreateAsset(subMenus[i], subMenuFilePath);
                    VRCMenuAddSubMenu(menu, "SubMenu" + i, subMenus[i], null, i + 1);
                }

                string menuFilePath = baseFilePath + "_vrc_menu.asset";
                AssetDatabase.CreateAsset(menu, menuFilePath);

                VRCExpressionParameters menuParams = VRCExpressionParamsCreate(1);
                VRCExpressionParamsSetParam(
                    menuParams, 0, emoteParamName,
                    VRCExpressionParameters.ValueType.Int, 0,
                    false);

                string paramsFilePath = baseFilePath + "_vrc_menu_parameters.asset";
                AssetDatabase.CreateAsset(menuParams, paramsFilePath);

                /* Setting up the avatar */
                avatar.customExpressions = true;
                avatar.customizeAnimationLayers = true;
                avatar.expressionsMenu = menu;
                avatar.expressionParameters = menuParams;
                // avatar.baseAnimationLayers[4] = FX Layer
                avatar.baseAnimationLayers[4].animatorController = controller;
                avatar.baseAnimationLayers[4].isDefault = false;

            }
        }
    }
}

#endif
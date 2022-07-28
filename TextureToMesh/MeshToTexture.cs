#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;

/* FIXME : Set that as a Component, instead of a floating window.
 * There's almost no need for a floating window anyway, beside testing
 */
namespace Voyage {
    
    public class MeshToTexture : EditorWindow
    {

        SerializedObject serialO;
        SerializedProperty meshSerialized;
        SerializedProperty pathSerialized;
        public Mesh mesh;
        public UnityEngine.Object saveDir;
        private string assetsDir;
        public string saveFilePath;

        /* Note : You cannot move this before variables declaration */
        [MenuItem("Voyage / Mesh To Texture")]

        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(MeshToTexture), true);
        }

        private void OnEnable()
        {
            assetsDir = Application.dataPath;
            serialO = new SerializedObject(this);
            meshSerialized = serialO.FindProperty("mesh");
            pathSerialized = serialO.FindProperty("saveFilePath");

            saveDir = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets");
        }

        const int float_size = 4;
        const int int_size = 4;

        void DumpMetadata(int[] metadata)
        {
            Debug.Log($"{metadata[0]:X} {metadata[1]:X}");
            Debug.Log($"vertices : {metadata[2]:X}");
            Debug.Log($"normals  : {metadata[3]:X}");
            Debug.Log($"uvs      : {metadata[4]:X}");
            Debug.Log($"indices  : {metadata[5]:X}");
        }

        Texture2D EncodeMeshAsTexture(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector2[] uvs = mesh.uv;
            int[] indices = mesh.triangles;


            int n_pixels =
                vertices.Length * 3
                + uvs.Length * 2
                + normals.Length * 3
                + indices.Length;


            int pow2_size = 1;
            int pow2 = 1;
            while (pow2_size < n_pixels)
            {
                pow2++;
                pow2_size <<= 1;
            }

            int pow2_part = 1 << (pow2 / 2);
            Debug.Log($"{n_pixels} < {pow2_size} - {pow2_part}");

            Texture2D texture = new Texture2D(pow2_part, pow2_part);
            float[] meshData = new float[vertices.Length * 3 + normals.Length * 3 + uvs.Length * 2];
            int m = 0;
            for (int v = 0; v < vertices.Length; v++)
            {
                Vector3 vertex = vertices[v];
                meshData[m + 0] = vertex.x;
                meshData[m + 1] = vertex.y;
                meshData[m + 2] = vertex.z;
                m += 3;
            }
            for (int n = 0; n < normals.Length; n++)
            {
                Vector3 normal = normals[n];
                meshData[m + 0] = normal.x;
                meshData[m + 1] = normal.y;
                meshData[m + 2] = normal.z;
                m += 3;
            }
            for (int u = 0; u < uvs.Length; u++)
            {
                Vector3 uv = uvs[u];
                meshData[m + 0] = uv.x;
                meshData[m + 1] = uv.y;
                m += 2;
            }
            int[] metadata = new int[8];

            metadata[0] = 0x48616d73; // HAMS
            metadata[1] = 0x74657273; // TERS
            metadata[2] = vertices.Length;
            metadata[3] = normals.Length;
            metadata[4] = uvs.Length;
            metadata[5] = indices.Length;
            metadata[6] = 0;
            metadata[7] = 0;
            DumpMetadata(metadata);
            byte[] pixels = new byte[pow2_part * pow2_part * 4];

            Debug.Log($"meshData[0] = {meshData[0]}");

            Buffer.BlockCopy(metadata, 0, pixels, 0, metadata.Length * int_size);
            Buffer.BlockCopy(meshData, 0, pixels, metadata.Length * int_size, meshData.Length * float_size);
            Buffer.BlockCopy(indices, 0, pixels, metadata.Length * int_size + meshData.Length * float_size, indices.Length * int_size);

            texture.SetPixelData(pixels, 0, 0);

            Debug.Log($"Pixels[32..35] = {BitConverter.ToSingle(pixels, (metadata.Length * int_size))}");

            return texture;
        }

        private void OnGUI()
        {

            
            bool everythingOK = true;
            serialO.Update();

            EditorGUILayout.PropertyField(meshSerialized, true);
            EditorGUILayout.PropertyField(pathSerialized);
            serialO.ApplyModifiedProperties();

            if (mesh == null || saveFilePath == null) everythingOK = false;
            if (!everythingOK) return;

            if (GUILayout.Button("Generate texture"))
            {
                Texture2D texture = EncodeMeshAsTexture(mesh);

                if (texture == null) return;

                byte[] pngData = texture.EncodeToPNG();
                File.WriteAllBytes($"{assetsDir}/{saveFilePath}", pngData);

                AssetDatabase.Refresh();
            }
        }
    }
}
#endif

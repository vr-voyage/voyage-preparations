#if UNITY_EDITOR

using Myy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Voyage
{
public class TextureToMesh : EditorWindow
{
    public Texture2D texture;

    SimpleEditorUI ui;



    bool CheckTexture(SerializedProperty textureProperty)
    {
        return texture != null;
    }

    private void OnEnable()
    {
        ui = new SimpleEditorUI(this,
            ("Texture", "texture", CheckTexture));
    }

    void DumpMetadata(int[] metadata)
    {
        Debug.Log($"{metadata[0]:X} {metadata[1]:X}");
        Debug.Log($"vertices : {metadata[2]:X}");
        Debug.Log($"normals  : {metadata[3]:X}");
        Debug.Log($"uvs      : {metadata[4]:X}");
        Debug.Log($"indices  : {metadata[5]:X}");
    }

    public const int HAMS = 0x48616d73;
    public const int TERS = 0x74657273;

    Mesh MeshFromTexture(Texture2D texture)
    {
        Mesh mesh = null;
        byte[] data = texture.GetRawTextureData();
        int[] metadata = new int[8];

        int metadataByteSize = metadata.Length * sizeof(int);

        if (data == null) return mesh;
        if (data.Length < metadataByteSize)
        {
            Debug.LogError($"The selected texture is WAY too small ({data.Length} bytes)");
            return mesh;
        }

        Buffer.BlockCopy(data, 0, metadata, 0, metadataByteSize);

        DumpMetadata(metadata);

        if ((metadata[0] != HAMS) & (metadata[1] != TERS))
        {
            Debug.LogError("This is not the hamsters you are looking for.");
            Debug.LogError($"{metadata[0]:X} - {metadata[1]:X}");
            return mesh;
        }

        int nVertices = metadata[2];
        int nNormals = metadata[3];
        int nUVS = metadata[4];
        int nIndices = metadata[5];

        float[] meshData = new float[nVertices * 3 + nNormals * 3 + nUVS * 2];
        int[] indices = new int[nIndices];

        int meshDataByteSize = meshData.Length * sizeof(float);

        int totalSize = metadataByteSize + meshDataByteSize;

        if (totalSize > data.Length)
        {
            Debug.LogError($"We need {totalSize} bytes but only got {data.Length} bytes");
            return mesh;
        }

        Buffer.BlockCopy(data, metadataByteSize,                    meshData, 0, meshDataByteSize);
        Buffer.BlockCopy(data, metadataByteSize + meshDataByteSize, indices,  0, indices.Length * sizeof(int));

        Vector3[] vertices = new Vector3[nVertices];
        Vector3[] normals = new Vector3[nNormals];
        Vector2[] uvs = new Vector2[nUVS];

        int cursor = 0;
        for (int v = 0; v < nVertices; v++)
        {
            Vector3 vertex = new Vector3(
                meshData[cursor + 0],
                meshData[cursor + 1],
                meshData[cursor + 2]);
            vertices[v] = vertex;
            cursor += 3;
        }
        for (int n = 0; n < nNormals; n++)
        {
            Vector3 normal = new Vector3(
                meshData[cursor + 0],
                meshData[cursor + 1],
                meshData[cursor + 2]);
            normals[n] = normal;
            cursor += 3;
        }
        for (int u = 0; u < nUVS; u++)
        {
            Vector2 uv = new Vector2(
                meshData[cursor + 0],
                meshData[cursor + 1]);
            uvs[u] = uv;
            cursor += 2;
        }


        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = indices;

        return mesh;
    }

    int Color32ToInt(Color32 color)
    {
        return
            (color.r << 0) |
            (color.g << 8) |
            (color.b << 16) |
            (color.a << 24);
    }

    Mesh MeshFromTexture2(Texture2D texture)
    {
        Mesh mesh = null;
        Color32[] data = texture.GetPixels32();

        Debug.Log($"{Color32ToInt(data[0]):X} == {HAMS:X}");
        Debug.Log($"{Color32ToInt(data[1]):X} == {TERS:X}");
        

        return mesh;

        /*int[] metadata = new int[8];

        int metadataByteSize = metadata.Length * sizeof(int);

        if (data == null) return mesh;
        if (data.Length < metadataByteSize)
        {
            Debug.LogError($"The selected texture is WAY too small ({data.Length} bytes)");
            return mesh;
        }

        Buffer.BlockCopy(data, 0, metadata, 0, metadataByteSize);

        DumpMetadata(metadata);

        if ((metadata[0] != HAMS) & (metadata[1] != TERS))
        {
            Debug.LogError("This is not the hamsters you are looking for.");
            Debug.LogError($"{metadata[0]:X} - {metadata[1]:X}");
            return mesh;
        }

        int nVertices = metadata[2];
        int nNormals = metadata[3];
        int nUVS = metadata[4];
        int nIndices = metadata[5];

        float[] meshData = new float[nVertices * 3 + nNormals * 3 + nUVS * 2];
        int[] indices = new int[nIndices];

        int meshDataByteSize = meshData.Length * sizeof(float);

        int totalSize = metadataByteSize + meshDataByteSize;

        if (totalSize > data.Length)
        {
            Debug.LogError($"We need {totalSize} bytes but only got {data.Length} bytes");
            return mesh;
        }

        Buffer.BlockCopy(data, metadataByteSize, meshData, 0, meshDataByteSize);
        Buffer.BlockCopy(data, metadataByteSize + meshDataByteSize, indices, 0, indices.Length * sizeof(int));

        Vector3[] vertices = new Vector3[nVertices];
        Vector3[] normals = new Vector3[nNormals];
        Vector2[] uvs = new Vector2[nUVS];

        int cursor = 0;
        for (int v = 0; v < nVertices; v++)
        {
            Vector3 vertex = new Vector3(
                meshData[cursor + 0],
                meshData[cursor + 1],
                meshData[cursor + 2]);
            vertices[v] = vertex;
            cursor += 3;
        }
        for (int n = 0; n < nNormals; n++)
        {
            Vector3 normal = new Vector3(
                meshData[cursor + 0],
                meshData[cursor + 1],
                meshData[cursor + 2]);
            normals[n] = normal;
            cursor += 3;
        }
        for (int u = 0; u < nUVS; u++)
        {
            Vector2 uv = new Vector2(
                meshData[cursor + 0],
                meshData[cursor + 1]);
            uvs[u] = uv;
            cursor += 2;
        }


        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = indices;

        return mesh;*/
    }

    //public static uint NAN = 0x80000000;
    //public static uint BIAS = 127;
    //public static uint K = 8;
    //public static uint N = 23;

    ///* Compute (int) f.
    // * If conversion causes overflow or f is NaN, return 0x80000000
    // */
    //int float_f2i(float_bits f)
    //{
    //    unsigned s = f >> (K + N);
    //    unsigned exp = f >> N & 0xFF;
    //    unsigned frac = f & 0x7FFFFF;

    //    /* Denormalized values round to 0 */
    //    if (exp == 0)
    //        return 0;
    //    /* f is NaN */
    //    if (exp == 0xFF)
    //        return NAN;
    //    /* Normalized values */
    //    int x;
    //    int E = exp - BIAS;
    //    /* Normalized value less than 0, return 0 */
    //    if (E < 0)
    //        return 0;
    //    /* Overflow condition */
    //    if (E > 30)
    //        return NAN;
    //    x = 1 << E;
    //    if (E < N)
    //        x |= frac >> (N - E);
    //    else
    //        x |= frac << (E - N);

    //    /* Negative values */
    //    if (s == 1)
    //        x = ~x + 1;

    //    return x;
    //}

    /* Note : You cannot move this before variables declaration */
    [MenuItem("Voyage / Texture to Mesh")]

    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TextureToMesh), true);

    }

    private void OnGUI()
    {
        if (!ui.DrawFields()) return;

        if (!GUILayout.Button("Generate Mesh")) return;

        
        string texturePath = AssetDatabase.GetAssetPath(texture);
        Debug.Log(texturePath);
        Debug.Log(AssetImporter.GetAtPath(texturePath));
        TextureImporter importer = (TextureImporter) AssetImporter.GetAtPath(texturePath);
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.sRGBTexture = true;
        importer.isReadable = true;

        var defaultSettings = importer.GetDefaultPlatformTextureSettings();
        defaultSettings.format = TextureImporterFormat.RGBA32;
        importer.SetPlatformTextureSettings(defaultSettings);
        importer.SaveAndReimport();

        /*MeshFromTexture(texture);*/
   
        
        Mesh mesh = MeshFromTexture(texture);
        if (mesh == null) return;

        AssetDatabase.CreateAsset(mesh, "Assets/Wonderful.mesh");


    }
}

}

#endif
#if UNITY_EDITOR

using Myy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TextureConversion : MonoBehaviour
{
    public Texture2D texture;
    public CustomRenderTexture crt;

    void DumpMetadata(int[] metadata)
    {
        Debug.Log($"{metadata[0]:X} {metadata[1]:X}");
        Debug.Log($"vertices : {metadata[2]:X}");
        Debug.Log($"normals  : {metadata[3]:X}");
        Debug.Log($"uvs      : {metadata[4]:X}");
        Debug.Log($"indices  : {metadata[5]:X}");
    }

    byte[] Color32ToByte(Color32[] colors, int startIndex)
    {
        int nColors = colors.Length;
        int color32ByteSize = (sizeof(byte) * 4);
        int byteSize = (nColors - startIndex) * color32ByteSize;
        byte[] ret = new byte[byteSize];

        for (int c = startIndex, b = 0; c < nColors; b+=4, c++)
        {
            Color32 color = colors[c];
            ret[b + 0] = color.r;
            ret[b + 1] = color.g;
            ret[b + 2] = color.b;
            ret[b + 3] = color.a;
        }

        return ret;
    }

    bool crtInitialised = false;
    bool crtUpdated = false;
    bool allDone = false;

    int i = 0;

    private void Update()
    {
        if (allDone) return;

        if (!crtInitialised)
        {
            crt.material.SetTexture("_Tex", texture);
            RenderTexture.active = crt;
            crt.Initialize();
            RenderTexture.active = null;
            crtInitialised = true;
            return;
        }

        if (!crtUpdated)
        {
            RenderTexture.active = crt;
            crt.Update(1);
            RenderTexture.active = null;
            crtUpdated = true;
            return;
        }

        i++;

        if (i > 120)
        {
            TestTextureConversions();
            allDone = true;
        }

    }

    void TestTextureConversions()
    {

        Color32[] colors = texture.GetPixels32();
        byte[] dataConverted = Color32ToByte(colors, 0);
        byte[] dataDirect = texture.GetRawTextureData();

        Texture2D loadRawResult = new Texture2D(texture.width, texture.height, TextureFormat.RFloat, false, false);
        loadRawResult.LoadRawTextureData(dataConverted);
        loadRawResult.Apply();
        Color[] resultColors = loadRawResult.GetPixels();

        Texture2D crtOutput = new Texture2D(texture.width, texture.height, TextureFormat.RFloat, false, false);


        RenderTexture.active = crt;
        crtOutput.ReadPixels(new Rect(0, 0, crt.width, crt.height), 0, 0);
        AssetDatabase.CreateAsset(crtOutput, "Assets/result.asset");
        RenderTexture.active = null;
        
        Color[] crtColors = crtOutput.GetPixels();

        float dataDirect32_8 = BitConverter.ToSingle(dataDirect, 8 * sizeof(float));
        float dataConverted32_8 = BitConverter.ToSingle(dataConverted, 8 * sizeof(float));
        float resultColor8 = resultColors[8].r;
        float crtColors8 = crtColors[8].r;

        Debug.Log($"Raw Texture data [32] (To float [8]) :\n{dataDirect32_8}");
        Debug.Log($"Color32 to byte [32] (To float [8]) :\n{dataConverted32_8}");
        Debug.Log($"Load Raw Texture Data result (Color[8].r) :\n{resultColor8}");
        Debug.Log($"CRT Colors (Color[8].r) :\n{crtColors8}");

        for (int i = 8; i < crtColors.Length; i++)
        {
            float direct    = BitConverter.ToSingle(dataDirect, i * sizeof(float));
            float converted = BitConverter.ToSingle(dataConverted, i * sizeof(float));
            float result = resultColors[i].r;
            float crtColor = crtColors[i].r;

            if (direct != converted)
            {
                Debug.Log($"[{i}] Direct {direct} != {converted} Converted");
                return;
            }

            if (direct != result)
            {
                Debug.Log($"[{i}] Direct {direct} != {result} Result");
                return;
            }

            if (direct != crtColor)
            {
                Debug.Log($"[{i}] Direct {direct} != {crtColor} CRT Color");
                return;
            }
        }
        Debug.Log("Success !");
        crt.material.SetTexture("_Tex", null);
    }

    private void Start()
    {

    }

}

#endif
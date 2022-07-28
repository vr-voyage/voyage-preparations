﻿#if UNITY_EDITOR
/*
This file is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

*/
using UnityEngine;

using UnityEditor;

using System.Collections;

// Texture Generator 
// Generate special textures that cannot be created easily

namespace Voyage
{
	public class TextureGenerator : ScriptableWizard
	{
		public TextureFormat format;
		public int width;
		public int height;
		public bool mipMap;
		public bool linear;
		public string relativePath;

		[MenuItem("Voyage/Generate Texture")]

		static void CreateWizard()
		{
			ScriptableWizard.DisplayWizard<TextureGenerator>("Texture Generator", "TextureGenerator");
		}

		void OnWizardCreate()
		{
			if (relativePath != null)
            {
				Texture2D tex = new Texture2D(1024, 1024, format, mipMap, linear);
				AssetDatabase.CreateAsset(tex, relativePath);
			}
			
			
		}
	}
}

#endif
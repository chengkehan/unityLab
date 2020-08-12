using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Text;

class ExportSplatmap : EditorWindow
{
    private Terrain terrainObj = null;
    private TerrainData terrain;

    [MenuItem("游戏工具/地形/导出Splatmap")] 
    private static void _ExportObj()
    {
        EditorWindow.GetWindow<ExportSplatmap>().Show();
    }

    void OnGUI()
    {
        terrainObj = EditorGUILayout.ObjectField(terrainObj, typeof(Terrain), true) as Terrain;

        if (terrainObj == null || terrainObj.terrainData == null)
        {
            if (GUILayout.Button("Cancel"))
            {
                EditorWindow.GetWindow<ExportTerrain>().Close();
            }
            return;
        }
        else
        {
            terrain = terrainObj.terrainData;
        }

        if (GUILayout.Button("Export"))
        {
            string fileName = EditorUtility.SaveFilePanel("Export splat file", "", "Splat", "tga");

            if (string.IsNullOrEmpty(fileName) == false)
            {
                int alphaMapsCount = terrain.alphamapTextureCount;

                for (int i = 0; i < alphaMapsCount; i++)
                {
                    Texture2D tex = terrain.GetAlphamapTexture(i);

                    byte[] pngData = tex.EncodeToTGA();
                    if (pngData != null)
                    {
                        File.WriteAllBytes(fileName, pngData);
                    }
                    else
                    {
                        Debug.Log("Could not convert " + tex.name + " to tga. Skipping saving texture.");
                    }
                }

                AssetDatabase.Refresh();

                fileName = fileName.Substring(Application.dataPath.Length);
                fileName = "Assets" + fileName;
                TextureImporter texImporter = AssetImporter.GetAtPath(fileName) as TextureImporter;
                texImporter.textureType = TextureImporterType.Default;
                texImporter.SaveAndReimport();
            }
        }
    }
}
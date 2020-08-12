using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ExportTerrainMaterial : EditorWindow
{
    private Terrain terrainObj = null;
    private TerrainData terrain;

    [MenuItem("游戏工具/地形/导出材质")]
    private static void _ExportObj()
    {
        EditorWindow.GetWindow<ExportTerrainMaterial>().Show();
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
            string fileName = EditorUtility.SaveFilePanel("Export splat file", "", "TerrainMaterial", "mat");

            if (string.IsNullOrEmpty(fileName) == false)
            {
                fileName = fileName.Substring(Application.dataPath.Length);
                fileName = "Assets" + fileName;

                Material mtrl = new Material(Shader.Find("Xslg/Terrain Mesh"));
                TerrainLayer[] layers = terrain.terrainLayers;
                for (int i = 0; i < layers.Length; ++i)
                {
                    TerrainLayer layer = layers[i];
                    string propName = "_Splat" + (i + 1);
                    if (mtrl.HasProperty(propName))
                    {
                        mtrl.SetTexture(propName, layer.diffuseTexture);
                        mtrl.SetTextureScale(propName, new Vector2(1 / layer.tileSize.x * terrain.size.x, 1 / layer.tileSize.y * terrain.size.z));
                    }
                }

                AssetDatabase.CreateAsset(mtrl, fileName);
                AssetDatabase.ImportAsset(fileName);
            }
        }
    }
}

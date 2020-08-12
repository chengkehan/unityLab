using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TerrainMaterialSetting : EditorWindow
{
    [MenuItem("游戏工具/地形/材质设置")]
    private static void Create()
    {
        EditorWindow.CreateWindow<TerrainMaterialSetting>().Show();
    }

    private Material material = null;

    private void OnGUI()
    {
        material = EditorGUILayout.ObjectField(material, typeof(Material), false) as Material;
        if(GUILayout.Button("设置"))
        {
            Terrain[] terrains = Terrain.activeTerrains;
            foreach(var terrain in terrains)
            {
                terrain.materialTemplate = material;
                EditorUtility.SetDirty(terrain);
            }
        }
    }
}

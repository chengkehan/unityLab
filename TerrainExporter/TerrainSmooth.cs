using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TerrainSmooth : EditorWindow
{
    //[MenuItem("游戏工具/地形/平滑")]
    private static void Create()
    {
        EditorWindow.CreateWindow<TerrainSmooth>().Show();
    }

    public MeshFilter[] mfs = null;

    private SerializedObject so = null;
    private SerializedProperty mfsProp = null;

    private void OnEnable()
    {
        so = new SerializedObject(this);
        mfsProp = so.FindProperty("mfs");
    }

    private void OnDisable()
    {
        so = null;
        mfsProp = so.FindProperty("mfs");
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(mfsProp, true);
        if(EditorGUI.EndChangeCheck())
        {
            so.ApplyModifiedProperties();
        }

        if(GUILayout.Button("Smooth"))
        {
            // 平滑两个导出的地形
            // 以后有时间再做
        }
    }
}

﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Text;

enum SaveFormat { Triangles, Quads }
enum SaveResolution { Full = 0, Half, Quarter, Eighth, Sixteenth }

class ExportTerrain : EditorWindow
{
    SaveFormat saveFormat = SaveFormat.Triangles; 
    SaveResolution saveResolution = SaveResolution.Half;

    private Terrain terrainObj = null;
    private TerrainData terrain;
    private Vector3 terrainPos;

    int tCount;
    int counter;
    int totalCount;
    int progressUpdateInterval = 10000;

    [MenuItem("游戏工具/地形/导出obj")]
    private static void _ExportObj()
    {
        EditorWindow.GetWindow<ExportTerrain>().Show();
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
            terrainPos = Vector3.zero;
        }
        //saveFormat = (SaveFormat)EditorGUILayout.EnumPopup("Export Format", saveFormat);

        //saveResolution = (SaveResolution)EditorGUILayout.EnumPopup("Resolution", saveResolution);

        if (GUILayout.Button("Export"))
        {
            string fileName = EditorUtility.SaveFilePanel("Export .obj file", "", "Terrain", "obj");
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            Export(true, fileName);
            AssetDatabase.Refresh();

            fileName = fileName.Substring(Application.dataPath.Length);
            fileName = "Assets" + fileName;
            ModelImporter modelImporter = AssetImporter.GetAtPath(fileName) as ModelImporter;
            modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
            modelImporter.animationType = ModelImporterAnimationType.None;
            modelImporter.importAnimation = false;
            modelImporter.importBlendShapes = false;
            modelImporter.importBlendShapeNormals = ModelImporterNormals.None;
            modelImporter.importVisibility = false;
            modelImporter.importCameras = false;
            modelImporter.importLights = false;
            modelImporter.SaveAndReimport();
        }
    }

    void Export(bool checkIsPurePlane, string fileName)
    {
        int w = terrain.heightmapResolution;
        int h = terrain.heightmapResolution;
        Vector3 meshScale = terrain.size;
        int tRes = (int)Mathf.Pow(2, (int)saveResolution);
        meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
        Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
        float[,] tData = terrain.GetHeights(0, 0, w, h);

        w = (w - 1) / tRes + 1;
        h = (h - 1) / tRes + 1;
        Vector3[] tVertices = new Vector3[w * h];
        Vector2[] tUV = new Vector2[w * h];
        Vector3[] tNormals = new Vector3[w * h];

        int[] tPolys;

        if (saveFormat == SaveFormat.Triangles)
        {
            tPolys = new int[(w - 1) * (h - 1) * 6];
        }
        else
        {
            tPolys = new int[(w - 1) * (h - 1) * 4];
        }

        // Build vertices and UVs
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(-y, tData[x * tRes, y * tRes], x)) + terrainPos;
                tUV[y * w + x] = Vector2.Scale(new Vector2(x * tRes, y * tRes), uvScale);
                tNormals[y * w + x] = Vector3.up;
            }
        }

        if(checkIsPurePlane)
        {
            bool isPurePlane = true;
            float y = tVertices[0].y;
            foreach(var v in tVertices)
            {
                if(Mathf.Abs(v.y - y) > 0.01f)
                {
                    isPurePlane = false;
                    break;
                }
            }
            if(isPurePlane)
            {
                saveResolution = SaveResolution.Sixteenth;
                Export(false, fileName);
                return;
            }
        }

        // 旋转uv
        // 逆时针90度
        Vector2[] tUV2 = new Vector2[tUV.Length];
        System.Array.Copy(tUV, 0, tUV2, 0, tUV.Length);
        for (int pi = 0; pi < h; ++pi)
        {
            for (int pj = 0; pj < w; ++pj)
            {
                Vector2 pc = tUV2[pi * w + pj];
                tUV[(w - pj - 1) * w + pi] = pc;
            }
        }
        System.Array.Copy(tUV, 0, tUV2, 0, tUV.Length);
        // 水平翻转
        for (int pi = 0; pi < h; ++pi)
        {
            for (int pj = 0; pj < w; ++pj)
            {
                Vector2 pc = tUV2[pi * w + pj];
                tUV[(w - pi - 1) * w + pj] = pc;
            }
        }

        int index = 0;
        if (saveFormat == SaveFormat.Triangles)
        {
            // Build triangle indices: 3 indices into vertex array for each triangle
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    // For each grid cell output two triangles
                    tPolys[index++] = (y * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = (y * w) + x + 1;

                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x + 1;
                    tPolys[index++] = (y * w) + x + 1;
                }
            }
        }
        else
        {
            // Build quad indices: 4 indices into vertex array for each quad
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    // For each grid cell output one quad
                    tPolys[index++] = (y * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x + 1;
                    tPolys[index++] = (y * w) + x + 1;
                }
            }
        }

        // Export to .obj
        StreamWriter sw = new StreamWriter(fileName);
        try
        {

            sw.WriteLine("# Unity terrain OBJ File");

            // Write vertices
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            counter = tCount = 0;
            totalCount = (tVertices.Length * 2 + (saveFormat == SaveFormat.Triangles ? tPolys.Length / 3 : tPolys.Length / 4)) / progressUpdateInterval;
            for (int i = 0; i < tVertices.Length; i++)
            {
                UpdateProgress();
                StringBuilder sb = new StringBuilder("v ", 20);
                // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
                // Which is important when you're exporting huge terrains.
                sb.Append(tVertices[i].x.ToString()).Append(" ").
                   Append(tVertices[i].y.ToString()).Append(" ").
                   Append(tVertices[i].z.ToString());
                sw.WriteLine(sb);
            }
            // Write normals
            //for(int i = 0; i < tNormals.Length; i++)
            //{
            //    UpdateProgress();
            //    StringBuilder sb = new StringBuilder("vn ", 22);
            //    sb.Append(tNormals[i].x.ToString()).Append(" ").
            //       Append(tNormals[i].y.ToString()).Append(" ").
            //       Append(tNormals[i].z.ToString());
            //    sw.WriteLine(sb);
            //}
            // Write UVs
            for (int i = 0; i < tUV.Length; i++)
            {
                UpdateProgress();
                StringBuilder sb = new StringBuilder("vt ", 22);
                sb.Append(tUV[i].x.ToString()).Append(" ").
                   Append(tUV[i].y.ToString());
                sw.WriteLine(sb);
            }
            if (saveFormat == SaveFormat.Triangles)
            {
                // Write triangles
                for (int i = 0; i < tPolys.Length; i += 3)
                {
                    UpdateProgress();
                    StringBuilder sb = new StringBuilder("f ", 43);
                    sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                       Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                       Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1);
                    sw.WriteLine(sb);
                }
            }
            else
            {
                // Write quads
                for (int i = 0; i < tPolys.Length; i += 4)
                {
                    UpdateProgress();
                    StringBuilder sb = new StringBuilder("f ", 57);
                    sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                       Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                       Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1).Append(" ").
                       Append(tPolys[i + 3] + 1).Append("/").Append(tPolys[i + 3] + 1);
                    sw.WriteLine(sb);
                }
            }
        }
        catch (Exception err)
        {
            Debug.Log("Error saving file: " + err.Message);
        }
        sw.Close();

        terrain = null;
        EditorUtility.DisplayProgressBar("Saving file to disc.", "This might take a while...", 1f);
        EditorWindow.GetWindow<ExportTerrain>().Close();
        EditorUtility.ClearProgressBar();
    }

    void UpdateProgress()
    {
        if (counter++ == progressUpdateInterval)
        {
            counter = 0;
            EditorUtility.DisplayProgressBar("Saving...", "", Mathf.InverseLerp(0, totalCount, ++tCount));
        }
    }
}
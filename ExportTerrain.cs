using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

enum SaveFormat { Triangles, Quads }
enum SaveResolution { Full = 0, Half, Quarter, Eighth, Sixteenth }

class ExportTerrain : EditorWindow
{
    SaveFormat saveFormat = SaveFormat.Triangles; 
    SaveResolution saveResolution = SaveResolution.Full;

    private Terrain terrainObj = null;
    private TerrainData terrain;
    private Vector3 terrainPos = Vector3.zero;

    List<Terrain> terrainObjList = new List<Terrain>();

    int tCount;
    int counter;
    int totalCount;
    int progressUpdateInterval = 10000;

    private Vector2 scroller = Vector2.zero;

    [MenuItem("游戏工具/地形/导出")]
    private static void _ExportObj()
    {
        EditorWindow.GetWindow<ExportTerrain>().Show();
    }

    void OnGUI()
    {
        //terrainObjList = EditorGUILayout.ObjectField(terrainObjList, typeof(Terrain), true) as Terrain;

        EditorGUI.BeginChangeCheck();
        GameObject target = EditorGUILayout.ObjectField(null, typeof(GameObject), true) as GameObject;
        if (EditorGUI.EndChangeCheck())
        {
            terrainObjList.Clear();
            terrainObjList.AddRange(target.GetComponentsInChildren<Terrain>(false));
        }

        if (terrainObjList.Count == 0)
        {
            return;
        }
        else
        {
            scroller = EditorGUILayout.BeginScrollView(scroller);
            {
                foreach (var terrainObj in terrainObjList)
                {
                    if (terrainObj == null)
                    {
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.ObjectField(terrainObj.gameObject, typeof(GameObject), true);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        //saveFormat = (SaveFormat)EditorGUILayout.EnumPopup("Export Format", saveFormat);
        //saveResolution = (SaveResolution)EditorGUILayout.EnumPopup("Resolution", saveResolution);

        if (GUILayout.Button("Export"))
        {
            string defaultDirectoryKey = "DefaultDirectoryForExportTerrain";
            string defaultDirectory = EditorPrefs.GetString(defaultDirectoryKey, Application.dataPath);
            string path = EditorUtility.SaveFolderPanel(string.Empty, defaultDirectory, string.Empty);
            path = path.Replace("\\", "/");
            if (string.IsNullOrEmpty(path) == false && path.StartsWith(Application.dataPath.Replace("\\", "/"))/*path in the project*/)
            {
                EditorPrefs.SetString(defaultDirectoryKey, path);

                int index = 0;
                foreach (var terrainObj in terrainObjList)
                {
                    ++index;

                    EditorUtility.DisplayProgressBar(string.Empty, index + "/" + terrainObjList.Count, (float)index / terrainObjList.Count);

                    if (terrainObj == null)
                    {
                        continue;
                    }

                    this.terrainObj = terrainObj;
                    this.terrain = terrainObj.terrainData;
                    if (terrain.alphamapTextureCount > 2)
                    {
                        EditorUtility.DisplayDialog(string.Empty, "splatmap数量过多，不支持\n" + terrainObj.name, "ok");
                        Debug.LogError(terrainObj.gameObject, terrainObj.gameObject);
                        continue;
                    }
                    if (terrain.terrainLayers.Length > 8)
                    {
                        EditorUtility.DisplayDialog(string.Empty, "笔刷层数大于8个，不支持\n" + terrainObj.name, "ok");
                        Debug.LogError(terrainObj.gameObject, terrainObj.gameObject);
                        continue;
                    }

                    var thisPath = "Assets" + path.Substring(Application.dataPath.Length);
                    
                    string modelPath = ExportModel(thisPath);
                    List<string> splatPathList = ExportSlpats(thisPath);
                    string materialPath = ExportMaterial(thisPath);
                    AssemblePrefab(thisPath, modelPath, splatPathList, materialPath);
                }

                EditorUtility.ClearProgressBar();
                EditorApplication.ExecuteMenuItem("File/Save Project");
            }
            else
            {
                ShowNotification(new GUIContent("必须保存在项目内"));
            }
        }
    }

    private void AssemblePrefab(string dirPath, string modelPath, List<string> splatPathList, string materialPath)
    {
        Material mtrl = AssetDatabase.LoadMainAssetAtPath(materialPath) as Material;
        for (int splatI = 0; splatI < splatPathList.Count; splatI++)
        {
            mtrl.SetTexture("_Control" + (splatI + 1), AssetDatabase.LoadMainAssetAtPath(splatPathList[splatI]) as Texture);
        }
        EditorUtility.SetDirty(mtrl);

        string prefabPath = dirPath + "/" + terrainObj.gameObject.name + ".prefab";

        AssetDatabase.DeleteAsset(prefabPath);

        GameObject go = GameObject.Instantiate(AssetDatabase.LoadMainAssetAtPath(modelPath)) as GameObject;
        go.transform.localPosition = terrainObj.gameObject.transform.localPosition;
        go.layer = LayerMask.NameToLayer("Terrain");
        go.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Terrain");
        go.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().sharedMaterial = mtrl;
        go.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        go.transform.GetChild(0).gameObject.AddComponent<MeshCollider>();
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        AssetDatabase.ImportAsset(prefabPath);
        GameObject.DestroyImmediate(go);
    }

    private string ExportMaterial(string dirPath)
    {
        string fileName = dirPath + "/" + terrainObj.gameObject.name + "Material.mat";

        Material mtrl = new Material(Shader.Find("Xslg/Terrain Mesh Standard"));
        TerrainLayer[] layers = terrain.terrainLayers;
        for (int i = 0; i < layers.Length; ++i)
        {
            TerrainLayer layer = layers[i];
            string propName = "_Splat" + (i + 1);
            if (mtrl.HasProperty(propName))
            {
                mtrl.SetTexture(propName, layer.diffuseTexture);
                mtrl.SetTexture("_Bump" + (i + 1), layer.normalMapTexture);
                mtrl.SetTextureScale(propName, new Vector2(1 / layer.tileSize.x * terrain.size.x, 1 / layer.tileSize.y * terrain.size.z));
                mtrl.SetVector("_Params" + (i + 1), new Vector4(layer.smoothness, layer.metallic, layer.normalScale, 0));
            }
            else
            {
                Log.Error(propName);
            }
        }

        AssetDatabase.CreateAsset(mtrl, fileName);
        AssetDatabase.ImportAsset(fileName);
        return fileName;
    }

    private List<string> ExportSlpats(string dirPath)
    {
        List<string> pathList = new List<string>();

        int alphaMapsCount = terrain.alphamapTextureCount;
        for (int i = 0; i < alphaMapsCount; i++)
        {
            string fileName = dirPath + "/" + terrainObj.gameObject.name + "Splat" + (i + 1) + ".tga";
            pathList.Add(fileName);

            Texture2D tex = terrain.GetAlphamapTexture(i);
            byte[] pngData = tex.EncodeToTGA();
            File.WriteAllBytes(fileName, pngData);

            AssetDatabase.Refresh();

            TextureImporter texImporter = AssetImporter.GetAtPath(fileName) as TextureImporter;
            texImporter.textureType = TextureImporterType.Default;
            texImporter.anisoLevel = 9;
            texImporter.SaveAndReimport();
        }

        return pathList;
    }

    private string ExportModel(string dirPath)
    {
        string fileName = dirPath + "/" + terrainObj.gameObject.name + "Model.obj";
        Export(fileName);
        AssetDatabase.Refresh();

        ModelImporter modelImporter = AssetImporter.GetAtPath(fileName) as ModelImporter;
        modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
        modelImporter.animationType = ModelImporterAnimationType.None;
        modelImporter.importAnimation = false;
        modelImporter.importBlendShapes = false;
        modelImporter.importBlendShapeNormals = ModelImporterNormals.None;
        modelImporter.importVisibility = false;
        modelImporter.importCameras = false;
        modelImporter.importLights = false;
        modelImporter.importNormals = ModelImporterNormals.Calculate;
        modelImporter.SaveAndReimport();

        return fileName;
    }

    void Export(string fileName)
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
    }
}
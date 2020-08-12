using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ExportTerrainTree : EditorWindow
{
    [MenuItem("游戏工具/地形/导出Tree")]
    private static void _ExportObj()
    {
        EditorWindow.GetWindow<ExportTerrainTree>().Show();
    }

    private Terrain terrainObj = null;
    private TerrainData terrain;

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
            Export();
        }
    }

    private void Export()
    {
        string savedDir = EditorUtility.SaveFolderPanel("Save Terrain Trees", Application.dataPath, "");
        if(string.IsNullOrEmpty(savedDir))
        {
            return;
        }

        TreePrototype[] treePropotypes = terrain.treePrototypes;
        TreeInstance[] treeInstances = terrain.treeInstances;

        Dictionary<GameObject, List<TreeInstance>> groups = new Dictionary<GameObject, List<TreeInstance>>();

        {
            foreach (TreeInstance treeInstance in treeInstances)
            {
                TreePrototype treePrototype = treePropotypes[treeInstance.prototypeIndex];
                if (treePrototype != null && treePrototype.prefab != null && 
                    treePrototype.prefab.GetComponent<MeshFilter>() != null && treePrototype.prefab.GetComponent<MeshFilter>().sharedMesh != null && 
                    treePrototype.prefab.GetComponent<MeshRenderer>() != null && treePrototype.prefab.GetComponent<MeshRenderer>().sharedMaterial != null)
                { 
                    if(treePrototype.prefab.GetComponent<MeshRenderer>().sharedMaterials.Length > 1)
                    {
                        EditorUtility.DisplayDialog(string.Empty, "不支持多维子材质", "ok");
                        return;
                    }

                    List<TreeInstance> group = null;
                    if (!groups.TryGetValue(treePrototype.prefab, out group))
                    {
                        group = new List<TreeInstance>();
                        groups.Add(treePrototype.prefab, group);
                    }
                    group.Add(treeInstance);
                }
            }
        }

        int iGroup = 0;
        foreach(var kv in groups)
        {
            int combinedCount = 0;
            int combinedIndex = 0;
            int combinedLimited = 800;  
            GameObject prefab = kv.Key;
            Mesh prefabMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
            int prefabNumVertices = prefabMesh.vertexCount;
            Vector3[] prefabVertices = prefabMesh.vertices;
            Vector2[] prefabUVs = prefabMesh.uv;
            Vector3[] prefabNormals = prefabMesh.normals;
            int[] prefabTriangles = prefabMesh.triangles;
            List<TreeInstance> instances = kv.Value;
            Mesh combinedMesh = null;
            List<Vector3> vertices = null;
            List<Vector2> uvs = null;
            List<Vector3> normals = null;
            List<Color> colors = null;
            List<int> triangles = null;
            int iInstance = 0;
            foreach(TreeInstance instance in instances)
            {
                if(combinedCount == 0)
                {
                    combinedMesh = new Mesh();
                    combinedMesh.name = "CombinedTrees";
                    vertices = new List<Vector3>();
                    uvs = new List<Vector2>();
                    normals = new List<Vector3>();
                    colors = new List<Color>();
                    triangles = new List<int>();
                }

                Matrix4x4 mat = Matrix4x4.TRS(
                    new Vector3(instance.position.x * terrain.size.x, instance.position.y * terrain.size.y, instance.position.z * terrain.size.z),
                    Quaternion.identity,
                    new Vector3(instance.widthScale * prefab.transform.localScale.x, instance.heightScale * prefab.transform.localScale.y, instance.widthScale * prefab.transform.localScale.z)
                );
                for (int iTri = 0; iTri < prefabTriangles.Length; iTri++)
                {
                    triangles.Add(vertices.Count + prefabTriangles[iTri]);
                }
                for (int iVert = 0; iVert < prefabNumVertices; iVert++)
                {
                    Vector3 vert = mat.MultiplyPoint(prefabVertices[iVert]);
                    vertices.Add(vert);
                    colors.Add(instance.color);
                }
                uvs.AddRange(prefabUVs);
                normals.AddRange(prefabNormals);
                ++combinedCount;

                ++iInstance;

                if (combinedCount == combinedLimited || iInstance == instances.Count)
                {
                    combinedCount = 0;

                    combinedMesh.vertices = vertices.ToArray();
                    combinedMesh.uv = uvs.ToArray();
                    combinedMesh.normals = normals.ToArray();
                    combinedMesh.colors = colors.ToArray();
                    combinedMesh.triangles = triangles.ToArray();
                    combinedMesh.UploadMeshData(false);
                    combinedMesh.RecalculateBounds();

                    Material combinedMaterial = new Material(prefab.GetComponent<MeshRenderer>().sharedMaterial);
                    combinedMaterial.shader = Shader.Find("Xslg/Tree");
                    combinedMaterial.name = combinedMesh.name;

                    GameObject combinedGo = new GameObject();
                    combinedGo.name = combinedMesh.name;

                    string savedPath = savedDir + "/" + combinedMesh.name + "_" + iGroup + "_" + combinedIndex + "_" + (System.DateTime.Now.ToString().Replace(" ", "_").Replace(":", "_").Replace("/", "_")) + ".prefab";
                    GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(combinedGo, savedPath);
                    AssetDatabase.AddObjectToAsset(combinedMesh, savedPrefab);
                    savedPrefab.AddComponent<MeshFilter>().sharedMesh = combinedMesh;
                    AssetDatabase.AddObjectToAsset(combinedMaterial, savedPrefab);
                    savedPrefab.AddComponent<MeshRenderer>().sharedMaterial = combinedMaterial;
                    EditorUtility.SetDirty(savedPrefab);
                    DestroyImmediate(combinedGo);

                    ++combinedIndex;
                }
            }

            ++iGroup;
        }

        AssetDatabase.SaveAssets();
        EditorApplication.ExecuteMenuItem("File/Save Project");
    }
}

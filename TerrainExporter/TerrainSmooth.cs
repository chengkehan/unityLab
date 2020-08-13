using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TerrainSmooth : EditorWindow
{
    [MenuItem("游戏工具/地形/平滑")]
    private static void Create()
    {
        EditorWindow.CreateWindow<TerrainSmooth>().Show();
    }

    public MeshFilter[] mfs = null;
    public float tolerance = 0.01f;

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
        mfsProp = null;
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(mfsProp, true);
        if(EditorGUI.EndChangeCheck())
        {
            so.ApplyModifiedProperties();
        }

        tolerance = EditorGUILayout.Slider("Tolerance", tolerance, 0.0001f, 1f);

        if(GUILayout.Button("Smooth") && mfs != null && mfs.Length > 1)
        {
            Dictionary<MeshFilter, MeshObject> targets = new Dictionary<MeshFilter, MeshObject>();
            foreach(var mf in mfs)
            {
                if(mf != null && mf.sharedMesh != null && targets.ContainsKey(mf) == false)
                {
                    targets.Add(mf, new MeshObject() { mesh = mf.sharedMesh, normals = null });
                }
            }

            if(targets.Count > 1)
            {
                Dictionary<MeshFilter, MeshObject> targets2 = new Dictionary<MeshFilter, MeshObject>(targets);
                foreach(var targetI in targets)
                {
                    MeshFilter mfI = targetI.Key;
                    MeshObject meshI = targetI.Value;
                    Vector3[] verticesI = meshI.mesh.vertices;
                    Vector3[] normalsI = GetNormals(meshI);
                    foreach (var targetJ in targets2)
                    {
                        MeshFilter mfJ = targetJ.Key;
                        MeshObject meshJ = targetJ.Value;
                        if (meshI != meshJ)
                        {
                            Vector3[] verticesJ = meshJ.mesh.vertices;
                            Vector3[] normalsJ = GetNormals(meshJ);
                            for(int vertI = 0; vertI < verticesI.Length; ++vertI)
                            {
                                Vector3 vertexI = mfI.transform.TransformPoint(verticesI[vertI]);
                                for (int vertJ = 0; vertJ < verticesJ.Length; ++vertJ)
                                {
                                    Vector3 vertexJ = mfJ.transform.TransformPoint(verticesJ[vertJ]);
                                    if(Vector3.Distance(vertexI, vertexJ) < tolerance)
                                    {
                                        Vector3 avgNormal = normalsI[vertI];
                                        normalsI[vertI] = avgNormal;
                                        normalsJ[vertJ] = avgNormal;
                                    }
                                }
                            }
                        }
                    }
                }

                foreach(var target in targets)
                {
                    GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(target.Key.gameObject);
                    if(prefab != null)
                    {
                        string prefabPath = AssetDatabase.GetAssetPath(prefab);
                        if (prefabPath.EndsWith(".obj"))
                        {
                            string savedPath = prefabPath.Substring(0, prefabPath.Length - ".obj".Length);
                            savedPath += "_opt.obj";
                            System.IO.File.WriteAllText(savedPath, MeshToObj(target.Key, target.Value));
                            AssetDatabase.ImportAsset(savedPath);
                            ModelImporter modelImporter = AssetImporter.GetAtPath(savedPath) as ModelImporter;
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
                        else
                        {
                            Debug.LogError("Save Smooth Terrain Error", prefab);
                        }
                    }
                    else
                    {
                        Debug.LogError("Save Smooth Terrain Error", target.Key.gameObject);
                    }
                }

                AssetDatabase.Refresh();
            }
        }
    }

    private string MeshToObj(MeshFilter mf, MeshObject meshObj)
    {
        string objStr = "# Unity terrain OBJ File\n";
        foreach(Vector3 vert in meshObj.mesh.vertices)
        {
            objStr += "v " + -vert.x + " " + vert.y + " " + vert.z + "\n";
        }
        foreach (Vector2 uv in meshObj.mesh.uv)
        {
            objStr += "vt " + uv.x + " " + uv.y + "\n";
        }
        foreach(Vector3 n in meshObj.normals)
        {
            objStr += "vn " + -n.x + " " + n.y + " " + n.z + "\n";
        }
        int[] triangles = meshObj.mesh.triangles;
        for(int triI = 0; triI < triangles.Length; triI += 3)
        {
            objStr += "f " + (triangles[triI + 2] + 1) + "/" + (triangles[triI + 2] + 1) + "/" + (triangles[triI + 2] + 1) + " " + 
                             (triangles[triI + 1] + 1) + "/" + (triangles[triI + 1] + 1) + "/" + (triangles[triI + 1] + 1) + " " + 
                             (triangles[triI + 0] + 1) + "/" + (triangles[triI + 0] + 1) + "/" + (triangles[triI + 0] + 1) + "\n";
        }
        return objStr;
    }

    private Vector3[] GetNormals(MeshObject meshObj)
    {
        if(meshObj.normals == null)
        {
            meshObj.normals = meshObj.mesh.normals;
            if (meshObj.normals == null || meshObj.normals.Length != meshObj.mesh.vertexCount)
            {
                meshObj.normals = new Vector3[meshObj.mesh.vertexCount];
                for (int i = 0; i < meshObj.mesh.vertexCount; ++i)
                {
                    meshObj.normals[i] = Vector3.up;
                }
            }
            return meshObj.normals;
        }
        else
        {
            return meshObj.normals;
        }
    }

    private class MeshObject
    {
        public Mesh mesh = null;
        public Vector3[] normals = null;
    }
}

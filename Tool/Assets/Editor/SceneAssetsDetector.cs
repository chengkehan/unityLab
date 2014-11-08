using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class SceneAssetsDetector : EditorWindow
{
    private static SceneAssetsDetector window;

    private Vector2 scroll;

    private Dictionary<Object, ObjectItem> items = new Dictionary<Object, ObjectItem>();

    private ObjectItem expandedItem = null;

    private FLAG flag = FLAG.SHADER;

    [MenuItem("Window/SceneAssetsDetector")]
    public static void CreateWizard()
    {
        window = (SceneAssetsDetector)EditorWindow.GetWindow(typeof(SceneAssetsDetector));
    }

    public void OnGUI()
    {
        GUILayout.BeginVertical();
        {
            EditorGUILayout.HelpBox("探测当前打开的场景中的资源", MessageType.Info, true);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Refresh"))
                {
                    SearchAssetsItems();
                }
                bool isShader = GUILayout.Toggle(flag == FLAG.SHADER, "Shader", EditorStyles.radioButton);
                if (isShader && flag != FLAG.SHADER)
                {
                    flag = FLAG.SHADER;
                    SearchAssetsItems();
                }
                bool isTexture = GUILayout.Toggle(flag == FLAG.TEXTURE, "Texture", EditorStyles.radioButton);
                if (isTexture && flag != FLAG.TEXTURE)
                {
                    flag = FLAG.TEXTURE;
                    SearchAssetsItems();
                }
                bool isMesh = GUILayout.Toggle(flag == FLAG.MESH, "Mesh", EditorStyles.radioButton);
                if (isMesh && flag != FLAG.MESH)
                {
                    flag = FLAG.MESH;
                    SearchAssetsItems();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Button(
                flag == FLAG.SHADER ? "Shader" : 
                flag == FLAG.TEXTURE ? "Texture" : "Mesh", 
                EditorStyles.toolbarPopup
            );

            scroll = EditorGUILayout.BeginScrollView(scroll);
            {
                foreach (var kv in items)
                {
                    if (expandedItem == kv.Value)
                    {
                        GUI.contentColor = Color.green;
                    }
                    else
                    {
                        GUI.contentColor = Color.white;
                    }
                    if (GUILayout.Button((kv.Value.tips == null ? kv.Value.obj.name : kv.Value.obj.name + kv.Value.tips) + "      " + "(" + kv.Value.objects.Count + ")", EditorStyles.toolbarDropDown))
                    {
                        if (expandedItem == kv.Value)
                        {
                            expandedItem = null;
                        }
                        else
                        {
                            expandedItem = kv.Value;
                        }

                    }
                    GUI.contentColor = Color.white;
                    if (expandedItem == kv.Value)
                    {
                        foreach (var obj in kv.Value.objects)
                        {
                            if (GUILayout.Button(obj.ToString(), EditorStyles.miniButton))
                            {
                                Selection.activeGameObject = obj as GameObject;
                            }
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }
        GUILayout.EndVertical();
    }

    private void SearchAssetsItems()
    {
        expandedItem = null;
        items.Clear();
        Renderer[] renderers = Object.FindObjectsOfType<Renderer>();
        foreach (var renderer in renderers)
        {
            Material material = renderer.sharedMaterial;
            if (material == null)
            {
                continue;
            }
            ObjectItem shaderItem = null;
            Shader shader = material.shader;
            if (shader == null)
            {
                continue;
            }
            if (flag == FLAG.SHADER)
            {
                if (!items.TryGetValue(shader, out shaderItem))
                {
                    shaderItem = new ObjectItem();
                    shaderItem.obj = shader;
                    items.Add(shader, shaderItem);
                }
                shaderItem.objects.Add(renderer.gameObject);
            }
            else if(flag == FLAG.TEXTURE)
            {
                int shaderPropertyCount = ShaderUtil.GetPropertyCount(shader);
                for (int shaderPropertyIndex = 0; shaderPropertyIndex < shaderPropertyCount; ++shaderPropertyIndex)
                {
                    if (ShaderUtil.GetPropertyType(shader, shaderPropertyIndex) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        string propertyName = ShaderUtil.GetPropertyName(shader, shaderPropertyIndex);
                        Texture texture = material.GetTexture(propertyName);
                        if (texture != null)
                        {
                            if (!items.TryGetValue(texture, out shaderItem))
                            {
                                shaderItem = new ObjectItem();
                                shaderItem.obj = texture;
                                shaderItem.tips = "      " + texture.width + "x" + texture.height;
                                if (texture is Texture2D)
                                {
                                    shaderItem.tips += "      " + ((Texture2D)texture).format;
                                }
                                items.Add(texture, shaderItem);
                            }
                            shaderItem.objects.Add(renderer.gameObject);
                        }
                    }
                }
            }
            else if (flag == FLAG.MESH)
            {
                Mesh mesh = null;
                MeshFilter meshFilter = renderer.gameObject.GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    if (renderer is SkinnedMeshRenderer)
                    {
                        mesh = ((SkinnedMeshRenderer)renderer).sharedMesh;
                    }
                }
                else
                {
                    mesh = meshFilter.sharedMesh;
                }
                if (mesh != null)
                {
                    if (!items.TryGetValue(mesh, out shaderItem))
                    {
                        shaderItem = new ObjectItem();
                        shaderItem.obj = mesh;
                        shaderItem.tips = "      " + "verts:" + mesh.vertexCount + " " + "tris:" + (mesh.triangles == null ? 0 : mesh.triangles.Length / 3);
                        items.Add(mesh, shaderItem);
                    }
                    shaderItem.objects.Add(renderer.gameObject);
                }
            }
        }
    }

    private class ObjectItem
    {
        public string tips = null;

        public Object obj = null;

        public List<Object> objects = new List<Object>();
    }

    private enum FLAG
    {
        SHADER, 
        TEXTURE, 
        MESH
    }
}

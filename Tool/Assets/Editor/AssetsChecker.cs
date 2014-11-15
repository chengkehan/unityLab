using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Configuration;
using System.Collections.Generic;

public class AssetsChecker : EditorWindow
{
    private static AssetsChecker window;
    [MenuItem("Window/AssetsChecker")]
    public static void CreateWizard()
    {
        window = (AssetsChecker)EditorWindow.GetWindow(typeof(AssetsChecker));
    }

    private enum CheckType
    {
        TEXTURE, 
        VERTEX_AND_FACE, 
        TANGENT, 
        BONE_AMOUNT, 
        BONE_QUALITY
    }

    struct MeshInfo
    {
        public string Path;
        public string MeshName;
        public int Vertexes;
        public int Faces;
    }

    struct PictureInfo
    {
        public string Path;
        public int Width;
        public int Height;
    }

    private Vector2 m_Scroll = Vector2.zero;
    private string htmlstr = "//InfoIndex.html";/*Application.dataPath + "//Editor*/
    private string dirInfoWrite = Application.dataPath + "//Editor//AssetsChecker_InfoIndexRef.html";
    private string temstr;
    private ArrayList Picture = new ArrayList();

    private ArrayList PictureRedWid = new ArrayList();
    private ArrayList PictureGreenWid = new ArrayList();
    private ArrayList PictureYellowWid = new ArrayList();

    private ArrayList PictureRedHei = new ArrayList();
    private ArrayList PictureGreenHei = new ArrayList();
    private ArrayList PictureYellowHei = new ArrayList();

    private ArrayList PictureWidHei = new ArrayList();

    private ArrayList ModelMeshRedVer = new ArrayList();
    private ArrayList ModelMeshGreenVer = new ArrayList();
    private ArrayList ModelMeshYellowVer = new ArrayList();

    private ArrayList ModelMeshRedFac = new ArrayList();
    private ArrayList ModelMeshGreenFac = new ArrayList();
    private ArrayList ModelMeshYellowFac = new ArrayList();

    private ArrayList ModelTangentMsg = new ArrayList();
    private ArrayList ModelBoneMsg = new ArrayList();

    private ArrayList BoneQualityMsg = new ArrayList();

    private List<MeshInfo> meshInfo = new List<MeshInfo>();
    private List<MeshInfo> meshInfoNoSort = new List<MeshInfo>();
    static private MeshInfo temMesh;

    private List<PictureInfo> pictureInfo = new List<PictureInfo>();
    private PictureInfo tempicture;

    private CheckType checkType = CheckType.TEXTURE;
    private string[] checkTypeLabel = { "图片", "顶点&面", "模型切线导入", "骨骼数量", "蒙皮质量" };

    //检索图片宽高
    private bool is_WidHei = false;
    //检索图片乘方
    private bool is_Mizhi = false;
    //检索图片宽度
    private bool is_Width = false;
    //检索图片高度
    private bool is_Height = false;
    //检索网格顶点
    private bool is_Vertex = false;
    //检索网格面
    private bool is_face = false;
    //检测模型tangent导入选项
    private bool is_set_tangent = false;
    private ModelImporterTangentSpaceMode modelTangentOption = ModelImporterTangentSpaceMode.None;
    //检测骨骼
    private bool ignoreZeroBone = true;
    //骨骼质量
    private SkinQuality boneQuality = SkinQuality.Auto;
    private bool is_set_boneQuality = false;

    private int widthWarning;
    private int widthErroring;

    private int heightWarning;
    private int heightErroring;

    private int vertexWarning;
    private int vertexErroring;

    private int faceWarning;
    private int faceErroring;

    private string m_RootDirectory = "";
    private string m_TargetDirectory = "";

    private int progressTotal = 0;
    private int progressCurrent = 0;
    private GameObject progressGo = null;

    private List<FileInfo> allFiles = null;
    // OnGUI每帧会被调用两次，这个变量保证每帧只处理一个文件
    private int progressCount = 0;

    public void OnGUI()
    {
        EditorGUILayout.LabelField("要检测的文件夹:");
        m_TargetDirectory = EditorGUILayout.TextField(m_TargetDirectory);
        GUI.SetNextControlName("Browse");
        if (GUILayout.Button("Browse", GUILayout.Width(60f)))
        {
            string pathTarget = EditorUtility.OpenFolderPanel("Open Target Directory", m_TargetDirectory, "");
            m_TargetDirectory = pathTarget.Length > 0 ? pathTarget : m_TargetDirectory;
            GUI.FocusControl("Browse");
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        checkType = (CheckType)GUILayout.Toolbar((int)checkType, checkTypeLabel);

        if (checkType == CheckType.TEXTURE)
        {
            EditorGUILayout.HelpBox("支持文件名后缀为 PSD、JPG、PNG、TGA", MessageType.Info, true);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("图片幂指数检索：", GUILayout.Width(80f), GUILayout.Height(30f));
            is_Mizhi = EditorGUILayout.Toggle(is_Mizhi, GUILayout.Width(40f), GUILayout.Height(30f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("宽高不相等检索:", GUILayout.Width(80f), GUILayout.Height(30f));
            is_WidHei = EditorGUILayout.Toggle(is_WidHei, GUILayout.Width(40f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            is_Width = EditorGUILayout.Toggle(is_Width, GUILayout.Width(40f), GUILayout.Height(30f));
            GUILayout.Label("图片宽检索：", GUILayout.Width(80f), GUILayout.Height(30f));
            widthWarning = EditorGUILayout.IntField("宽度像素（警告）:", widthWarning);
            widthErroring = EditorGUILayout.IntField("宽度像素（错误）:", widthErroring);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            is_Height = EditorGUILayout.Toggle(is_Height, GUILayout.Width(40f), GUILayout.Height(30f));
            GUILayout.Label("图片高检索：", GUILayout.Width(80f), GUILayout.Height(30f));
            heightWarning = EditorGUILayout.IntField("高度像素（警告）:", heightWarning);
            heightErroring = EditorGUILayout.IntField("高度像素（错误）:", heightErroring);
            EditorGUILayout.EndHorizontal();
        }

        if (checkType == CheckType.VERTEX_AND_FACE)
        {
            EditorGUILayout.HelpBox("支持文件名后缀为 FBX", MessageType.Info, true);

            EditorGUILayout.BeginHorizontal();
            is_Vertex = EditorGUILayout.Toggle(is_Vertex, GUILayout.Width(40f), GUILayout.Height(30f));
            GUILayout.Label("模型顶点检索：", GUILayout.Width(80f), GUILayout.Height(30f));
            vertexWarning = EditorGUILayout.IntField("顶点数（警告）:", vertexWarning);
            vertexErroring = EditorGUILayout.IntField("顶点数（错误）:", vertexErroring);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            is_face = EditorGUILayout.Toggle(is_face, GUILayout.Width(40f), GUILayout.Height(30f));
            GUILayout.Label("模型面检索：", GUILayout.Width(80f), GUILayout.Height(30f));
            faceWarning = EditorGUILayout.IntField("面数（警告）:", faceWarning);
            faceErroring = EditorGUILayout.IntField("面数（错误）:", faceErroring);
            EditorGUILayout.EndHorizontal();
        }

        if (checkType == CheckType.TANGENT)
        {
            EditorGUILayout.HelpBox("支持文件名后缀为 FBX", MessageType.Info, true);

            EditorGUILayout.BeginHorizontal();
            is_set_tangent = EditorGUILayout.Toggle("修改为：", is_set_tangent);
            modelTangentOption = (ModelImporterTangentSpaceMode)EditorGUILayout.EnumPopup(modelTangentOption);
            EditorGUILayout.EndHorizontal();
        }

        if (checkType == CheckType.BONE_AMOUNT)
        {
            EditorGUILayout.HelpBox("支持文件名后缀为 FBX", MessageType.Info, true);

            EditorGUILayout.BeginHorizontal();
            ignoreZeroBone = EditorGUILayout.Toggle("忽略0骨骼", ignoreZeroBone);
            EditorGUILayout.EndHorizontal();
        }

        if (checkType == CheckType.BONE_QUALITY)
        {
            EditorGUILayout.HelpBox("检测 Prefab", MessageType.Info, true);

            EditorGUILayout.BeginHorizontal();
            is_set_boneQuality = EditorGUILayout.Toggle("修改为：", is_set_boneQuality);
            boneQuality = (SkinQuality)EditorGUILayout.EnumPopup(boneQuality);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Go", GUILayout.Width(80f), GUILayout.Height(30f)))
        {
            if (allFiles == null)
            {
                try
                {
                    if (m_TargetDirectory == "")
                    {
                        string str = "设置要检测的文件夹";
                        GUIContent content = new GUIContent(str);
                        window.ShowNotification(content);
                    }

                    if (m_TargetDirectory != "")
                    {
                        allFiles = new List<FileInfo>();
                        GetAllFiles(new DirectoryInfo(m_TargetDirectory));
                        progressTotal = allFiles.Count;
                        progressCurrent = 0;
                        meshInfo.Clear();
                        meshInfoNoSort.Clear();
                        pictureInfo.Clear();
                        PictureRedWid.Clear();
                        PictureGreenWid.Clear();
                        PictureYellowWid.Clear();

                        PictureRedHei.Clear();
                        PictureGreenHei.Clear();
                        PictureYellowHei.Clear();

                        PictureWidHei.Clear();

                        ModelMeshRedVer.Clear();
                        ModelMeshGreenVer.Clear();
                        ModelMeshYellowVer.Clear();

                        ModelMeshRedFac.Clear();
                        ModelMeshGreenFac.Clear();
                        ModelMeshYellowFac.Clear();

                        ModelTangentMsg.Clear();
                        ModelBoneMsg.Clear();

                        BoneQualityMsg.Clear();
                    }
                }

                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    Debug.LogError(e.StackTrace);
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        {
            float progressValue = progressTotal == 0 ? 0 : (float)progressCurrent / (float)progressTotal;
            EditorGUI.ProgressBar(new Rect(5, position.height - 22, position.width - 10, 20), progressValue, progressCurrent + "/" + progressTotal);
        }

        if (allFiles != null)
        {
            if (allFiles.Count > 0)
            {
                if (progressCount >= 1)
                { 
                    progressCount = -1;
                    ProgressCoroutine();
                }
                ++progressCount;
                this.Repaint();
            }
            else
            {
                allFiles = null;
                m_RootDirectory = Application.persistentDataPath;
                string resultPath = m_RootDirectory + htmlstr;
                ConstructOutput();
                ConstructHTML();
                UnityEditor.EditorUtility.OpenWithDefaultApp(resultPath);
            }
        }
    }

    private void ProgressCoroutine()
    {
        FileInfo file = allFiles[allFiles.Count - 1];
        allFiles.RemoveAt(allFiles.Count - 1);
        ++progressCurrent;
        if (file == null)
        {
            return;
        }
        if (!file.Exists)
        {
            return;
        }
        if (file.FullName.Contains("Plugins"))
        {
            return;
        }
        //png图片格式幂指数判断
        string fileExtension = file.Extension.ToLower();
        if ((fileExtension == ".png" || fileExtension == ".jpg" || fileExtension == ".tga" || fileExtension == ".psd") && checkType == CheckType.TEXTURE)
        {
            string filename = Application.dataPath + "/put.png";
            file.CopyTo(filename, true);
            try
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceUncompressedImport);
                UnityEngine.Object ob = AssetDatabase.LoadMainAssetAtPath(filename.Substring(filename.IndexOf("Assets")));
                Texture2D tex = (Texture2D)ob;
                TextureImporter texImporter = (TextureImporter)TextureImporter.GetAtPath(filename.Substring(filename.IndexOf("Assets")));
                TextureImporterSettings s = new TextureImporterSettings();
                s.ApplyTextureType(TextureImporterType.Advanced, true);

                s.mipmapEnabled = false;
                s.maxTextureSize = 4096;
                s.readable = true;
                s.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                texImporter.SetTextureSettings(s);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tex));
                processTexture(tex, file);
            }
            catch (System.Exception exception)
            {
                Debug.LogError(exception.Message);
                Debug.LogError(file.FullName);
            }
            finally
            {
                System.IO.File.Delete(filename);
            }
        }

        //FBX mesh文件顶点和三角型面输出
        if (fileExtension == ".fbx" && (checkType == CheckType.VERTEX_AND_FACE || checkType == CheckType.TANGENT || checkType == CheckType.BONE_AMOUNT))
        {
            if (checkType == CheckType.VERTEX_AND_FACE)
            {
                string filename = Application.dataPath + "/put.FBX";
                file.CopyTo(filename, true);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUncompressedImport);
                UnityEngine.Object obmodel = AssetDatabase.LoadMainAssetAtPath(filename.Substring(filename.IndexOf("Assets")));
                GameObject objFBX = (GameObject)obmodel;
                processModel(objFBX, file);
                System.IO.File.Delete(filename);
            }
            if (checkType == CheckType.TANGENT)
            {
                processModelTangent(file);
            }
            if (checkType == CheckType.BONE_AMOUNT)
            {
                processModelBone(file);
            }
        }

        if (fileExtension == ".prefab")
        {
            if (checkType == CheckType.BONE_QUALITY)
            {
                processBoneQuality(file);
            }
        }
    }

    private void GetAllFiles(FileSystemInfo info)
    {
        if (!info.Exists) return;
        DirectoryInfo dir = info as DirectoryInfo;
        if (dir == null || dir.Name.ToLower() == ".svn")
            return;

        FileSystemInfo[] files = dir.GetFileSystemInfos();
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].FullName.Contains("Plugins"))
            {
                continue;
            }
            FileInfo file = files[i] as FileInfo;
            if (null != file)
            {
                string fileExtension = file.Extension.ToLower();
                if ((fileExtension == ".png" || fileExtension == ".jpg" || fileExtension == ".tga" || fileExtension == ".psd") && checkType == CheckType.TEXTURE)
                {
                    allFiles.Add(file);
                }

                if (fileExtension == ".fbx" && (checkType == CheckType.VERTEX_AND_FACE || checkType == CheckType.TANGENT || checkType == CheckType.BONE_AMOUNT))
                {
                    allFiles.Add(file);
                }
                if (fileExtension == ".prefab")
                {
                    if (checkType == CheckType.BONE_QUALITY)
                    {
                        GameObject go = AssetDatabase.LoadMainAssetAtPath(file.FullName.Substring(file.FullName.IndexOf("Assets"))) as GameObject;
                        if (go != null)
                        {
                            if (go.GetComponent<SkinnedMeshRenderer>() != null)
                            {
                                allFiles.Add(file);
                            }
                            else
                            {
                                SkinnedMeshRenderer[] smrs = go.GetComponentsInChildren<SkinnedMeshRenderer>();
                                if (smrs != null || smrs.Length > 0)
                                {
                                    allFiles.Add(file);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                GetAllFiles(files[i]);
            }
        }
    }

    public void ConstructOutput()
    {
        if (checkType == CheckType.VERTEX_AND_FACE)
        {
            if(is_Vertex)
            {
                for (int i = 0; i < meshInfo.Count; i++)
                {
                    if (meshInfo[i].Vertexes <= vertexWarning)
                    {
                        temstr = "<td>" + meshInfo[i].Path + "</td>" + "<td>" + meshInfo[i].MeshName + "</td>" + "<td>" + meshInfo[i].Vertexes.ToString() + "</td>" + "<td>" + meshInfo[i].Faces.ToString() + "</td>";
                        ModelMeshGreenVer.Add("<tr>" + temstr + "</tr>");
                    }
                    if (meshInfo[i].Vertexes <= vertexErroring && meshInfo[i].Vertexes > vertexWarning)
                    {
                        temstr = "<td>" + meshInfo[i].Path + "</td>" + "<td>" + meshInfo[i].MeshName + "</td>" + "<td>" + meshInfo[i].Vertexes.ToString() + "</td>" + "<td>" + meshInfo[i].Faces.ToString() + "</td>";
                        ModelMeshYellowVer.Add("<tr>" + temstr + "</tr>");
                    }
                    if (meshInfo[i].Vertexes > vertexErroring)
                    {
                        temstr = "<td>" + meshInfo[i].Path + "</td>" + "<td>" + meshInfo[i].MeshName + "</td>" + "<td>" + meshInfo[i].Vertexes.ToString() + "</td>" + "<td>" + meshInfo[i].Faces.ToString() + "</td>";
                        ModelMeshRedVer.Add("<tr>" + temstr + "</tr>");
                    } 
                }
            }


            if (is_face)
            {
                for (int i = 0; i < meshInfo.Count; i++)
                {
                    if (meshInfo[i].Faces <= faceWarning)
                    {
                        temstr = "<td>" + meshInfo[i].Path + "</td>" + "<td>" + meshInfo[i].MeshName + "</td>" + "<td>" + meshInfo[i].Vertexes.ToString() + "</td>" + "<td>" + meshInfo[i].Faces.ToString() + "</td>";
                        ModelMeshGreenFac.Add("<tr>" + temstr + "</tr>");
                    }
                    if (meshInfo[i].Faces <= faceErroring && meshInfo[i].Faces > faceWarning)
                    {
                        temstr = "<td>" + meshInfo[i].Path + "</td>" + "<td>" + meshInfo[i].MeshName + "</td>" + "<td>" + meshInfo[i].Vertexes.ToString() + "</td>" + "<td>" + meshInfo[i].Faces.ToString() + "</td>";
                        ModelMeshYellowFac.Add("<tr>" + temstr + "</tr>");
                    }
                    if (meshInfo[i].Faces > faceErroring)
                    {
                        temstr = "<td>" + meshInfo[i].Path + "</td>" + "<td>" + meshInfo[i].MeshName + "</td>" + "<td>" + meshInfo[i].Vertexes.ToString() + "</td>" + "<td>" + meshInfo[i].Faces.ToString() + "</td>";
                        ModelMeshRedFac.Add("<tr>" + temstr + "</tr>");
                    }
                }
            }
        }
    }

    public void ConstructHTML()
    {
        string htstr = File.ReadAllText(dirInfoWrite);
        string htmlstr = m_RootDirectory + this.htmlstr;
        if (m_RootDirectory == "")
        {
            string str = "Export Directory should not be empty";
            GUIContent content = new GUIContent(str);
            window.ShowNotification(content);
        }
        File.Delete(htmlstr);
        File.AppendAllText(htmlstr, htstr);

        string str0;

        string str3 = "</body>";
        string str4 = "</html>";

        //幂指数检索 
        if (checkType == CheckType.TEXTURE)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "------------------------不符合幂指数-----------------------" + "</p>");
            if (pictureInfo.Count > 0)
            {
                File.AppendAllText(htmlstr, "<table class=\"red\">");
                File.AppendAllText(htmlstr, "<tr><td>路径</td><td>宽x高（像素）</td></tr>");
                for (int i = 0; i < pictureInfo.Count; i++)
                {
                    str0 = "<tr>" + "<td>" + pictureInfo[i].Path.ToString() + "</td>" + "<td>" + pictureInfo[i].Width.ToString() + "x" + pictureInfo[i].Height.ToString() + "</td>" + "</tr>";
                    File.AppendAllText(htmlstr, str0);
                }
                File.AppendAllText(htmlstr, "</table>");
            }
        }

        //图片宽分类
        if (checkType == CheckType.TEXTURE && is_Width)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "------------------------图片宽分类-----------------------" + "</p>");
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "宽 >" + widthErroring.ToString() + "</p>");
            if (PictureRedWid.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有图片在此范围内" + "</p>");
            }
            else
            {
                File.AppendAllText(htmlstr, "<table class=\"red\">");
                File.AppendAllText(htmlstr, "<tr><td>路径</td><td>宽x高（像素）</td></tr>");
                for (int i = 0; i < PictureRedWid.Count; i++)
                {
                    str0 = PictureRedWid[i].ToString();
                    File.AppendAllText(htmlstr, str0);
                }
                File.AppendAllText(htmlstr, "</table>");
            }
        }

        if (checkType == CheckType.TEXTURE && is_Width)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + widthWarning.ToString() + " < 宽 <=" + widthErroring.ToString() + "</p>");
            if (PictureYellowWid.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有图片在此范围内" + "</p>");

            }
            else
            {
                File.AppendAllText(htmlstr, "<table class=\"yellow\">");
                File.AppendAllText(htmlstr, "<tr><td>路径</td><td>宽x高（像素）</td></tr>");
                for (int i = 0; i < PictureYellowWid.Count; i++)
                {

                    str0 = PictureYellowWid[i].ToString();
                    File.AppendAllText(htmlstr, str0);
                }
                File.AppendAllText(htmlstr, "</table>");
            }
        }

        if (checkType == CheckType.TEXTURE && is_Width)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "宽 <=" + widthWarning.ToString() + "</p>");
            if (PictureGreenWid.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有图片在此范围内" + "</p>");

            }
            else
            {
                File.AppendAllText(htmlstr, "<table class=\"green\">");
                File.AppendAllText(htmlstr, "<tr><td>路径</td><td>宽x高（像素）</td></tr>");
                for (int i = 0; i < PictureGreenWid.Count; i++)
                {

                    str0 = PictureGreenWid[i].ToString();
                    File.AppendAllText(htmlstr, str0);
                }
                File.AppendAllText(htmlstr, "</table>");
            }
        }

        //图片高度分类
        if (checkType == CheckType.TEXTURE && is_Height)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "------------------------图片高分类------------------------" + "</p>");
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "高 >" + heightErroring.ToString() + "</p>");
            if (PictureRedHei.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有图片在此范围内" + "</p>");
            }
            else
            {
                File.AppendAllText(htmlstr, "<table class=\"red\">");
                File.AppendAllText(htmlstr, "<tr><td>路径</td><td>宽x高（像素）</td></tr>");
                for (int i = 0; i < PictureRedHei.Count; i++)
                {

                    str0 = PictureRedHei[i].ToString();
                    File.AppendAllText(htmlstr, str0);
                }
                File.AppendAllText(htmlstr, "</table>");
            }
        }

        if (checkType == CheckType.TEXTURE && is_Height)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + heightWarning.ToString() + " < 高 <=" + heightErroring.ToString() + "</p>");
            if (PictureYellowHei.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有图片在此范围内" + "</p>");

            }
            else
            {
                File.AppendAllText(htmlstr, "<table class=\"yellow\">");
                File.AppendAllText(htmlstr, "<tr><td>路径</td><td>宽x高（像素）</td></tr>");
                for (int i = 0; i < PictureYellowHei.Count; i++)
                {

                    str0 = PictureYellowHei[i].ToString();
                    File.AppendAllText(htmlstr, str0);
                }
                File.AppendAllText(htmlstr, "</table>");
            }
        }

        if (checkType == CheckType.TEXTURE && is_Height)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "高 <=" + heightWarning.ToString() + "</p>");
            if (PictureGreenHei.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有图片在此范围内" + "</p>");

            }
            else
            {
                File.AppendAllText(htmlstr, "<table class=\"green\">");
                File.AppendAllText(htmlstr, "<tr><td>路径</td><td>宽x高（像素）</td></tr>");
                for (int i = 0; i < PictureGreenHei.Count; i++)
                {

                    str0 = PictureGreenHei[i].ToString();
                    File.AppendAllText(htmlstr, str0);
                }
                File.AppendAllText(htmlstr, "</table>");
            }
        }

        //宽高不相等检索 
        if (checkType == CheckType.TEXTURE)
        {
            if (is_WidHei)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "------------------------宽高不相等检索 -----------------------" + "</p>");
                if (PictureWidHei.Count > 0)
                {
                    File.AppendAllText(htmlstr, "<table class=\"red\">");
                    File.AppendAllText(htmlstr, "<tr><td>路径</td><td>宽x高（像素）</td></tr>");
                    for (int i = 0; i < PictureWidHei.Count; i++)
                    {
                        str0 = PictureWidHei[i].ToString();
                        File.AppendAllText(htmlstr, str0);
                    } 
                    File.AppendAllText(htmlstr, "</table>");
                }
            }
        }

        //模型顶点分类
        if (checkType == CheckType.VERTEX_AND_FACE && is_Vertex)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "------------------------模型顶点分类------------------------" + "</p>");
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "顶点数 >" + vertexErroring.ToString() + "</p>");
            if (ModelMeshRedVer.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有模型在此范围内" + "</p>");

            }
            else
            {
                File.AppendAllText(htmlstr, "<table class=\"red\">");
                File.AppendAllText(htmlstr, "<tr><td>路径</td><td>GameObject</td><td>顶点数</td><td>面数</td></tr>");
                for (int i = 0; i < ModelMeshRedVer.Count; i++)
                {

                    str0 = ModelMeshRedVer[i].ToString();
                    File.AppendAllText(htmlstr, str0);
                }
                File.AppendAllText(htmlstr, "</table>");
            }
        }

        if (checkType == CheckType.VERTEX_AND_FACE && is_Vertex)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + vertexWarning.ToString() + " < 顶点数 <=" + vertexErroring.ToString() + "</p>");
            if (ModelMeshYellowVer.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有模型在此范围内" + "</p>");

            }
            else
            {
                File.AppendAllText(htmlstr, "<table class=\"yellow\">");
                File.AppendAllText(htmlstr, "<tr><td>路径</td><td>GameObject</td><td>顶点数</td><td>面数</td></tr>");
                for (int i = 0; i < ModelMeshYellowVer.Count; i++)
                {

                    str0 = ModelMeshYellowVer[i].ToString();
                    File.AppendAllText(htmlstr, str0);
                }
                File.AppendAllText(htmlstr, "</table>");
            }
        }

        if (checkType == CheckType.VERTEX_AND_FACE && is_Vertex)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "顶点数 <=" + vertexWarning.ToString() + "</p>");
            if (ModelMeshGreenVer.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有模型在此范围内" + "</p>");

            }
            else
            {
                File.AppendAllText(htmlstr, "<table class=\"green\">");
                File.AppendAllText(htmlstr, "<tr><td>路径</td><td>GameObject</td><td>顶点数</td><td>面数</td></tr>");
                for (int i = 0; i < ModelMeshGreenVer.Count; i++)
                {

                    str0 = ModelMeshGreenVer[i].ToString();
                    File.AppendAllText(htmlstr, str0);
                }
                File.AppendAllText(htmlstr, "</table>");
            }
        }

        //模型面分类
        if (checkType == CheckType.VERTEX_AND_FACE && is_face)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "------------------------模型面分类------------------------" + "</p>");
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "面数 >" + vertexErroring.ToString() + "</p>");
            if (ModelMeshRedFac.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有模型在此范围内" + "</p>");

            }
            else
            {
                File.AppendAllText(htmlstr, "<table class=\"red\">");
                File.AppendAllText(htmlstr, "<tr><td>路径</td><td>GameObject</td><td>顶点数</td><td>面数</td></tr>");
                for (int i = 0; i < ModelMeshRedFac.Count; i++)
                {

                    str0 = ModelMeshRedFac[i].ToString();
                    File.AppendAllText(htmlstr, str0);
                }
                File.AppendAllText(htmlstr, "</table>");
            }
        }

        if (checkType == CheckType.VERTEX_AND_FACE && is_face)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + faceWarning.ToString() + " < 面数 <=" + faceErroring.ToString() + "</p>");
            if (ModelMeshYellowFac.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有模型在此范围内" + "</p>");

            }
            else
            {
                File.AppendAllText(htmlstr, "<table class=\"yellow\">");
                File.AppendAllText(htmlstr, "<tr><td>路径</td><td>GameObject</td><td>顶点数</td><td>面数</td></tr>");
                for (int i = 0; i < ModelMeshYellowFac.Count; i++)
                {

                    str0 = ModelMeshYellowFac[i].ToString();
                    File.AppendAllText(htmlstr, str0);
                }
                File.AppendAllText(htmlstr, "</table>");
            }
        }

        if (checkType == CheckType.VERTEX_AND_FACE && is_face)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "面数 <=" + faceWarning.ToString() + "</p>");
            if (ModelMeshGreenFac.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有模型在此范围内" + "</p>");

            }
            else
            {
                File.AppendAllText(htmlstr, "<table class=\"green\">");
                File.AppendAllText(htmlstr, "<tr><td>路径</td><td>GameObject</td><td>顶点数</td><td>面数</td></tr>");
                for (int i = 0; i < ModelMeshGreenFac.Count; i++)
                {

                    str0 = ModelMeshGreenFac[i].ToString();
                    File.AppendAllText(htmlstr, str0);
                }
                File.AppendAllText(htmlstr, "</table>");
            }
        }

        // 模型切线
        if (checkType == CheckType.TANGENT)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "------------------------模型切线------------------------" + "</p>");
            if (ModelTangentMsg.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有符合的模型" + "</p>");
            }
            else
            {
                File.AppendAllText(htmlstr, "<table class=\"green\">");
                File.AppendAllText(htmlstr, "<tr><td>路径</td><td>切线</td></tr>");
                for (int i = 0; i < ModelTangentMsg.Count; i++)
                {
                    File.AppendAllText(htmlstr, ModelTangentMsg[i].ToString());
                }
                File.AppendAllText(htmlstr, "</table>");
            }
        }

        // 模型骨骼
        if (checkType == CheckType.BONE_AMOUNT)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "------------------------模型骨骼------------------------" + "</p>");
            if (ModelBoneMsg.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有符合的模型" + "</p>");
            }
            else
            {
                File.AppendAllText(htmlstr, "<table class=\"green\">");
                File.AppendAllText(htmlstr, "<tr><td>路径</td><td>骨骼数量</td></tr>");
                for (int i = 0; i < ModelBoneMsg.Count; i++)
                {
                    File.AppendAllText(htmlstr, ModelBoneMsg[i].ToString());
                }
                File.AppendAllText(htmlstr, "</table>");
            }
        }

        // 骨骼质量
        if (checkType == CheckType.BONE_QUALITY)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "------------------------骨骼质量------------------------" + "</p>");
            if (BoneQualityMsg.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有符合的模型" + "</p>");
            }
            else
            {
                File.AppendAllText(htmlstr, "<table class=\"green\">");
                File.AppendAllText(htmlstr, "<tr><td>路径</td><td>GameObject</td><td>骨骼质量</td></tr>");
                for (int i = 0; i < BoneQualityMsg.Count; i++)
                {
                    File.AppendAllText(htmlstr, BoneQualityMsg[i].ToString());
                }
                File.AppendAllText(htmlstr, "</table>");
            }
        }

        AssetDatabase.Refresh();
        //Debug.Log(ModelMeshRedFac.Count + ModelMeshYellowFac.Count+ModelMeshGreenFac.Count);
        File.AppendAllText(htmlstr, str3);
        File.AppendAllText(htmlstr, str4);
    }

    private void processBoneQuality(FileInfo file)
    {
        string goPath = file.FullName.Substring(file.FullName.IndexOf("Assets"));
        GameObject asset = AssetDatabase.LoadMainAssetAtPath(goPath) as GameObject;
        if (asset != null)
        {
            SkinnedMeshRenderer smr = asset.GetComponent<SkinnedMeshRenderer>();
            SkinnedMeshRenderer[] smrs = asset.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (is_set_boneQuality)
            {
                bool reload = false;
                if (smr != null && smr.quality != boneQuality)
                {
                    smr.quality = boneQuality;
                    reload = true;
                }
                if (smrs != null)
                {
                    foreach (var asmr in smrs)
                    {
                        if (asmr.quality != boneQuality)
                        {
                            asmr.quality = boneQuality;
                            reload = true;
                        }
                    }
                }
                if (reload)
                {
                    AssetDatabase.ImportAsset(goPath);
                }
            }
            if (smr != null)
            {
                if (smr.quality == boneQuality)
                {
                    BoneQualityMsg.Add("<tr>" + "<td>" + file.FullName + "</td>" + "<td>" + smr.gameObject.name + "</td>" + "<td>" + smr.quality + "</td>" + "</tr>");
                }
            }
            if (smrs != null)
            {
                foreach (var asmr in smrs)
                {
                    if (asmr.quality == boneQuality)
                    {
                        BoneQualityMsg.Add("<tr>" + "<td>" + file.FullName + "</td>" + "<td>" + asmr.gameObject.name + "</td>" + "<td>" + asmr.quality + "</td>" + "</tr>");
                    }
                }
            }
        }
    }

    private void processModelBone(FileInfo file)
    {
        string fbxPath = file.FullName.Substring(file.FullName.IndexOf("Assets"));
        GameObject asset = AssetDatabase.LoadMainAssetAtPath(fbxPath) as GameObject;
        if (asset != null)
        {
            SkinnedMeshRenderer smr = asset.GetComponent<SkinnedMeshRenderer>();
            SkinnedMeshRenderer[] smrs = asset.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            int count = 0;
            if (smr != null)
            {
                count += smr.bones == null ? 0 : smr.bones.Length;
            }
            foreach (var asmr in smrs)
            {
                count += asmr.bones == null ? 0 : asmr.bones.Length;
            }
            if (count != 0 || !ignoreZeroBone)
            {
                ModelBoneMsg.Add("<tr>" + "<td>" + file.FullName + "</td>" + "<td>" + count + "</td>" + "</tr>");
            }
        }
    }

    private void processModelTangent(FileInfo file)
    {
        string fbxPath = file.FullName.Substring(file.FullName.IndexOf("Assets"));
        ModelImporter modelImporter = (ModelImporter)ModelImporter.GetAtPath(fbxPath);
        if(is_set_tangent)
        {
            if (modelImporter.tangentImportMode != modelTangentOption)
            {
                modelImporter.tangentImportMode = modelTangentOption;
                AssetDatabase.ImportAsset(fbxPath);
            }
        }
        if (modelImporter.tangentImportMode == modelTangentOption)
        {
            ModelTangentMsg.Add("<tr>" + "<td>" + file.FullName + "</td>" + "<td>" + modelTangentOption + "</td>" + "</tr>");
        }
    }

    private void processModel(GameObject g, FileInfo file)
    {
        Mesh mesh = null;
        if (null != g.GetComponent<SkinnedMeshRenderer>())
        {
            mesh = g.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        }
        if (null == mesh)
        {
            if (null != g.GetComponent<MeshFilter>())
            {
                mesh = g.GetComponent<MeshFilter>().sharedMesh;
            }
        }
        if (null != mesh)
        {
            int verNum;
            int[] faces;
            //if (mesh.isReadable)
            //    faces = mesh.triangles;
            //else
            //{
            //    Debug.Log("不可读写不可读写");
            //    return;
            //}
            verNum = mesh.vertexCount;
            faces = mesh.triangles;

            int faceNum = faces.Length / 3;
            //Debug.Log("FBX路径: " + file.FullName + "\t" + "  网格: " + mesh.name + "\t" + "  顶点数: " + verNum.ToString() + "\t" + "  面数: " + faceNum.ToString());
            temstr = file.FullName + "\t" + "  Mesh: " + mesh.name + "\t" + "  Vertexs: " + verNum.ToString() + "\t" + "  Faces: " + faceNum.ToString();
            temMesh.Path = file.FullName;
            temMesh.MeshName = mesh.name;
            temMesh.Vertexes = verNum;
            temMesh.Faces = faceNum;
            meshInfo.Add(temMesh);
            meshInfoNoSort.Add(temMesh);
        }

        if (null == mesh)
        {
            //Debug.Log("Skinned Mesh" + "\t" + file.FullName);
            for (int i = 0; i < g.transform.childCount; i++)
            {
                Mesh meshTem = null;
                if (null != g.transform.GetChild(i).gameObject.GetComponent<SkinnedMeshRenderer>())
                {
                    meshTem = g.transform.GetChild(i).gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
                }

                if (null == meshTem)
                {
                    if (null != g.transform.GetChild(i).gameObject.GetComponent<MeshFilter>())
                    {
                        meshTem = g.transform.GetChild(i).gameObject.GetComponent<MeshFilter>().sharedMesh;
                    }
                }
                if (null == meshTem)
                    continue;
                processModel(g.transform.GetChild(i).gameObject, file);
            }
        }
    }

    private void processTexture(Texture2D tex, FileInfo file)
    {
        
        int w = tex.width;
        int h = tex.height;
       
        bool hasoutput = false;

        if (is_WidHei)
        {
            if (w != h)
            {
                string WHtemstr = "<tr>" + "<td>" + file.FullName + "</td>" + "<td>" + tex.width.ToString() + "x" + tex.height.ToString() + "</td>" + "</tr>";
                PictureWidHei.Add(WHtemstr);
            }
        }

        if(is_Width)
        {
            if (w > widthErroring)
            {
                string Wtemstr = "<tr>" + "<td>" + file.FullName + "</td>" + "<td>" + tex.width.ToString() + "x" + tex.height.ToString() + "</td>" + "</tr>";
                PictureRedWid.Add(Wtemstr);
            }

            if (w <= widthErroring && w > widthWarning)
            {
                string Wtemstr = "<tr>" + "<td>" + file.FullName + "</td>" + "<td>" + tex.width.ToString() + "x" + tex.height.ToString() + "</td>" + "</tr>";
                PictureYellowWid.Add(Wtemstr);
            }

            if (w <= widthWarning)
            {
                string Wtemstr = "<tr>" + "<td>" + file.FullName + "</td>" + "<td>" + tex.width.ToString() + "x" + tex.height.ToString() + "</td>" + "</tr>";
                PictureGreenWid.Add(Wtemstr);
            }
        }
        ////////////////
        if (is_Height)
        {
            if (h > heightErroring)
            {
                string Wtemstr = "<tr>" + "<td>" + file.FullName + "</td>" + "<td>" + tex.width.ToString() + "x" + tex.height.ToString() + "</td>" + "</tr>";
                PictureRedHei.Add(Wtemstr);
            }

            if (h <= heightErroring && h > heightWarning)
            {
                string Wtemstr = "<tr>" + "<td>" + file.FullName + "</td>" + "<td>" + tex.width.ToString() + "x" + tex.height.ToString() + "</td>" + "</tr>";
                PictureYellowHei.Add(Wtemstr);
            }

            if (h <= heightWarning)
            {
                string Wtemstr = "<tr>" + "<td>" + file.FullName + "</td>" + "<td>" + tex.width.ToString() + "x" + tex.height.ToString() + "</td>" + "</tr>";
                PictureGreenHei.Add(Wtemstr);
            }
        }

        if (is_Mizhi)
        {
            while (w > 2)
            {
                if (w % 2 != 0)
                {
                    temstr = "<tr>" + "<td>" + file.FullName + "</td>" + "<td>" + tex.width.ToString() + "x" + tex.height.ToString() + "</td>" + "</tr>";
                    Picture.Add(temstr);
                    tempicture.Path = file.FullName;
                    tempicture.Width = tex.width;
                    tempicture.Height = tex.height;
                    pictureInfo.Add(tempicture);
                    
                    hasoutput = true;
                    break;
                }
                w /= 2;
                if (w == 2)
                {
                    break;
                }
                if (w == 1)
                {
                    temstr = "<tr>" + "<td>" + file.FullName + "</td>" + "<td>" + tex.width.ToString() + "x" + tex.height.ToString() + "</td>" + "</tr>";
                    Picture.Add(temstr);
                    tempicture.Path = file.FullName;
                    tempicture.Width = tex.width;
                    tempicture.Height = tex.height;
                    pictureInfo.Add(tempicture);
                   
                    hasoutput = true;
                    break;
                }
            }

            while (h > 2)
            {
                if (h % 2 != 0)
                {
                    if (!hasoutput)
                    {
                        temstr = "<tr>" + "<td>" + file.FullName + "</td>" + "<td>" + tex.width.ToString() + "x" + tex.height.ToString() + "</td>" + "</tr>";
                        tempicture.Path = file.FullName;
                        tempicture.Width = tex.width;
                        tempicture.Height = tex.height;
                        pictureInfo.Add(tempicture);
                        Picture.Add(temstr);
                    }
                    break;
                }
                h /= 2;
                if (h == 2)
                {
                    break;
                }
                if (h == 1)
                {
                    if (!hasoutput)
                    {
                        temstr = "<tr>" + "<td>" + file.FullName + "</td>" + "<td>" + tex.width.ToString() + "x" + tex.height.ToString() + "</td>" + "</tr>";
                        Picture.Add(temstr);
                        tempicture.Path = file.FullName;
                        tempicture.Width = tex.width;
                        tempicture.Height = tex.height;
                        pictureInfo.Add(tempicture);
                        
                    }
                    break;
                }
            }
        }
    }
}

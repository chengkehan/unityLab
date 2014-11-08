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

    const string notification = "1.Function: Show picture and mesh infomation"
        + "\n2.Operation: Import the package to project, then select configuration"
        + "\n3.Save: select directory, save information as HTML";

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


    private List<MeshInfo> meshInfo = new List<MeshInfo>();
    private List<MeshInfo> meshInfoNoSort = new List<MeshInfo>();
    static private MeshInfo temMesh;

    private List<PictureInfo> pictureInfo = new List<PictureInfo>();
    private PictureInfo tempicture;

    //检索网格
    private bool is_Mesh = false;
    //检索图片
    private bool is_Picture = false;
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
        EditorGUILayout.HelpBox(notification, MessageType.Info, true);
        EditorGUILayout.LabelField("要检测的文件夹:");
        m_TargetDirectory = EditorGUILayout.TextField(m_TargetDirectory);
        GUI.SetNextControlName("Browse");
        if (GUILayout.Button("Browse", GUILayout.Width(60f)))
        {
            string pathTarget = EditorUtility.OpenFolderPanel("Open Target Directory", m_TargetDirectory, "");
            m_TargetDirectory = pathTarget.Length > 0 ? pathTarget : m_TargetDirectory;
            GUI.FocusControl("Browse");
        }
        is_Picture = EditorGUILayout.BeginToggleGroup("图片:", is_Picture);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("图片幂指数检索：", GUILayout.Width(80f), GUILayout.Height(30f));
        Texture2D image = new Texture2D(30, 30);
        Color color = new Color(255, 255, 0);
        image.SetPixel(20, 20, color);
        GUILayout.Box(image, GUILayout.Width(30), GUILayout.Height(30f));
        is_Mizhi = EditorGUILayout.Toggle(is_Mizhi, GUILayout.Width(40f), GUILayout.Height(30f));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        is_Width = EditorGUILayout.Toggle(is_Width, GUILayout.Width(40f), GUILayout.Height(30f));
        GUILayout.Label("图片宽检索：", GUILayout.Width(80f), GUILayout.Height(30f));
        widthWarning = EditorGUILayout.IntField("widthWarning:", widthWarning);
        widthErroring = EditorGUILayout.IntField("widthErroring:", widthErroring);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        is_Height = EditorGUILayout.Toggle(is_Height, GUILayout.Width(40f), GUILayout.Height(30f));
        GUILayout.Label("图片高检索：", GUILayout.Width(80f), GUILayout.Height(30f));
        heightWarning = EditorGUILayout.IntField("heightWarning:", heightWarning);
        heightErroring = EditorGUILayout.IntField("heightErroring:", heightErroring);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("宽高不相等检索:", GUILayout.Width(80f), GUILayout.Height(30f));
        is_WidHei = EditorGUILayout.Toggle(is_WidHei, GUILayout.Width(40f));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndToggleGroup();

        is_Mesh = EditorGUILayout.BeginToggleGroup("模型:", is_Mesh);

        EditorGUILayout.BeginHorizontal();
        is_Vertex = EditorGUILayout.Toggle(is_Vertex, GUILayout.Width(40f), GUILayout.Height(30f));
        GUILayout.Label("模型顶点检索：", GUILayout.Width(80f), GUILayout.Height(30f));
        vertexWarning = EditorGUILayout.IntField("vertexWarning:", vertexWarning);
        vertexErroring = EditorGUILayout.IntField("vertexErroring:", vertexErroring);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        is_face = EditorGUILayout.Toggle(is_face, GUILayout.Width(40f), GUILayout.Height(30f));
        GUILayout.Label("模型面检索：", GUILayout.Width(80f), GUILayout.Height(30f));
        faceWarning = EditorGUILayout.IntField("faceWarning:", faceWarning);
        faceErroring = EditorGUILayout.IntField("faceErroring:", faceErroring);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndToggleGroup();
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Go", GUILayout.Width(80f), GUILayout.Height(30f)))
        {
            if (allFiles == null)
            {
                try
                {
                    if (m_TargetDirectory == "")
                    {
                        string str = "Target Directory should not be empty";
                        GUIContent content = new GUIContent(str);
                        window.ShowNotification(content);
                    }
                    if (false == is_Picture && false == is_Mesh)
                    {
                        string str = "Should select one of the Picture and Mesh to be inspected ";
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

        EditorGUILayout.BeginHorizontal();
        {
            float progressValue = progressTotal == 0 ? 0 : (float)progressCurrent / (float)progressTotal;
            EditorGUI.ProgressBar(new Rect(5, 390, 520, 20), progressValue, progressCurrent + "/" + progressTotal);
        } 
        EditorGUILayout.EndHorizontal();

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
        if ((fileExtension == ".png" || fileExtension == ".jpg" || fileExtension == ".tga" || fileExtension == ".psd") && is_Picture)
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
        if (fileExtension == ".fbx" && is_Mesh)
        {
            string filename = Application.dataPath + "/put.FBX";
            file.CopyTo(filename, true);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUncompressedImport);
            UnityEngine.Object obmodel = AssetDatabase.LoadMainAssetAtPath(filename.Substring(filename.IndexOf("Assets")));
            GameObject objFBX = (GameObject)obmodel;
            processModel(objFBX, file);
            System.IO.File.Delete(filename);
        }

        if (fileExtension == ".unity3d")
        {
            if (file.FullName.Contains("android"))
                return;
            if (file.FullName.Contains("iPhone"))
                return;
            if (file.FullName.Contains("wp8"))
                return;

            string filename = Application.dataPath + "/put.unity3d";
            System.IO.File.Copy(file.FullName, filename, true);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUncompressedImport);
            GameObject go = new GameObject();
            go.AddComponent<MonoBehaviour>().StartCoroutine(ParseU3D(filename, go, file));
            System.IO.File.Delete(filename);
        }
    }

    private void GetAllFiles(FileSystemInfo info)
    {
        if (!info.Exists) return;
        DirectoryInfo dir = info as DirectoryInfo;
        if (dir == null)
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
                if ((fileExtension == ".png" || fileExtension == ".jpg" || fileExtension == ".tga" || fileExtension == ".psd") && is_Picture)
                {
                    allFiles.Add(file);
                }

                if (fileExtension == ".fbx" && is_Mesh)
                {
                    allFiles.Add(file);
                }

                if (fileExtension == ".unity3d")
                {
                    if (file.FullName.Contains("android"))
                        continue;
                    if (file.FullName.Contains("iPhone"))
                        continue;
                    if (file.FullName.Contains("wp8"))
                        continue;

                    allFiles.Add(file);
                }
            }
            else
            {
                GetAllFiles(files[i]);
            }
        }
    }

    //检索Unity3D
    public IEnumerator ParseU3D(string filename, GameObject go, FileInfo file)
    {
        WWW www = new WWW("file://" + filename);
        yield return www;
        UnityEngine.Object[] assets = www.assetBundle.LoadAll();
        foreach (UnityEngine.Object asset in assets)
        {
            if (asset is Texture2D && is_Picture)
            {
                Texture2D tex = (Texture2D)asset;
                
                processU3DTexture(tex, file);
            }

            if (asset is GameObject && is_Mesh)
            {
                GameObject objFBX = (GameObject)asset;
                processU3DModel(objFBX, file);
            }
        }

        www.assetBundle.Unload(false);
        www.Dispose();
        UnityEngine.GameObject.DestroyImmediate(go);
        System.IO.File.Delete(filename);
    }

    public void ConstructOutput()
    {
        if (is_Mesh)
        {
            if(is_Vertex)
            {
                for (int i = 0; i < meshInfo.Count; i++)
                {
                    if (meshInfo[i].Vertexes <= vertexWarning)
                    {
                        temstr = "Mesh path: " + meshInfo[i].Path + "\t" + "  Mesh: " + meshInfo[i].MeshName + "\t" + "  Vertexs: " + meshInfo[i].Vertexes.ToString() + "\t" + "  Faces: " + meshInfo[i].Faces.ToString();
                        ModelMeshGreenVer.Add(temstr);
                    }
                    if (meshInfo[i].Vertexes <= vertexErroring && meshInfo[i].Vertexes > vertexWarning)
                    {
                        temstr = "Mesh path: " + meshInfo[i].Path + "\t" + "  Mesh: " + meshInfo[i].MeshName + "\t" + "  Vertexs: " + meshInfo[i].Vertexes.ToString() + "\t" + "  Faces: " + meshInfo[i].Faces.ToString();
                        ModelMeshYellowVer.Add(temstr);
                    }
                    if (meshInfo[i].Vertexes > vertexErroring)
                    {
                        temstr = "Mesh path: " + meshInfo[i].Path + "\t" + "  Mesh: " + meshInfo[i].MeshName + "\t" + "  Vertexs: " + meshInfo[i].Vertexes.ToString() + "\t" + "  Faces: " + meshInfo[i].Faces.ToString();
                        ModelMeshRedVer.Add(temstr);
                    }
                }
            }


            if (is_face)
            {
                for (int i = 0; i < meshInfo.Count; i++)
                {
                    if (meshInfo[i].Faces <= faceWarning)
                    {
                        temstr = "Mesh path: " + meshInfo[i].Path + "\t" + "  Mesh: " + meshInfo[i].MeshName + "\t" + "  Vertexs: " + meshInfo[i].Vertexes.ToString() + "\t" + "  Faces: " + meshInfo[i].Faces.ToString();
                        ModelMeshGreenFac.Add(temstr);
                    }
                    if (meshInfo[i].Faces <= faceErroring && meshInfo[i].Faces > faceWarning)
                    {
                        temstr = "Mesh path: " + meshInfo[i].Path + "\t" + "  Mesh: " + meshInfo[i].MeshName + "\t" + "  Vertexs: " + meshInfo[i].Vertexes.ToString() + "\t" + "  Faces: " + meshInfo[i].Faces.ToString();
                        ModelMeshYellowFac.Add(temstr);
                    }
                    if (meshInfo[i].Faces > faceErroring)
                    {
                        temstr = "Mesh path: " + meshInfo[i].Path + "\t" + "  Mesh: " + meshInfo[i].MeshName + "\t" + "  Vertexs: " + meshInfo[i].Vertexes.ToString() + "\t" + "  Faces: " + meshInfo[i].Faces.ToString();
                        ModelMeshRedFac.Add(temstr);
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
        string str1 = "<p class=\"white\">" + "图片:" + "</p>";
        string str2 = "<p class=\"white\">" + "模型:" + "</p>";

        string str3 = "</body>";
        string str4 = "</html>";

        if (false == is_Mesh && false == is_Picture)
        {
            File.Delete(htmlstr);
            string NullStr = "<p class=\"red\">" + "There is nothing to output! Retry..." + "</p>";
            File.AppendAllText(htmlstr, NullStr);
            File.AppendAllText(htmlstr, str3);
            File.AppendAllText(htmlstr, str4);
            return;
        }

        //string maodianmizhi = "<a href = \"#001\">" + "mizhi  " + "</a>";
        string maodianwidth = "<font color=\"red\"><a href = \"#002\">" + "InspectPicWidth       " + "&nbsp" + "</a>";
        string maodianheight = "<a href = \"#003\">" + "InspectPicHeight       " + "</a>";
        string maodianwidhei = "<a href = \"#004\">" + "InspectPicWidhei       " + "</a>";
        string maodianvertex = "<a href = \"#005\">" + "InspectMeshVertex       " + "</a>";
        string maodianface = "<a href = \"#006\">" + "InspectMeshFace       " + "</a></font>";

        string maodianTop = "<p align = \"right\">" + "<a href = \"#007\">" + "Top" + "</a>" + "</p>";


        //string maodianMiz = "<a name = \"001\" id = \"001\">" + "&nbsp </a>";
        string maodianWid = "<a name = \"002\" id = \"002\">" + "&nbsp </a>";
        string maodianHei = "<a name = \"003\" id = \"003\">" + "&nbsp </a>";
        string maodianWH = "<a name = \"004\" id = \"004\">" + "&nbsp </a>";
        string maodianVer = "<a name = \"005\" id = \"005\">" + "&nbsp </a>";
        string maodianFac = "<a name = \"006\" id = \"006\">" + "&nbsp </a>";

        string maodianT = "<a name = \"007\" id = \"007\">" + "&nbsp </a>";

        File.AppendAllText(htmlstr, maodianT);
        File.AppendAllText(htmlstr, maodianwidth);
        File.AppendAllText(htmlstr, maodianheight);
        File.AppendAllText(htmlstr, maodianwidhei);
        File.AppendAllText(htmlstr, maodianvertex);
        File.AppendAllText(htmlstr, maodianface);
        File.AppendAllText(htmlstr, str1);

        //幂指数检索 
        File.AppendAllText(htmlstr, "<p class=\"white\">" + "------------------------不符合幂指数-----------------------" + "</p>");
        if (is_Picture)
        {
            for (int i = 0; i < pictureInfo.Count; i++)
            {
                str0 = "<p class=\"white\">" + pictureInfo[i].Path.ToString() + "\t" + pictureInfo[i].Width.ToString() + "\t" + pictureInfo[i].Height.ToString() + "</p>";
                File.AppendAllText(htmlstr, str0);
            }
        }
        Debug.Log(pictureInfo.Count);
        //图片宽分类
        File.AppendAllText(htmlstr, maodianWid);
        File.AppendAllText(htmlstr, maodianTop);
        File.AppendAllText(htmlstr, "<p class=\"white\">" + "------------------------图片宽分类-----------------------" + "</p>");
        
        if (is_Picture && is_Width)
        {

            File.AppendAllText(htmlstr, "<p class=\"white\">" + "宽 >" + widthErroring.ToString() + "</p>");
            if (PictureRedWid.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有图片在此范围内" + "</p>");
            }
            for (int i = 0; i < PictureRedWid.Count; i++)
            {
                str0 = "<p class=\"red\">" + PictureRedWid[i].ToString() + "</p>";
                File.AppendAllText(htmlstr, str0);
            }
        }

        if (is_Picture && is_Width)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + widthWarning.ToString() + " < 宽 <=" + widthErroring.ToString() + "</p>");
            if (PictureYellowWid.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有图片在此范围内" + "</p>");

            }
            for (int i = 0; i < PictureYellowWid.Count; i++)
            {

                str0 = "<p class=\"yellow\">" + PictureYellowWid[i].ToString() + "</p>";
                File.AppendAllText(htmlstr, str0);
            }
        }

        if (is_Picture && is_Width)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "宽 <=" + widthWarning.ToString() + "</p>");
            if (PictureGreenWid.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有图片在此范围内" + "</p>");

            }
            for (int i = 0; i < PictureGreenWid.Count; i++)
            {

                str0 = "<p class=\"green\">" + PictureGreenWid[i].ToString() + "</p>";
                File.AppendAllText(htmlstr, str0);
            }
        }
        Debug.Log( PictureRedWid.Count + PictureYellowWid.Count + PictureGreenWid.Count);
        //图片高度分类
        File.AppendAllText(htmlstr, maodianHei);
        File.AppendAllText(htmlstr, maodianTop);
        File.AppendAllText(htmlstr, "<p class=\"white\">" + "------------------------图片高分类------------------------" + "</p>");

        if (is_Picture && is_Height)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "高 >" + heightErroring.ToString() + "</p>");
            if (PictureRedHei.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有图片在此范围内" + "</p>");

            }
            for (int i = 0; i < PictureRedHei.Count; i++)
            {

                str0 = "<p class=\"red\">" + PictureRedHei[i].ToString() + "</p>";
                File.AppendAllText(htmlstr, str0);
            }
        }

        if (is_Picture && is_Height)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + heightWarning.ToString() + " < 高 <=" + heightErroring.ToString() + "</p>");
            if (PictureYellowHei.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有图片在此范围内" + "</p>");

            }
            for (int i = 0; i < PictureYellowHei.Count; i++)
            {

                str0 = "<p class=\"yellow\">" + PictureYellowHei[i].ToString() + "</p>";
                File.AppendAllText(htmlstr, str0);
            }
        }

        if (is_Picture && is_Height)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "高 <=" + heightWarning.ToString() + "</p>");
            if (PictureGreenHei.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有图片在此范围内" + "</p>");

            }
            for (int i = 0; i < PictureGreenHei.Count; i++)
            {

                str0 = "<p class=\"green\">" + PictureGreenHei[i].ToString() + "</p>";
                File.AppendAllText(htmlstr, str0);
            }
        }
        Debug.Log(PictureRedHei.Count + PictureYellowHei.Count + PictureGreenHei.Count);
        //宽高不相等检索 
        File.AppendAllText(htmlstr, maodianWH);
        File.AppendAllText(htmlstr, maodianTop);

        if (is_Picture)
        {
            if (is_WidHei)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "------------------------宽高不相等检索 -----------------------" + "</p>");
                for (int i = 0; i < PictureWidHei.Count; i++)
                {
                    str0 = "<p class=\"red\">" + PictureWidHei[i].ToString() + "</p>";
                    File.AppendAllText(htmlstr, str0);
                }
            }
        }

        Debug.Log(PictureWidHei.Count);
        File.AppendAllText(htmlstr, str2);
        //模型顶点分类
        File.AppendAllText(htmlstr, maodianVer);
        File.AppendAllText(htmlstr, maodianTop);
        File.AppendAllText(htmlstr, "<p class=\"white\">" + "------------------------模型顶点分类------------------------" + "</p>");
        if (is_Mesh && is_Vertex)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "顶点数 >" + vertexErroring.ToString() + "</p>");
            if (ModelMeshRedVer.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有模型在此范围内" + "</p>");

            }
            for (int i = 0; i < ModelMeshRedVer.Count; i++)
            {

                str0 = "<p class=\"red\">" + ModelMeshRedVer[i].ToString() + "</p>";
                File.AppendAllText(htmlstr, str0);
            }
        }

        if (is_Mesh && is_Vertex)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + vertexWarning.ToString() + " < 顶点数 <=" + vertexErroring.ToString() + "</p>");
            if (ModelMeshYellowVer.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有模型在此范围内" + "</p>");

            }
            for (int i = 0; i < ModelMeshYellowVer.Count; i++)
            {

                str0 = "<p class=\"yellow\">" + ModelMeshYellowVer[i].ToString() + "</p>";
                File.AppendAllText(htmlstr, str0);
            }
        }

        if (is_Mesh && is_Vertex)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "顶点数 <=" + vertexWarning.ToString() + "</p>");
            if (ModelMeshGreenVer.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有模型在此范围内" + "</p>");

            }
            for (int i = 0; i < ModelMeshGreenVer.Count; i++)
            {

                str0 = "<p class=\"green\">" + ModelMeshGreenVer[i].ToString() + "</p>";
                File.AppendAllText(htmlstr, str0);
            }
        }
        Debug.Log(ModelMeshRedVer.Count + ModelMeshYellowVer.Count + ModelMeshGreenVer.Count);
        //模型面分类
        File.AppendAllText(htmlstr, maodianFac);
        File.AppendAllText(htmlstr, maodianTop);
        File.AppendAllText(htmlstr, "<p class=\"white\">" + "------------------------模型面分类------------------------" + "</p>");
        if (is_Mesh && is_face)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "面数 >" + vertexErroring.ToString() + "</p>");
            if (ModelMeshRedFac.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有模型在此范围内" + "</p>");

            }
            for (int i = 0; i < ModelMeshRedFac.Count; i++)
            {

                str0 = "<p class=\"red\">" + ModelMeshRedFac[i].ToString() + "</p>";
                File.AppendAllText(htmlstr, str0);
            }
        }

        if (is_Mesh && is_face)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + faceWarning.ToString() + " < 面数 <=" + faceErroring.ToString() + "</p>");
            if (ModelMeshYellowFac.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有模型在此范围内" + "</p>");

            }
            for (int i = 0; i < ModelMeshYellowFac.Count; i++)
            {

                str0 = "<p class=\"yellow\">" + ModelMeshYellowFac[i].ToString() + "</p>";
                File.AppendAllText(htmlstr, str0);
            }
        }

        if (is_Mesh && is_face)
        {
            File.AppendAllText(htmlstr, "<p class=\"white\">" + "面数 <=" + faceWarning.ToString() + "</p>");
            if (ModelMeshGreenFac.Count == 0)
            {
                File.AppendAllText(htmlstr, "<p class=\"white\">" + "没有模型在此范围内" + "</p>");

            }
            for (int i = 0; i < ModelMeshGreenFac.Count; i++)
            {

                str0 = "<p class=\"green\">" + ModelMeshGreenFac[i].ToString() + "</p>";
                File.AppendAllText(htmlstr, str0);
            }
        }
        Debug.Log(ModelMeshRedFac.Count + ModelMeshYellowFac.Count+ModelMeshGreenFac.Count);
        File.AppendAllText(htmlstr, str3);
        File.AppendAllText(htmlstr, str4);
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
            temstr = "Mesh path: " + file.FullName + "\t" + "  Mesh: " + mesh.name + "\t" + "  Vertexs: " + verNum.ToString() + "\t" + "  Faces: " + faceNum.ToString();
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

    private void processU3DModel(GameObject g, FileInfo file)
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
            //Debug.Log(".Unity3D Mesh路径: " + file.FullName + "\t" + "  网格: " + mesh.name + "\t" + "  顶点数: " + verNum.ToString() + "\t" + "  面数: " + faceNum.ToString());
            temstr = ".Unity3D Mesh path: " + file.FullName + "\t" + "  Mesh: " + mesh.name + "\t" + "  Vertexs: " + verNum.ToString() + "\t" + "  Faces: " + faceNum.ToString();
            temMesh.Path = file.FullName;
            temMesh.MeshName = mesh.name;
            temMesh.Vertexes = verNum;
            temMesh.Faces = faceNum;
            meshInfo.Add(temMesh);
            meshInfoNoSort.Add(temMesh);

            //Debug.Log(meshInfo.Count); 
        }

        if (null == mesh)
        {
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
                processU3DModel(g.transform.GetChild(i).gameObject, file);
            }
        }
    }

    private void processU3DTexture(Texture2D tex, FileInfo file)
    {
        int w = tex.width;
        int h = tex.height;

        bool hasoutput = false;

        if (is_WidHei)
        {
            if (w != h)
            {
                string WHtemstr = "U3Dtexture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
                PictureWidHei.Add(WHtemstr);
            }
        }

        if (is_Width)
        {
            if (w > widthErroring)
            {
                string Wtemstr = "U3Dtexture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
                PictureRedWid.Add(Wtemstr);
            }

            if (w <= widthErroring && w > widthWarning)
            {
                string Wtemstr = "U3Dtexture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
                PictureYellowWid.Add(Wtemstr);
            }

            if (w <= widthWarning)
            {
                string Wtemstr = "U3Dtexture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
                PictureGreenWid.Add(Wtemstr);
            }
        }
       
        ////////////////
        if(is_Height)
        {
            if (h > heightErroring)
            {
                string Wtemstr = "U3Dtexture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
                PictureRedHei.Add(Wtemstr);
            }

            if (h <= heightErroring && h > heightWarning)
            {
                string Wtemstr = "U3Dtexture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
                PictureYellowHei.Add(Wtemstr);
            }

            if (h <= heightWarning)
            {
                string Wtemstr = "U3Dtexture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
                PictureGreenHei.Add(Wtemstr);
            }
        }

        if (is_Mizhi)
        {
            while (w > 2)
            {
                if (w % 2 != 0)
                {
                    temstr = "U3Dtexture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
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
                    temstr = "U3Dtexture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
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
                        temstr = "U3Dtexture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
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
                        temstr = "U3Dtexture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
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

    private void processTexture(Texture2D tex, FileInfo file)
    {
        
        int w = tex.width;
        int h = tex.height;
       
        bool hasoutput = false;

        if (is_WidHei)
        {
            if (w != h)
            {
                string WHtemstr = "Texture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
                PictureWidHei.Add(WHtemstr);
            }
        }

        if(is_Width)
        {
            if (w > widthErroring)
            {
                string Wtemstr = "Texture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
                PictureRedWid.Add(Wtemstr);
            }

            if (w <= widthErroring && w > widthWarning)
            {
                string Wtemstr = "Texture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
                PictureYellowWid.Add(Wtemstr);
            }

            if (w <= widthWarning)
            {
                string Wtemstr = "Texture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
                PictureGreenWid.Add(Wtemstr);
            }
        }
        ////////////////
        if (is_Height)
        {
            if (h > heightErroring)
            {
                string Wtemstr = "Texture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
                PictureRedHei.Add(Wtemstr);
            }

            if (h <= heightErroring && h > heightWarning)
            {
                string Wtemstr = "Texture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
                PictureYellowHei.Add(Wtemstr);
            }

            if (h <= heightWarning)
            {
                string Wtemstr = "Texture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
                PictureGreenHei.Add(Wtemstr);
            }
        }

        if (is_Mizhi)
        {
            while (w > 2)
            {
                if (w % 2 != 0)
                {
                    temstr = "Texture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
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
                    temstr = "Texture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
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
                        temstr = "Texture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
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
                        temstr = "Texture path: " + file.FullName + "\t" + "  Width: " + tex.width.ToString() + "\t" + "  Height: " + tex.height.ToString();
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

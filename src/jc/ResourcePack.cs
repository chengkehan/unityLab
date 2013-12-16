using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using JC;

public class ResourcePack : ScriptableWizard 
{
    public string config = "Assets/src/jc/ResourcePack.xml";

    private List<string> excludeList = null;

    [MenuItem("JC/ResourcePack")]
    public static void Do()
    {
        ScriptableWizard.DisplayWizard<ResourcePack>("ResourcePack", "Do");
    }

    private void OnWizardCreate()
    {
        TextAsset configTextAsset = (TextAsset)AssetDatabase.LoadMainAssetAtPath(config);
        if (configTextAsset == null)
        {
            Debug.LogError("The specified config file isnot exists or is illegal format.");
            return;
        }

        XmlDocument doc = new XmlDocument();
        try
        {
            doc.LoadXml(configTextAsset.text);
        }
        catch(System.Exception exception)
        {
            Debug.LogError("Illegal xml format.");
            Debug.LogError(exception.Message);
            return;
        }

        XmlNode excludeNode = doc.SelectSingleNode("resourcePack").Attributes.GetNamedItem("exclude");
        if (excludeNode != null)
        {
            string[] excludeBlocks = excludeNode.Value.Split(';');
            excludeList = new List<string>(excludeBlocks);
        }

        Dictionary<string/*id*/, Resource> resMap = new Dictionary<string, Resource>();
        XmlNodeList resXmlList = doc.SelectNodes("resourcePack/resource");
        foreach (XmlNode resXml in resXmlList)
        {
            XmlNode idNode = resXml.Attributes.GetNamedItem("id");
            if (idNode == null)
            {
                Debug.LogError("Missing id attribute");
            }

            if (resMap.ContainsKey(idNode.Value))
            {
                Debug.LogError("Repeated id " + idNode.Value);
                return;
            }

            XmlNode typeNode = resXml.Attributes.GetNamedItem("type");
            if (typeNode == null)
            {
                Debug.LogError("Missing type attribute at resource " + idNode.Value);
            }

            XmlNode bindingNode = resXml.Attributes.GetNamedItem("binding");
            if (bindingNode == null)
            {
                Debug.LogError("Missing binding attribute at resource " + idNode.Value);
            }

            XmlNode dependenceNode = resXml.Attributes.GetNamedItem("dependence");
            string dependence = dependenceNode == null ? null : dependenceNode.Value;

            Resource res = new Resource();
            res.id = idNode.Value;
            res.dependence = dependence;
            res.enabled = true;

            try
            {
                res.type = (Type)System.Enum.Parse(typeof(Type), typeNode.Value);
            }
            catch (System.Exception exception)
            {
                Debug.LogError("Illegal value of type attribute");
                Debug.LogError(exception.Message);
                return;
            }
            
            res.binding = bindingNode.Value == "true";
            if (res.binding)
            {
                XmlNode bindingSaveNode = resXml.SelectSingleNode("save");
                if (bindingSaveNode == null)
                {
                    Debug.LogError("Missing save node of binding resource " + res.id);
                    return;
                }
                res.save = bindingSaveNode.InnerText;
            }

            if (res.type == Type.Directory)
            {
                XmlNode recursiveNode = resXml.Attributes.GetNamedItem("recursive");
                res.recursive = recursiveNode == null ? false : (recursiveNode.Value == "true");
            }

            XmlNodeList itemXmlList = resXml.SelectNodes("item");
            if (itemXmlList.Count == 0)
            {
                Debug.LogWarning("Empty resource " + res.id);
            }
            else
            {
                res.itemList = new List<Item>();
                foreach (XmlNode itemXml in itemXmlList)
                {
                    XmlNode itemPathNode = itemXml.SelectSingleNode("path");
                    if (itemPathNode == null)
                    {
                        Debug.LogError("Missing item path node of resource " + res.id);
                        return;
                    }
                    XmlNode itemSaveNode = itemXml.SelectSingleNode("save");
                    if (itemSaveNode == null && !res.binding)
                    {
                        Debug.LogError("Missing item save node of resource " + res.id);
                        return;
                    }

                    Item item = new Item();
                    item.path = itemPathNode.InnerText;
                    item.save = itemSaveNode == null ? null : itemSaveNode.InnerText;
                    res.itemList.Add(item);
                }

                resMap.Add(res.id, res);
            }
        }

        foreach (Resource res in resMap.Values)
        {
            if (res.dependence != null)
            {
                Resource resDeps = null;
                if (resMap.TryGetValue(res.dependence, out resDeps))
                {
                    if (resDeps.subResList == null)
                    {
                        resDeps.subResList = new LinkedList<Resource>();
                    }
                    resDeps.subResList.AddLast(res);
                }
                else
                {
                    Debug.LogError("Missing dependence node that id is " + res.dependence);
                    return;
                }
            }
        }

        foreach (Resource res in resMap.Values)
        {
            if (res.dependence != null && res.subResList != null)
            {
                Debug.LogError("Illegal nested dependence resource " + res.id);
                return;
            }
        }

        foreach (Resource res in resMap.Values)
        {
            if (res.subResList != null)
            {
                BuildPipeline.PushAssetDependencies();
                if (!BuildResource(res))
                {
                    return;
                }
                foreach (Resource subRes in res.subResList)
                {
                    BuildPipeline.PushAssetDependencies();
                    if (!BuildResource(subRes))
                    {
                        return;
                    }
                    BuildPipeline.PopAssetDependencies();
                }
                BuildPipeline.PopAssetDependencies();
            }
        }
        foreach (Resource res in resMap.Values)
        {
            if (res.enabled)
            {
                if (!BuildResource(res))
                {
                    return;
                }
            }
        }

        Debug.Log("ResourcePack all complete");
    }

    private bool BuildResource(Resource res)
    {
        if (!res.enabled)
        {
            return true;
        }

        List<Object> assetList = new List<Object>();
        List<string> nameList = new List<string>();
        List<string> nameList2 = new List<string>();
        if (GetAssets(res, assetList, nameList, nameList2))
        {
            if (assetList.Count > 0)
            {
                if (res.binding)
                {
                    Object[] assetListCopy = new Object[assetList.Count];
                    string[] nameListCopy = new string[nameList.Count];
                    assetList.CopyTo(assetListCopy);
                    nameList.CopyTo(nameListCopy);
                    if (!BuildPipeline.BuildAssetBundleExplicitAssetNames(assetListCopy, nameListCopy, URL.dataPath + "/" + res.save, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.StandaloneWindows))
                    {
                        Debug.LogError("Build resource " + res.id + " failed");
                        return false;
                    }
                    else
                    {
                        res.enabled = false;
                    }
                }
                else
                {
                    int i = 0;
                    foreach (Object asset in assetList)
                    {
                        string name = nameList[i];
                        string savePath = null;
                        if (res.type == Type.File)
                        {
                            savePath = res.itemList[i].save;
                        }
                        else
                        {
                            savePath = nameList2[i] + ".unity3d";
                        }

                        string saveFullPath = URL.dataPath + "/" + savePath;
                        string saveFullDir = saveFullPath.Substring(0, saveFullPath.LastIndexOf('/'));
                        if (!Directory.Exists(saveFullDir))
                        {
                            Directory.CreateDirectory(saveFullDir);
                        }

                        if (!BuildPipeline.BuildAssetBundleExplicitAssetNames(new Object[] { asset }, new string[] { name }, saveFullPath, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.StandaloneWindows))
                        {
                            Debug.LogError("Build the " + i + "th item of resource " + res.id + " failed");
                            return false;
                        }
                        else
                        {
                            res.enabled = false;
                        }
                        ++i;
                    }
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool GetAssets(Resource res, List<Object> assetList, List<string> nameList, List<string> nameList2)
    {
        if (res.itemList == null || res.itemList.Count == 0)
        {
            return true;
        }
        if (res.type == Type.File)
        {
            return GetFileAssets(res, assetList, nameList);
        }
        else
        {
            foreach (Item item in res.itemList)
            {
                if (!GetDirectoryAssets(item.path, item.save, res.recursive, assetList, nameList, nameList2, item.path))
                {
                    return false;
                }
            }
            return true;
        }
    }

    private bool GetFileAssets(Resource res, List<Object> assetList, List<string> nameList)
    {
        foreach (Item item in res.itemList)
        {
            Object resObj = AssetDatabase.LoadMainAssetAtPath(URL.assets + "/" + item.path);
            if (resObj == null)
            {
                Debug.LogError("Resource is not exists of resource " + res.id + ". Path is " + item.path);
                return false;
            }
            else
            {
                assetList.Add(resObj);
                nameList.Add(item.path);
            }
        }
        return true;
    }

    private bool GetDirectoryAssets(string directory, string save, bool recursive, List<Object> assetList, List<string> nameList, List<string> nameList2, string dirRoot)
    {
        if (IsExclude(directory))
        {
            return true;
        }

        try
        {
            string[] fileList = Directory.GetFiles(URL.dataPath + "/" + directory);
            int dataPathLength = URL.dataPath.Length;
            foreach (string file in fileList)
            {
                if (IsExclude(file))
                {
                    continue;
                }
                string filePath = file.Substring(dataPathLength);
                Object resObj = AssetDatabase.LoadMainAssetAtPath(URL.assets + filePath);
                if (resObj == null)
                {
                    Debug.LogError("Resource " + filePath + " is not exists");
                    return false;
                }
                else
                {
                    filePath = filePath.Replace("\\", "/").Substring(1);
                    assetList.Add(resObj);
                    nameList.Add(filePath);

                    string name2 = filePath.Substring(dirRoot.Length + 1);
                    nameList2.Add(save + "/" + name2.Substring(0, name2.LastIndexOf(".")));
                }
            }

            if (recursive)
            {
                string[] dirList = Directory.GetDirectories(URL.dataPath + "/" + directory);
                foreach (string dir in dirList)
                {
                    string newDir = dir.Replace("\\", "/");
                    newDir = newDir.Substring(dataPathLength + 1);
                    if (!GetDirectoryAssets(newDir, save, recursive, assetList, nameList, nameList2, dirRoot))
                    {
                        return false;
                    }
                }
            }
        }
        catch (System.Exception exception)
        {
            Debug.LogError(directory);
            Debug.LogError(exception.Message);
            return false;
        }
        return true;
    }

    private bool IsExclude(string file)
    {
        if (excludeList == null)
        {
            return false;
        }

        foreach (string exclude in excludeList)
        {
            if (file.EndsWith(exclude))
            {
                return true;
            }
        }
        return false;
    }

    private class Resource
    {
        public string id = null;

        public Type type;

        public string dependence = null;

        public bool binding = false;

        public List<Item> itemList = null;

        public string save = null;

        public bool recursive = false;

        public bool enabled = false;

        public LinkedList<Resource> subResList = null;
    }

    private class Item
    {
        public string path = null;

        public string save = null;
    }

    private enum Type
    {
        Directory, 
        File
    }
}

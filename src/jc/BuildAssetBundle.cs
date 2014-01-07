using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace JC
{
    public class BuildAssetBundle : ScriptableWizard
    {
        public string configPath = "JC/Editor/BuildAssetBundle.xml";

        private static string error_configString = "无法获取配置文件 ";

        private static string error_configXML = "无法解析配置文件";

        private static string error_configXMLRoot = "无法解析配置文件根节点（buildAssetBundle）";

        private static string error_platform = "无法解析目标平台（platform）";

        private static string error_missingSaveRootNode = "无法确定输出的根目录（saveRoot节点）";

        private static string error_emptyAsset = "没有配置任何资源";

        private static string error_missingId = "无法获取资源的id";

        private static string error_missingIdValue = "没有指定资源的id";

        private static string error_repetitiveId = "重复的id";

        private static string error_missingItemType = "无法获取资源的类型（item节点的type缺失）";

        private static string error_unrecognizedItemType = "无法识别的资源类型（item节点的type）";

        private static string error_missingItemPath = "无法获取资源的位置（item节点中的内容）";

        private static string error_missingBindSavePath = "无法获取绑定资源的保存路径（bind节点的savePath）";

        private static string wanring_emptyItemDataCollection = "存在未指定任何资源的配置";

        private static string error_dependentAssetNotFound = "依赖的资源未找到";

        private static string error_dependOnSelf = "无法自我依赖";

        private static string error_recursiveDependence = "非法的递归依赖";

        private static string error_fileNotExist = "文件不存在";

        private static string error_directoryNotExist = "目录不存在";

        private static string error_fileAssetObjectFail = "读取资源文件失败";

        private static string error_savePath = "保存路径存在异常";

        private static string error_uncaughtError = "发生未知的错误";

        private static AssetBundleData assetBundleData = null;

        [MenuItem("JC/BuildAssetBundle")]
        private static void Build()
        {
            ScriptableWizard.DisplayWizard<BuildAssetBundle>("BuildAssetBundle", "Create");
        }

        private void OnWizardCreate()
        {
            assetBundleData = null;

            string configString = null;
            if (!GetConfigString(out configString))
            {
                return;
            }

            if (!ParseAssetBundleData(configString, out assetBundleData))
            {
                return;
            }

            if (!PreBuildDo())
            {
                return;
            }

            if (!BuildDo())
            {
                return;
            }

            Debug.Log("BuildAssetBundle Complete");
        }

        private bool BuildDo()
        {
            foreach (AssetData assetData in assetBundleData.assetDataMap.Values)
            {
                if (!assetData.enabled || assetData.HasDependence())
                {
                    continue;
                }

                if (!BuildAsset(assetData))
                {
                    return false;
                }
            }
            return true;
        }

        private bool BuildAsset(AssetData assetData)
        {
            if (assetData.IsEmpty())
            {
                return true;
            }

            AssetData[] assetDataChildren = null;
            bool hasAssetDataChildren = assetBundleData.TryGetDependenceChildren(assetData.id, out assetDataChildren);
            if (hasAssetDataChildren || assetData.HasDependence())
            {
                BuildPipeline.PushAssetDependencies();
            }

            assetData.enabled = false;
            foreach (ItemDataCollection itemDataCollection in assetData.itemDataCollectionList)
            {
                if (itemDataCollection.IsEmpty())
                {
                    continue;
                }
                else
                {
                    if (!BuildItemDataCollection(itemDataCollection))
                    {
                        return false;
                    }
                }
            }

            if (hasAssetDataChildren)
            {
                foreach (AssetData assetDataChild in assetDataChildren)
                {
                    BuildAsset(assetDataChild);
                }
            }

            if (hasAssetDataChildren || assetData.HasDependence())
            {
                BuildPipeline.PopAssetDependencies();
            }
            return true;
        }

        private bool BuildItemDataCollection(ItemDataCollection itemDataCollection)
        {
            List<Object> assetObjectList = new List<Object>();
            List<string> assetObjectNameList = new List<string>();
            List<string> savePathList = new List<string>();

            if (!itemDataCollection.GetBuildParams(assetObjectList, assetObjectNameList, savePathList))
            {
                return false;
            }

            if (assetObjectList.Count == 0)
            {
                return true;
            }

            if (itemDataCollection.bind)
            {
                string dir = null;
                string name = null;
                if (!SplitPath(itemDataCollection.savePath, out dir, out name))
                {
                    Debug.LogError(error_savePath + ":" + itemDataCollection.savePath);
                    return false;
                }

                if (!Directory.Exists(URL.dataPath + "/" + assetBundleData.saveRoot + "/" + dir))
                {
                    try
                    {
                        Directory.CreateDirectory(URL.dataPath + "/" + assetBundleData.saveRoot + "/" + dir);
                    }
                    catch (System.Exception exception)
                    {
                        Debug.LogError(error_savePath + ":" + itemDataCollection.savePath + "->" + exception.Message);
                        return false;
                    }
                }

                if (!BuildPipeline.BuildAssetBundleExplicitAssetNames(assetObjectList.ToArray(), assetObjectNameList.ToArray(), URL.assets + "/" + assetBundleData.saveRoot  + "/" + itemDataCollection.savePath, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, assetBundleData.platform))
                {
                    return false;
                }
            }
            else
            {
                foreach (string savePath in savePathList)
                {
                    string dir = null;
                    string name = null;
                    if (!SplitPath(savePath, out dir, out name))
                    {
                        Debug.LogError(error_savePath + ":" + savePath);
                        return false;
                    }

                    if (!Directory.Exists(URL.dataPath + "/" + assetBundleData.saveRoot + "/" + dir))
                    {
                        try
                        {
                            Directory.CreateDirectory(URL.dataPath + "/" + assetBundleData.saveRoot + "/" + dir);
                        }
                        catch (System.Exception exception)
                        {
                            Debug.LogError(error_savePath + ":" + savePath + "->" + exception.Message);
                            return false;
                        }
                    }
                }

                int length = assetObjectList.Count;
                Object[] tempObjList = new Object[1];
                string[] tempNameList = new string[1];
                for (int i = 0; i < length; ++i)
                {
                    tempObjList[0] = assetObjectList[i];
                    tempNameList[0] = assetObjectNameList[i];
                    if (!BuildPipeline.BuildAssetBundleExplicitAssetNames(tempObjList, tempNameList, URL.assets + "/" + assetBundleData.saveRoot + "/" + savePathList[i], BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, assetBundleData.platform))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool PreBuildDo()
        {
            // savePath
            if (!Directory.Exists(URL.dataPath + "/" + assetBundleData.saveRoot))
            {
                Directory.CreateDirectory(URL.dataPath + "/" + assetBundleData.saveRoot);
            }

            foreach (AssetData assetData in assetBundleData.assetDataMap.Values)
            {
                // asset id
                if (assetData.HasDependence())
                {
                    if (!assetBundleData.assetDataMap.ContainsKey(assetData.dependence))
                    {
                        Debug.LogError(error_dependentAssetNotFound + ":" + assetData.id + "->" + assetData.dependence);
                        return false;
                    }
                    if (assetData.id == assetData.dependence)
                    {
                        Debug.LogError(error_dependOnSelf + ":" + assetData.id);
                        return false;
                    }
                }

                // dependence
                if (!DependenceChainChecking(assetData.id))
                {
                    return false;
                }

                // itemDataCollection
                foreach (ItemDataCollection itemDataCollection in assetData.itemDataCollectionList)
                {
                    if (itemDataCollection.IsEmpty())
                    {
                        continue;
                    }

                    // itemData
                    foreach (ItemData itemData in itemDataCollection.itemDataList)
                    {
                        if (itemData.type == ItemType.File)
                        {
                            if (!File.Exists(URL.dataPath + "/" + itemData.path))
                            {
                                Debug.LogError(error_fileNotExist + ":" + itemData.path);
                                return false;
                            }
                        }
                        else if (itemData.type == ItemType.Directory)
                        {
                            if (!Directory.Exists(URL.dataPath + "/" + itemData.path))
                            {
                                Debug.LogError(error_directoryNotExist + ":" + itemData.path);
                                return false;
                            }
                        }
                        else
                        {
                            Debug.LogError(error_uncaughtError + ":" + itemData.type);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool GetConfigString(out string configString)
        {
            configString = null;
            Object configObj = AssetDatabase.LoadMainAssetAtPath(URL.assets + "/" + configPath);
            if(configObj == null)
            {
                Debug.Log(error_configString + configPath);
                return false;
            }

            TextAsset configTextAsset = configObj as TextAsset;
            if (configTextAsset == null)
            {
                Debug.Log(error_configString + configPath);
                return false;
            }

            configString = configTextAsset.text;
            return true;
        }

        private bool ParseAssetBundleData(string configString, out AssetBundleData assetBundleData)
        {
            assetBundleData = null;

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(configString);
            }
            catch (System.Exception exception)
            {
                Debug.LogError(error_configXML);
                return false;
            }

            assetBundleData = new AssetBundleData();

            XmlNode buildAssetBundleNode = doc.SelectSingleNode("buildAssetBundle");
            if (buildAssetBundleNode == null)
            {
                Debug.LogError(error_configXMLRoot);
                return false;
            }

            // ignoreExtention
            XmlNode ignoreExtentionNode = buildAssetBundleNode.Attributes.GetNamedItem("ignoreExtention");
            if (ignoreExtentionNode != null)
            {
                string[] blocks = ignoreExtentionNode.Value.Split(';');
                if (blocks != null && blocks.Length > 0)
                {
                    assetBundleData.ignoreExtentionList = new List<string>();
                    foreach (string block in blocks)
                    {
                        assetBundleData.ignoreExtentionList.Add(block);
                    }
                }
            }

            // platform
            XmlNode platformNode = buildAssetBundleNode.Attributes.GetNamedItem("platform");
            if(platformNode != null)
            {
                try
                {
                    assetBundleData.platform = (BuildTarget)System.Enum.Parse(typeof(BuildTarget), platformNode.Value);
                }
                catch(System.Exception exception)
                {
                    Debug.LogError(error_platform + ":" + platformNode.Value);
                    return false;
                }
            }

            // saveRoot
            XmlNode saveRootNode = doc.SelectSingleNode("buildAssetBundle/saveRoot");
            if (saveRootNode == null)
            {
                Debug.LogError(error_missingSaveRootNode);
                return false;
            }
            assetBundleData.saveRoot = saveRootNode.InnerText;

            // asset
            XmlNodeList assetNodeList = doc.SelectNodes("buildAssetBundle/asset");
            if (assetNodeList == null || assetNodeList.Count == 0)
            {
                Debug.LogError(error_emptyAsset);
                return false;
            }
            assetBundleData.assetDataMap = new Dictionary<string, AssetData>();
            foreach (XmlNode assetNode in assetNodeList)
            {
                AssetData assetData = new AssetData();
                assetData.enabled = true;
                assetData.itemDataCollectionList = new List<ItemDataCollection>();

                // id
                XmlNode idNode = assetNode.Attributes.GetNamedItem("id");
                if (idNode == null)
                {
                    Debug.LogError(error_missingId);
                    return false;
                }
                if (assetBundleData.assetDataMap.ContainsKey(idNode.Value))
                {
                    Debug.LogError(error_repetitiveId + ":" + idNode.Value);
                    return false;
                }
                if (string.IsNullOrEmpty(idNode.Value))
                {
                    Debug.LogError(error_missingIdValue + ":" + idNode.Value);
                    return false;
                }
                assetData.id = idNode.Value;
                assetBundleData.assetDataMap.Add(assetData.id, assetData);

                // dependence
                XmlNode dependenceNode = assetNode.Attributes.GetNamedItem("dependence");
                if (dependenceNode != null)
                {
                    assetData.dependence = dependenceNode.Value;
                }

                // item
                XmlNodeList itemNodeList = assetNode.SelectNodes("item");
                if(itemNodeList != null && itemNodeList.Count > 0)
                {
                    foreach (XmlNode itemNode in itemNodeList)
                    {
                        ItemDataCollection itemDataCollection = new ItemDataCollection();
                        assetData.itemDataCollectionList.Add(itemDataCollection);
                        itemDataCollection.bind = false;
                        itemDataCollection.itemDataList = new List<ItemData>();

                        ItemData itemData = null;
                        if (!ParseItemNode(itemNode, assetData.id, out itemData))
                        {
                            return false;
                        }
                        itemDataCollection.itemDataList.Add(itemData);
                    }
                }

                // bind
                XmlNodeList bindNodeList = assetNode.SelectNodes("bind");
                if(bindNodeList != null)
                {
                    foreach (XmlNode bindNode in bindNodeList)
                    {
                        ItemDataCollection itemDataCollection = new ItemDataCollection();
                        assetData.itemDataCollectionList.Add(itemDataCollection);
                        itemDataCollection.bind = true;
                        itemDataCollection.itemDataList = new List<ItemData>();

                        // savePath
                        XmlNode savePathNode = bindNode.Attributes.GetNamedItem("savePath");
                        if (savePathNode == null)
                        {
                            Debug.LogError(error_missingBindSavePath + ":" + assetData.id);
                            return false;
                        }
                        itemDataCollection.savePath = savePathNode.Value;

                        // item
                        XmlNodeList itemNodeList2 = bindNode.SelectNodes("item");
                        if (itemNodeList2 != null && itemNodeList2.Count > 0)
                        {
                            foreach (XmlNode itemNode in itemNodeList2)
                            {
                                ItemData itemData = null;
                                if (!ParseItemNode(itemNode, assetData.id, out itemData))
                                {
                                    return false;
                                }
                                itemDataCollection.itemDataList.Add(itemData);
                            }
                        }

                        // empty test
                        if (itemDataCollection.IsEmpty())
                        {
                            Debug.LogWarning(wanring_emptyItemDataCollection + ":" + assetData.id);
                        }
                    }
                }

                // empty test
                if (assetData.IsEmpty())
                {
                    Debug.LogWarning(wanring_emptyItemDataCollection + ":" + assetData.id);
                }
            }

            return true;
        }

        private bool ParseItemNode(XmlNode itemNode, string assetDataId, out ItemData itemData)
        {
            itemData = new ItemData();

            // type
            XmlNode typeNode = itemNode.Attributes.GetNamedItem("type");
            if (typeNode == null)
            {
                Debug.LogError(error_missingItemType + ":" + assetDataId);
                return false;
            }
            try
            {
                ItemType type = (ItemType)System.Enum.Parse(typeof(ItemType), typeNode.Value, true);
                itemData.type = type;
            }
            catch (System.Exception exception)
            {
                Debug.LogError(error_unrecognizedItemType + ":" + assetDataId);
                return false;
            }

            // recursion
            XmlNode recursionNode = itemNode.Attributes.GetNamedItem("recursion");
            itemData.recursive = recursionNode == null ? false : (recursionNode.Value.ToLower() == "true");

            string path = itemNode.InnerText.Replace('\\', '/');
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError(error_missingItemPath + ":" + assetDataId);
                return false;
            }
            itemData.path = path;

            return true;
        }

        private bool DependenceChainChecking(string assetDataId)
        {
            Dictionary<string/*assetDataId*/, int> checkingList = new Dictionary<string, int>();
            while (!string.IsNullOrEmpty(assetDataId))
            {
                if (checkingList.ContainsKey(assetDataId))
                {
                    Debug.LogError(error_recursiveDependence + ":" + assetDataId);
                    return false;
                }
                checkingList.Add(assetDataId, 0);

                AssetData assetData = null;
                if (!assetBundleData.assetDataMap.TryGetValue(assetDataId, out assetData))
                {
                    Debug.LogError(error_uncaughtError + ":" + assetDataId);
                    return false;
                }

                if (assetData.HasDependence())
                {
                    assetDataId = assetData.dependence;
                }
                else
                {
                    assetDataId = null;
                }
            }
            return true;
        }

        private bool SplitPath(string path, out string dir, out string name)
        {
            dir = null;
            name = null;
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            int index = path.LastIndexOf('/');
            if (index == -1)
            {
                dir = string.Empty;
                name = path;
            }
            else
            {
                dir = path.Substring(0, index);
                name = path.Substring(index + 1);
            }
            return true;
        }

        private class AssetBundleData
        {
            public List<string> ignoreExtentionList = null;

            public string saveRoot = null;

            public BuildTarget platform = BuildTarget.WebPlayer;

            public Dictionary<string/*id of AssetData*/, AssetData> assetDataMap = null;

            public bool TryGetDependenceChildren(string assetId, out AssetData[] assetDataDependenceChildren)
            {
                List<AssetData> temp = null;
                foreach (AssetData assetData in assetDataMap.Values)
                {
                    if (assetData.id != assetId && assetData.HasDependence() && assetData.dependence == assetId)
                    {
                        if (temp == null)
                        {
                            temp = new List<AssetData>();
                        }
                        temp.Add(assetData);
                    }
                }
                assetDataDependenceChildren = temp == null ? null : temp.ToArray();

                return temp != null && temp.Count > 0;
            }

            public bool IsIgnoreExtension(string path)
            {
                if (ignoreExtentionList == null || ignoreExtentionList.Count == 0)
                {
                    return false;
                }

                foreach (string ignoreExtension in ignoreExtentionList)
                {
                    if (path.IndexOf(ignoreExtension) != -1)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private class AssetData
        {
            public bool enabled = false;

            public string id = null;

            public string dependence = null;

            public List<ItemDataCollection> itemDataCollectionList = null;

            public bool HasDependence()
            {
                return !string.IsNullOrEmpty(dependence);
            }

            public bool IsEmpty()
            {
                if (itemDataCollectionList == null || itemDataCollectionList.Count == 0)
                {
                    return true;
                }
                else
                {
                    foreach (ItemDataCollection itemDataCollection in itemDataCollectionList)
                    {
                        if (!itemDataCollection.IsEmpty())
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }

        private class ItemDataCollection
        {
            public bool bind = false;

            public string savePath = null;

            public List<ItemData> itemDataList = null;

            public bool IsEmpty()
            {
                return itemDataList == null || itemDataList.Count == 0;
            }

            public bool GetBuildParams(List<Object> assetObjectList, List<string> assetObjectNameList, List<string> savePathList)
            {
                if (IsEmpty())
                {
                    return true;
                }

                foreach (ItemData itemData in itemDataList)
                {
                    if (!itemData.GetBuildParams(assetObjectList, assetObjectNameList, savePathList))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private class ItemData
        {
            public ItemType type;

            public bool recursive = false;

            public string path = null;

            public bool GetBuildParams(List<Object> assetObjectList, List<string> assetObjectNameList, List<string> savePathList)
            {
                if (type == ItemType.File)
                {
                    return GetBuildFileParams(path, assetObjectList, assetObjectNameList, savePathList);
                }
                else if (type == ItemType.Directory)
                {
                    return GetBuildDirectoryParams(path, assetObjectList, assetObjectNameList, savePathList);
                }
                else
                {
                    Debug.LogError(error_uncaughtError + ":" + type);
                    return false;
                }
            }

            private bool GetBuildDirectoryParams(string dirPath, List<Object> assetObjectList, List<string> assetObjectNameList, List<string> savePathList)
            {
                if (assetBundleData.IsIgnoreExtension(dirPath))
                {
                    return true;
                }

                // files
                string[] fileFullPathList = Directory.GetFiles(URL.dataPath + "/" + dirPath);
                if (fileFullPathList != null && fileFullPathList.Length != 0)
                {
                    foreach (string fileFullPath in fileFullPathList)
                    {
                        string filePath = fileFullPath.Replace('\\', '/').Substring(URL.dataPath.Length + 1);
                        if (!GetBuildFileParams(filePath, assetObjectList, assetObjectNameList, savePathList))
                        {
                            return false;
                        }
                    }
                }                

                // sub directory
                if (recursive)
                {
                    string[] subDirFullPathList = Directory.GetDirectories(URL.dataPath + "/" + dirPath);
                    if (subDirFullPathList != null && subDirFullPathList.Length != 0)
                    {
                        foreach (string subDirFullPath in subDirFullPathList)
                        {
                            string subDirPath = subDirFullPath.Replace('\\', '/').Substring(URL.dataPath.Length + 1);
                            if (!GetBuildDirectoryParams(subDirPath, assetObjectList, assetObjectNameList, savePathList))
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }

            private bool GetBuildFileParams(string filePath, List<Object> assetObjectList, List<string> assetObjectNameList, List<string> savePathList)
            {
                if (assetBundleData.IsIgnoreExtension(filePath))
                {
                    return true;
                }

                Object assetObject = AssetDatabase.LoadMainAssetAtPath(URL.assets + "/" + filePath);
                if (assetObject == null)
                {
                    Debug.LogError(error_fileAssetObjectFail + ":" + filePath);
                    return false;
                }
                assetObjectList.Add(assetObject);

                assetObjectNameList.Add(filePath);

                savePathList.Add(filePath + ".unity3d");
                return true;
            }
        }

        private enum ItemType
        {
            Directory, 
            File
        }
    }
}
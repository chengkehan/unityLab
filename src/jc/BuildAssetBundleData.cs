using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace JC
{
    public class BuildAssetBundleData
    {
        public class AssetBundleData
        {
            public List<string> ignoreExtentionList = null;

            public string saveRoot = null;

#if UNITY_EDITOR
            public BuildTarget platform = BuildTarget.WebPlayer;
#endif

            public Dictionary<string/*id of AssetData*/, AssetData> assetDataMap = null;

            public static bool ParseAssetBundleData(string configString, out AssetBundleData assetBundleData)
            {
                assetBundleData = null;

                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.LoadXml(configString);
                }
                catch (System.Exception exception)
                {
                    Debug.LogError("无法解析配置文件");
                    return false;
                }

                assetBundleData = new AssetBundleData();

                XmlNode buildAssetBundleNode = doc.SelectSingleNode("buildAssetBundle");
                if (buildAssetBundleNode == null)
                {
                    Debug.LogError("无法解析配置文件根节点（buildAssetBundle）");
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
#if UNITY_EDITOR
                XmlNode platformNode = buildAssetBundleNode.Attributes.GetNamedItem("platform");
                if (platformNode != null)
                {
                    try
                    {
                        assetBundleData.platform = (BuildTarget)System.Enum.Parse(typeof(BuildTarget), platformNode.Value);
                    }
                    catch (System.Exception exception)
                    {
                        Debug.LogError("无法解析目标平台（platform）" + ":" + platformNode.Value);
                        return false;
                    }
                }
#endif

                // saveRoot
                XmlNode saveRootNode = doc.SelectSingleNode("buildAssetBundle/saveRoot");
                if (saveRootNode == null)
                {
                    Debug.LogError("无法确定输出的根目录（saveRoot节点）");
                    return false;
                }
                assetBundleData.saveRoot = saveRootNode.InnerText;

                // asset
                XmlNodeList assetNodeList = doc.SelectNodes("buildAssetBundle/asset");
                if (assetNodeList == null || assetNodeList.Count == 0)
                {
                    Debug.LogError("没有配置任何资源");
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
                        Debug.LogError("无法获取资源的id");
                        return false;
                    }
                    if (assetBundleData.assetDataMap.ContainsKey(idNode.Value))
                    {
                        Debug.LogError("重复的id" + ":" + idNode.Value);
                        return false;
                    }
                    if (string.IsNullOrEmpty(idNode.Value))
                    {
                        Debug.LogError("没有指定资源的id" + ":" + idNode.Value);
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
                    if (itemNodeList != null && itemNodeList.Count > 0)
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
                    if (bindNodeList != null)
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
                                Debug.LogError("无法获取绑定资源的保存路径（bind节点的savePath）" + ":" + assetData.id);
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
                                Debug.LogWarning("存在未指定任何资源的配置" + ":" + assetData.id);
                            }
                        }
                    }

                    // empty test
                    if (assetData.IsEmpty())
                    {
                        Debug.LogWarning("存在未指定任何资源的配置" + ":" + assetData.id);
                    }
                }

                return true;
            }

            private static bool ParseItemNode(XmlNode itemNode, string assetDataId, out ItemData itemData)
            {
                itemData = new ItemData();

                // type
                XmlNode typeNode = itemNode.Attributes.GetNamedItem("type");
                if (typeNode == null)
                {
                    Debug.LogError("无法获取资源的类型（item节点的type缺失）" + ":" + assetDataId);
                    return false;
                }
                try
                {
                    ItemType type = (ItemType)System.Enum.Parse(typeof(ItemType), typeNode.Value, true);
                    itemData.type = type;
                }
                catch (System.Exception exception)
                {
                    Debug.LogError("无法识别的资源类型（item节点的type）" + ":" + assetDataId);
                    return false;
                }

                // recursion
                XmlNode recursionNode = itemNode.Attributes.GetNamedItem("recursion");
                itemData.recursive = recursionNode == null ? false : (recursionNode.Value.ToLower() == "true");

                string path = itemNode.InnerText.Replace('\\', '/');
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogError("无法获取资源的位置（item节点中的内容）" + ":" + assetDataId);
                    return false;
                }
                itemData.path = path;

                // isScene
                itemData.isScene = path.EndsWith(".unity");

                return true;
            }

            public bool TryGetDependenceList(string assetId, out AssetData[] assetDataDependenceList)
            {
                List<AssetData> temp = null;
                AssetData assetData = null;
                if (assetDataMap.TryGetValue(assetId, out assetData))
                {
                    temp = new List<AssetData>();
                    while (assetData != null)
                    {
                        temp.Add(assetData);
                        if (assetData.HasDependence())
                        {
                            assetDataMap.TryGetValue(assetData.dependence, out assetData);
                        }
                        else
                        {
                            assetData = null;
                        }
                    }
                }
                if (temp != null)
                {
                    temp.Reverse();
                }
                assetDataDependenceList = temp == null ? null : temp.ToArray();

                return temp != null && temp.Count > 0;
            }

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

        public class AssetData
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

        public class ItemDataCollection
        {
            public bool bind = false;

            public string savePath = null;

            public List<ItemData> itemDataList = null;

            public bool IsEmpty()
            {
                return itemDataList == null || itemDataList.Count == 0;
            }
#if UNITY_EDITOR
            public bool GetBuildParams(AssetBundleData assetBundleData, List<Object> assetObjectList, List<string> assetObjectNameList, List<string> savePathList)
            {
                if (IsEmpty())
                {
                    return true;
                }

                foreach (ItemData itemData in itemDataList)
                {
                    if (!itemData.GetBuildParams(assetBundleData, assetObjectList, assetObjectNameList, savePathList))
                    {
                        return false;
                    }
                }
                return true;
            }
#endif
        }

        public class ItemData
        {
            public ItemType type;

            public bool isScene = false;

            public bool recursive = false;

            public string path = null;

#if UNITY_EDITOR
            public bool GetBuildParams(AssetBundleData assetBundleData, List<Object> assetObjectList, List<string> assetObjectNameList, List<string> savePathList)
            {
                if (isScene)
                {
                    return GetBuildSceneParams(assetBundleData, path, assetObjectList, assetObjectNameList, savePathList);
                }
                else
                {
                    if (type == ItemType.File)
                    {
                        return GetBuildFileParams(assetBundleData, path, assetObjectList, assetObjectNameList, savePathList);
                    }
                    else if (type == ItemType.Directory)
                    {
                        return GetBuildDirectoryParams(assetBundleData, path, assetObjectList, assetObjectNameList, savePathList);
                    }
                    else
                    {
                        Debug.LogError("发生未知的错误" + ":" + type);
                        return false;
                    }
                }
            }

            private bool GetBuildDirectoryParams(AssetBundleData assetBundleData, string dirPath, List<Object> assetObjectList, List<string> assetObjectNameList, List<string> savePathList)
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
                        if (!GetBuildFileParams(assetBundleData, filePath, assetObjectList, assetObjectNameList, savePathList))
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
                            if (!GetBuildDirectoryParams(assetBundleData, subDirPath, assetObjectList, assetObjectNameList, savePathList))
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }

            private bool GetBuildFileParams(AssetBundleData assetBundleData, string filePath, List<Object> assetObjectList, List<string> assetObjectNameList, List<string> savePathList)
            {
                if (assetBundleData.IsIgnoreExtension(filePath))
                {
                    return true;
                }

                Object assetObject = AssetDatabase.LoadMainAssetAtPath(URL.assets + "/" + filePath);
                if (assetObject == null)
                {
                    Debug.LogError("读取资源文件失败" + ":" + filePath);
                    return false;
                }
                assetObjectList.Add(assetObject);
                assetObjectNameList.Add(filePath);
                savePathList.Add(filePath + ".unity3d");
                return true;
            }

            private bool GetBuildSceneParams(AssetBundleData assetBundleData, string filePath, List<Object> assetObjectList, List<string> assetObjectNameList, List<string> savePathList)
            {
                if (assetBundleData.IsIgnoreExtension(filePath))
                {
                    return true;
                }

                assetObjectList.Add(null);
                assetObjectNameList.Add(filePath);
                savePathList.Add(filePath + ".unity3d");
                return true;
            }
#endif
        }

        public enum ItemType
        {
            Directory,
            File
        }
    }
}

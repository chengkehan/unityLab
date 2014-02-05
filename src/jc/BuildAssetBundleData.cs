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
        public static bool SetupLightmap(Object lightmapAssetObj)
        {
            if (!(lightmapAssetObj is LightMapAsset))
            {
                return false;
            }


            LightMapAsset lightmapAsset = (LightMapAsset)lightmapAssetObj;

            if (lightmapAsset.lightmapFar == null || lightmapAsset.lightmapNear == null || lightmapAsset.lightmapFar.Length != lightmapAsset.lightmapNear.Length)
            {
                return false;
            }

            int count = lightmapAsset.lightmapFar.Length;
            LightmapData[] lightmapDatas = new LightmapData[count];
            for (int i = 0; i < count; ++i)
            {
                LightmapData Lightmap = new LightmapData();
                Lightmap.lightmapFar = lightmapAsset.lightmapFar[i];
                Lightmap.lightmapNear = lightmapAsset.lightmapNear[i];
                lightmapDatas[i] = Lightmap;
            }
            LightmapSettings.lightmaps = lightmapDatas;
            return true;
        }

        public class AssetBundleData
        {
            public List<string> ignoreExtentionList = null;

            public string saveRoot = null;

#if UNITY_EDITOR
            public BuildTarget platform = BuildTarget.WebPlayer;

            public List<string> garbageAssetList = new List<string>();
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
                    Debug.LogError("Cannot parse config file");
                    return false;
                }

                assetBundleData = new AssetBundleData();

                XmlNode buildAssetBundleNode = doc.SelectSingleNode("buildAssetBundle");
                if (buildAssetBundleNode == null)
                {
                    Debug.LogError("Cannot parse node（buildAssetBundle）");
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
                        Debug.LogError("Cannot parse node（platform）" + ":" + platformNode.Value);
                        return false;
                    }
                }
#endif

                // saveRoot
                XmlNode saveRootNode = doc.SelectSingleNode("buildAssetBundle/saveRoot");
                if (saveRootNode == null)
                {
                    Debug.LogError("Cannot parse node（saveRoot）");
                    return false;
                }
                assetBundleData.saveRoot = saveRootNode.InnerText;

                // asset
                XmlNodeList assetNodeList = doc.SelectNodes("buildAssetBundle/asset");
                if (assetNodeList == null || assetNodeList.Count == 0)
                {
                    Debug.LogError("Nothing asset");
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
                        Debug.LogError("Missing asset id");
                        return false;
                    }
                    if (assetBundleData.assetDataMap.ContainsKey(idNode.Value))
                    {
                        Debug.LogError("Repeated id:" + idNode.Value);
                        return false;
                    }
                    if (string.IsNullOrEmpty(idNode.Value))
                    {
                        Debug.LogError("Missing asset id:" + idNode.Value);
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
                                Debug.LogError("Cannot parse node（bind.savePath）" + ":" + assetData.id);
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
                                Debug.LogWarning("Empty asset config:" + assetData.id);
                            }
                        }
                    }

                    // empty test
                    if (assetData.IsEmpty())
                    {
                        Debug.LogWarning("Empty asset config:" + assetData.id);
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
                    Debug.LogError("Cannot parse node（item.type）:" + assetDataId);
                    return false;
                }
                try
                {
                    ItemType type = (ItemType)System.Enum.Parse(typeof(ItemType), typeNode.Value, true);
                    itemData.type = type;
                }
                catch (System.Exception exception)
                {
                    Debug.LogError("Cannot parse node（item.type）:" + assetDataId);
                    return false;
                }

                // recursion
                XmlNode recursionNode = itemNode.Attributes.GetNamedItem("recursion");
                itemData.recursive = recursionNode == null ? false : (recursionNode.Value.ToLower() == "true");

                // path
                XmlNode pathNode = itemNode.SelectSingleNode("path");
                if (pathNode == null)
                {
                    Debug.LogError("Missing path:" + assetDataId);
                    return false;
                }
                string path = pathNode.InnerText.Replace('\\', '/');
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogError("Missing path:" + assetDataId);
                    return false;
                }
                itemData.path = path;

                // isScene
                itemData.isScene = path.EndsWith(".unity");

                // lightmap
                XmlNode lightmapNode = itemNode.SelectSingleNode("lightmap");
                if (lightmapNode != null)
                {
                    if (itemData.type != ItemType.File)
                    {
                        Debug.LogError("Only the file type item can specify the lightmap node:" + itemData.path);
                        return false;
                    }

                    string lightmapScene = lightmapNode.InnerText;
                    if (!lightmapScene.EndsWith(".unity"))
                    {
                        Debug.LogError("lightmap can only be set to the scene file:" + lightmapScene);
                        return false;
                    }
                    itemData.lightmapScene = lightmapScene;
                }

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

            public string lightmapScene = null;

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
                        return GetBuildFileParams(assetBundleData, path, assetObjectList, assetObjectNameList, savePathList, lightmapScene);
                    }
                    else if (type == ItemType.Directory)
                    {
                        return GetBuildDirectoryParams(assetBundleData, path, assetObjectList, assetObjectNameList, savePathList);
                    }
                    else
                    {
                        Debug.LogError("Undefined Error" + type);
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
                        if (!GetBuildFileParams(assetBundleData, filePath, assetObjectList, assetObjectNameList, savePathList, null))
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

            private bool GetBuildFileParams(AssetBundleData assetBundleData, string filePath, List<Object> assetObjectList, List<string> assetObjectNameList, List<string> savePathList, string lightmapScene)
            {
                if (assetBundleData.IsIgnoreExtension(filePath))
                {
                    return true;
                }

                Object assetObject = AssetDatabase.LoadMainAssetAtPath(URL.assets + "/" + filePath);
                if (assetObject == null)
                {
                    Debug.LogError("Read asset file fail:" + filePath);
                    return false;
                }

                assetObjectList.Add(assetObject);
                assetObjectNameList.Add(filePath);
                savePathList.Add(filePath + ".unity3d");

                if (!GetBuildLightmapParams(assetBundleData, filePath, assetObjectList, assetObjectNameList, savePathList, lightmapScene))
                {
                    return false;
                }

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

            private bool GetBuildLightmapParams(AssetBundleData assetBundleData, string filePath, List<Object> assetObjectList, List<string> assetObjectNameList, List<string> savePathList, string lightmapScene)
            {
                if (string.IsNullOrEmpty(lightmapScene))
                {
                    return true;
                }

                string currentScene = EditorApplication.currentScene;
                if (!AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(URL.assets + "/" + lightmapScene)))
                {
                    Debug.LogError("Cannot open the scene:" + filePath);
                    return false;
                }

                if (LightmapSettings.lightmaps == null || LightmapSettings.lightmaps.Length == 0)
                {
                    Debug.LogWarning("There is no lightmap data in the scene:" + filePath);
                    return true;
                }

                LightMapAsset lightmapAsset = ScriptableObject.CreateInstance<LightMapAsset>();
                int count = LightmapSettings.lightmaps.Length;
                lightmapAsset.lightmapFar = new Texture2D[count];
                lightmapAsset.lightmapNear = new Texture2D[count];
                for (int i = 0; i < count; ++i)
                {
                    lightmapAsset.lightmapFar[i] = LightmapSettings.lightmaps[i].lightmapFar;
                    lightmapAsset.lightmapNear[i] = LightmapSettings.lightmaps[i].lightmapNear;
                }
                string assetPath = URL.assets + "/" + filePath + ".lightmap.asset";
                assetBundleData.garbageAssetList.Add(assetPath);
                AssetDatabase.CreateAsset(lightmapAsset, assetPath);
                assetObjectList.Add(AssetDatabase.LoadAssetAtPath(assetPath, typeof(LightMapAsset)));
                assetObjectNameList.Add(filePath + ".lightmap");
                savePathList.Add(filePath + ".lightmap.unity3d");

                if (string.IsNullOrEmpty(currentScene))
                {
                    EditorApplication.NewScene();
                }
                else
                {
                    AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(currentScene));
                }

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

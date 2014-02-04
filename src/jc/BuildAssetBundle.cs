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

        private BuildAssetBundleData.AssetBundleData assetBundleData = null;

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

            if (!BuildAssetBundleData.AssetBundleData.ParseAssetBundleData(configString, out assetBundleData))
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

            ClearGarbage();

            Debug.Log("BuildAssetBundle Complete");
        }

        private void ClearGarbage()
        {
            foreach (string path in assetBundleData.garbageAssetList)
            {
                AssetDatabase.DeleteAsset(path);
            }
        }

        private bool BuildDo()
        {
            foreach (BuildAssetBundleData.AssetData assetData in assetBundleData.assetDataMap.Values)
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

        private bool BuildAsset(BuildAssetBundleData.AssetData assetData)
        {
            if (assetData.IsEmpty())
            {
                return true;
            }

            BuildAssetBundleData.AssetData[] assetDataChildren = null;
            bool hasAssetDataChildren = assetBundleData.TryGetDependenceChildren(assetData.id, out assetDataChildren);
            if (hasAssetDataChildren || assetData.HasDependence())
            {
                BuildPipeline.PushAssetDependencies();
            }

            assetData.enabled = false;
            foreach (BuildAssetBundleData.ItemDataCollection itemDataCollection in assetData.itemDataCollectionList)
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
                foreach (BuildAssetBundleData.AssetData assetDataChild in assetDataChildren)
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

        private bool BuildItemDataCollection(BuildAssetBundleData.ItemDataCollection itemDataCollection)
        {
            List<Object> assetObjectList = new List<Object>();
            List<string> assetObjectNameList = new List<string>();
            List<string> savePathList = new List<string>();

            if (!itemDataCollection.GetBuildParams(assetBundleData, assetObjectList, assetObjectNameList, savePathList))
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
                    Debug.LogError("保存路径存在异常" + ":" + itemDataCollection.savePath);
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
                        Debug.LogError("保存路径存在异常" + ":" + itemDataCollection.savePath + "->" + exception.Message);
                        return false;
                    }
                }

                // isScene
                if (assetObjectList[0] == null)
                {
                    int numAssets = assetObjectList.Count;
                    for (int i = 0; i < numAssets; ++i)
                    {
                        assetObjectNameList[i] = URL.assets + "/" + assetObjectNameList[i];
                    }
                    if(!string.IsNullOrEmpty(BuildPipeline.BuildPlayer(assetObjectNameList.ToArray(), URL.assets + "/" + assetBundleData.saveRoot + "/" + itemDataCollection.savePath, assetBundleData.platform, BuildOptions.BuildAdditionalStreamedScenes)))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!BuildPipeline.BuildAssetBundleExplicitAssetNames(assetObjectList.ToArray(), assetObjectNameList.ToArray(), URL.assets + "/" + assetBundleData.saveRoot + "/" + itemDataCollection.savePath, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, assetBundleData.platform))
                    {
                        return false;
                    }
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
                        Debug.LogError("保存路径存在异常" + ":" + savePath);
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
                            Debug.LogError("保存路径存在异常" + ":" + savePath + "->" + exception.Message);
                            return false;
                        }
                    }
                }

                int length = assetObjectList.Count;
                // isScene
                if (assetObjectList[0] == null)
                {
                    string[] sceneNameList = new string[1];
                    for (int i = 0; i < length; ++i)
                    {
                        sceneNameList[0] = URL.assets + "/" + assetObjectNameList[i];
                        if (!string.IsNullOrEmpty(BuildPipeline.BuildPlayer(sceneNameList, URL.assets + "/" + assetBundleData.saveRoot + "/" + savePathList[i], assetBundleData.platform, BuildOptions.BuildAdditionalStreamedScenes)))
                        {
                            return false;
                        }
                    }
                }
                else
                {
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

            foreach (BuildAssetBundleData.AssetData assetData in assetBundleData.assetDataMap.Values)
            {
                // asset id
                if (assetData.HasDependence())
                {
                    if (!assetBundleData.assetDataMap.ContainsKey(assetData.dependence))
                    {
                        Debug.LogError("依赖的资源未找到" + ":" + assetData.id + "->" + assetData.dependence);
                        return false;
                    }
                    if (assetData.id == assetData.dependence)
                    {
                        Debug.LogError("无法自我依赖" + ":" + assetData.id);
                        return false;
                    }
                }

                // dependence
                if (!DependenceChainChecking(assetData.id))
                {
                    return false;
                }

                // itemDataCollection
                foreach (BuildAssetBundleData.ItemDataCollection itemDataCollection in assetData.itemDataCollectionList)
                {
                    if (itemDataCollection.IsEmpty())
                    {
                        continue;
                    }

                    // itemData
                    foreach (BuildAssetBundleData.ItemData itemData in itemDataCollection.itemDataList)
                    {
                        if (itemData.type == BuildAssetBundleData.ItemType.File)
                        {
                            if (!File.Exists(URL.dataPath + "/" + itemData.path))
                            {
                                Debug.LogError("文件不存在" + ":" + itemData.path);
                                return false;
                            }
                        }
                        else if (itemData.type == BuildAssetBundleData.ItemType.Directory)
                        {
                            if (!Directory.Exists(URL.dataPath + "/" + itemData.path))
                            {
                                Debug.LogError("目录不存在" + ":" + itemData.path);
                                return false;
                            }
                        }
                        else
                        {
                            Debug.LogError("发生未知的错误" + ":" + itemData.type);
                            return false;
                        }
                    }

                    // isScene
                    int sceneCount = 0;
                    int resCount = 0;
                    foreach (BuildAssetBundleData.ItemData itemData in itemDataCollection.itemDataList)
                    {
                        if (itemData.isScene)
                        {
                            ++sceneCount;
                        }
                        else
                        {
                            ++resCount;
                        }
                    }
                    if (sceneCount > 0 && resCount != 0)
                    {
                        Debug.LogError("场景资源不能和普通资源合并");
                        return false;
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
                Debug.Log("无法获取配置文件 " + configPath);
                return false;
            }

            TextAsset configTextAsset = configObj as TextAsset;
            if (configTextAsset == null)
            {
                Debug.Log("无法获取配置文件 " + configPath);
                return false;
            }

            configString = configTextAsset.text;
            return true;
        }

        private bool DependenceChainChecking(string assetDataId)
        {
            Dictionary<string/*assetDataId*/, int> checkingList = new Dictionary<string, int>();
            while (!string.IsNullOrEmpty(assetDataId))
            {
                if (checkingList.ContainsKey(assetDataId))
                {
                    Debug.LogError("非法的递归依赖" + ":" + assetDataId);
                    return false;
                }
                checkingList.Add(assetDataId, 0);

                BuildAssetBundleData.AssetData assetData = null;
                if (!assetBundleData.assetDataMap.TryGetValue(assetDataId, out assetData))
                {
                    Debug.LogError("发生未知的错误" + ":" + assetDataId);
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
    }
}
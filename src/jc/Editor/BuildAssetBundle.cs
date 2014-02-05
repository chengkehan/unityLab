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
        public static string configPathStatic = "JC/Editor/BuildAssetBundle.xml";

        public string configPath = configPathStatic;

        [MenuItem("JC/BuildAssetBundle")]
        private static void Build()
        {
            ScriptableWizard.DisplayWizard<BuildAssetBundle>("BuildAssetBundle", "Create");
        }

        private static void BuildDo()
        {
            BuildAssetBundleData.AssetBundleData assetBundleData = null;

            string configString = null;
            if (!GetConfigString(out configString))
            {
                return;
            }

            if (!BuildAssetBundleData.AssetBundleData.ParseAssetBundleData(configString, out assetBundleData))
            {
                return;
            }

            if (!PreBuildDo(assetBundleData))
            {
                return;
            }

            if (!BuildDo(assetBundleData))
            {
                return;
            }

            ClearGarbage(assetBundleData);

            Debug.Log("BuildAssetBundle Complete");
        }

        private void OnWizardCreate()
        {
            BuildDo();
        }

        private static void ClearGarbage(BuildAssetBundleData.AssetBundleData assetBundleData)
        {
            foreach (string path in assetBundleData.garbageAssetList)
            {
                AssetDatabase.DeleteAsset(path);
            }
        }

        private static bool BuildDo(BuildAssetBundleData.AssetBundleData assetBundleData)
        {
            foreach (BuildAssetBundleData.AssetData assetData in assetBundleData.assetDataMap.Values)
            {
                if (!assetData.enabled || assetData.HasDependence())
                {
                    continue;
                }

                if (!BuildAsset(assetData, assetBundleData))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool BuildAsset(BuildAssetBundleData.AssetData assetData, BuildAssetBundleData.AssetBundleData assetBundleData)
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
                    if (!BuildItemDataCollection(itemDataCollection, assetBundleData))
                    {
                        return false;
                    }
                }
            }

            if (hasAssetDataChildren)
            {
                foreach (BuildAssetBundleData.AssetData assetDataChild in assetDataChildren)
                {
                    BuildAsset(assetDataChild, assetBundleData);
                }
            }

            if (hasAssetDataChildren || assetData.HasDependence())
            {
                BuildPipeline.PopAssetDependencies();
            }
            return true;
        }

        private static bool BuildItemDataCollection(BuildAssetBundleData.ItemDataCollection itemDataCollection, BuildAssetBundleData.AssetBundleData assetBundleData)
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
                    Debug.LogError("Error savePath:" + itemDataCollection.savePath);
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
                        Debug.LogError("Error savePath:" + itemDataCollection.savePath + "->" + exception.Message);
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
                        Debug.LogError("Error savePath:" + savePath);
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
                            Debug.LogError("Error savePath:" + savePath + "->" + exception.Message);
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

        private static bool PreBuildDo(BuildAssetBundleData.AssetBundleData assetBundleData)
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
                        Debug.LogError("Cannot find the dependent asset:" + assetData.id + "->" + assetData.dependence);
                        return false;
                    }
                    if (assetData.id == assetData.dependence)
                    {
                        Debug.LogError("Cannot dependent on self" + ":" + assetData.id);
                        return false;
                    }
                }

                // dependence
                if (!DependenceChainChecking(assetData.id, assetBundleData))
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
                                Debug.LogError("File not exist:" + itemData.path);
                                return false;
                            }
                        }
                        else if (itemData.type == BuildAssetBundleData.ItemType.Directory)
                        {
                            if (!Directory.Exists(URL.dataPath + "/" + itemData.path))
                            {
                                Debug.LogError("Directory not exist:" + itemData.path);
                                return false;
                            }
                        }
                        else
                        {
                            Debug.LogError("Undefined Error:" + itemData.type);
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
                        Debug.LogError("Scene asset cannot build in one bundle with other type asset");
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool GetConfigString(out string configString)
        {
            configString = null;
            Object configObj = AssetDatabase.LoadMainAssetAtPath(URL.assets + "/" + configPathStatic);
            if(configObj == null)
            {
                Debug.Log("Cannot get config " + configPathStatic);
                return false;
            }

            TextAsset configTextAsset = configObj as TextAsset;
            if (configTextAsset == null)
            {
                Debug.Log("Cannot get config " + configPathStatic);
                return false;
            }

            configString = configTextAsset.text;
            return true;
        }

        private static bool DependenceChainChecking(string assetDataId, BuildAssetBundleData.AssetBundleData assetBundleData)
        {
            Dictionary<string/*assetDataId*/, int> checkingList = new Dictionary<string, int>();
            while (!string.IsNullOrEmpty(assetDataId))
            {
                if (checkingList.ContainsKey(assetDataId))
                {
                    Debug.LogError("Illegal recursive dependence:" + assetDataId);
                    return false;
                }
                checkingList.Add(assetDataId, 0);

                BuildAssetBundleData.AssetData assetData = null;
                if (!assetBundleData.assetDataMap.TryGetValue(assetDataId, out assetData))
                {
                    Debug.LogError("Undfined Error:" + assetDataId);
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

        private static bool SplitPath(string path, out string dir, out string name)
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
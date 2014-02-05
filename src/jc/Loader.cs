using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JC
{
    public class Loader : ILoader
    {
        private static bool coroutineWorking = false;

        private static List<Loader> loaderList = new List<Loader>();

        private static List<AssetBundle> garbageList = new List<AssetBundle>();

        private WWW www = null;

        private LoaderRequest request = null;

        private System.Action<LoaderResponse> callback = null;

        private object extraData = null;

        public static void DestroyCache()
        {
            CacheLoader.DestroyCache();
        }

        public static void DestroyGarbage()
        {
            foreach (AssetBundle item in garbageList)
            {
                item.Unload(false);
            }
            garbageList.Clear();
            Resources.UnloadUnusedAssets();
        }

        public static bool LoadSQ(LoaderRequest request, System.Action<LoaderResponse> callback = null, object extraData = null)
        {
            Loader loader = new Loader();
            return loader.Load(request, callback, extraData);
        }

        public Loader()
        {

        }

        public bool Load(LoaderRequest request, System.Action<LoaderResponse> callback = null, object extraData = null)
        {
            if (request == null || string.IsNullOrEmpty(request.url))
            {
                return false;
            }

            Stop();

            this.request = request;
            this.callback = callback;
            this.extraData = extraData;
            loaderList.Add(this);

            www = new WWW(request.url);

            if (!coroutineWorking)
            {
                coroutineWorking = true;
                GlobalCoroutine.Start(LoaderGlobalCoroutine());
            }

            return true;
        }

        public void Stop()
        {
            loaderList.Remove(this);
            if (www != null)
            {
                www.Dispose();
                www = null;
            }
        }

        private static IEnumerator LoaderGlobalCoroutine()
        {
            while (true)
            {
                if(loaderList.Count > 0)
                {
                    foreach(Loader loader in loaderList)
                    {
                        if (loader.www.isDone)
                        {
                            loaderList.Remove(loader);

                            if (string.IsNullOrEmpty(loader.www.error))
                            {
                                Debug.Log("Loader complete:" + loader.request.url);

                                if (loader.callback != null)
                                {
                                    LoaderResponse response = new LoaderResponse();
                                    response.isSuccessful = true;
                                    response.request = loader.request;
                                    response.extraData = loader.extraData;

                                    LoaderType type = loader.request.type;
                                    if (type == LoaderType.Text)
                                    {
                                        response.text = loader.www.text;
                                    }
                                    else if (type == LoaderType.Byte)
                                    {
                                        response.bytes = loader.www.bytes;
                                    }
                                    else if (type == LoaderType.Texture)
                                    {
                                        response.texture2D = loader.www.texture;
                                    }
                                    else if (type == LoaderType.AssetBundle)
                                    {
                                        response.assetBundle = loader.www.assetBundle;
                                        garbageList.Add(response.assetBundle);
                                    }
                                    else
                                    {
                                        // Do nothing
                                    }

                                    loader.callback(response);
                                }
                            }
                            else
                            {
                                Debug.LogError("Loader error:" + loader.www.error);

                                if (loader.callback != null)
                                {
                                    LoaderResponse response = new LoaderResponse();
                                    response.isSuccessful = false;
                                    response.error = loader.www.error;
                                    response.request = loader.request;
                                    response.extraData = loader.extraData;

                                    loader.callback(response);
                                }
                            }
                            break;
                        }
                    }
                }

                yield return null;
            }
        }
    }
}

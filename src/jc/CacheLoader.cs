using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JC
{
    public class CacheLoader : ILoader
    {
        private static Dictionary<string/*url*/, LoaderResponse> cacheMap = new Dictionary<string, LoaderResponse>();

        private Loader loader = null;

        private System.Action<LoaderResponse> callback = null;

        public CacheLoader()
        {
            loader = new Loader();
        }

        public bool Load(LoaderRequest request, System.Action<LoaderResponse> callback = null, object extraData = null)
        {
            if (request == null || string.IsNullOrEmpty(request.url))
            {
                return false;
            }
            else
            {
                LoaderResponse response = null;
                if (cacheMap.TryGetValue(request.url, out response))
                {
                    Debug.Log("CacheLoader complete:" + request.url);

                    if (callback != null)
                    {
                        response.extraData = extraData;
                        callback(response);
                    }
                    return true;
                }
                else
                {
                    if (loader.Load(request, LoaderCallback, extraData))
                    {
                        this.callback = callback;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public void Stop()
        {
            loader.Stop();
        }

        private void LoaderCallback(LoaderResponse response)
        {
            if (response.isSuccessful)
            {
                cacheMap.Add(response.request.url, response);
            }

            if (callback != null)
            {
                callback(response);
            }
        }
    }
}

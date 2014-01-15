using UnityEngine;
using System.Collections;

namespace JC
{
    public delegate void LoaderCallback(LoaderResponse response);

    public interface ILoader
    {
        bool Load(LoaderRequest request, LoaderCallback callback = null, object extraData = null);

        void Stop();
    }

    public class LoaderRequest
    {
        public string url = null;

        public LoaderType type;

        public string[] urlList = null;

        public LoaderType[] typeList = null;

        public LoaderRequest(string url, LoaderType type)
        {
            this.url = url;
            this.type = type;
        }

        public LoaderRequest(string[] urlList, LoaderType[] typeList)
        {
            this.urlList = urlList;
            this.typeList = typeList;
        }
    }

    public class LoaderResponse
    {
        public LoaderRequest request = null;

        public string text = null;

        public byte[] bytes = null;

        public Texture2D texture = null;

        public AssetBundle assetBundle = null;

        public bool isSuccessful = false;

        public string error = null;

        public object extraData = null;

        public LoaderResponse[] responseList = null;

        public bool IsResponseListComplete()
        {
            return responseList == null ? false : responseList[responseList.Length - 1] != null;
        }

        public LoaderResponse GetTheLastLoaderResponse()
        {
            if (responseList == null)
            {
                return null;
            }
            else
            {
                return responseList[responseList.Length - 1];
            }
        }
    }

    public enum LoaderType
    {
        Text,
        Byte,
        Texture,
        AssetBundle
    }
}

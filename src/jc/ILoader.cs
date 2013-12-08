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

        public LoaderRequest(string url, LoaderType type)
        {
            this.url = url;
            this.type = type;
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
    }

    public enum LoaderType
    {
        Text,
        Byte,
        Texture,
        AssetBundle
    }
}

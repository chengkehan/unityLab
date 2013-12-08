using UnityEngine;
using System.Collections;

namespace JC
{
    public class URL
    {
        public static string file = "file:///";

        public static string streamingAssetsPath = Application.streamingAssetsPath;

        public static string persistentDataPath = Application.persistentDataPath;

        public static string dataPath = Application.dataPath;

        public static string GetFileStreamingAssetsPath(string url)
        {
            return file + streamingAssetsPath + "/" + url;
        }
    }
}

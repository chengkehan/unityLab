using UnityEngine;
using System.Collections;

namespace JC
{
    public class URL
    {
        private static string m_file = "file:///";
        public static string file
        {
            get
            {
                return m_file;
            }
        }

        private static string s_streamingAssetsPath = null;
        public static string streamingAssetsPath
        {
            get
            {
                if (s_streamingAssetsPath == null)
                {
                    s_streamingAssetsPath = Application.streamingAssetsPath.Replace('\\', '/');
                }
                return s_streamingAssetsPath;
            }
        }

        private static string s_persistentDataPath = null;
        public static string persistentDataPath
        {
            get
            {
                if (s_persistentDataPath == null)
                {
                    s_persistentDataPath = Application.persistentDataPath.Replace('\\', '/');
                }
                return s_persistentDataPath;
            }
        }

        private static string s_dataPath = null;
        public static string dataPath
        {
            get
            {
                if (s_dataPath == null)
                {
                    s_dataPath = Application.dataPath.Replace('\\', '/');
                }
                return s_dataPath;
            }
        }

        public static string assets = "Assets";

        public static string GetFileStreamingAssetsPath(string url)
        {
            return file + streamingAssetsPath + "/" + url;
        }
    }
}

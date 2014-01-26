using UnityEngine;
using System.Collections;

namespace JC
{
    public class URL
    {
        private static string m_file = null;
        public static string file
        {
            get
            {
                if (m_file == null)
                {
#if UNITY_ANDROID && !UNITY_EDITOR
                    m_file = "jar:file:///";
#else
                    m_file = "file:///";
#endif
                }
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
#if UNITY_ANDROID && !UNITY_EDITOR
                    s_streamingAssetsPath = Application.dataPath + "!/assets";
#else
                    s_streamingAssetsPath = Application.streamingAssetsPath.Replace('\\', '/');
#endif
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

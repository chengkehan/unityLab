using UnityEngine;
using System.Collections;
using System.IO;

namespace JC
{
    public class FileUtil
    {
        public static bool DeletePersistentFile(string path)
        {
            if (PersistentFileExist(path))
            {
                File.Delete(URL.persistentDataPath + "/" + path);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool PersistentFileExist(string path)
        {
            return File.Exists(URL.persistentDataPath + "/" + path);
        }

        public static string ReadStringFromPersistentDataPath(string path)
        {
            return (string)ReadFromPersistentDataPath(path, (url) => {
                Debug.Log("Read text from:" + url);
                return File.ReadAllText(url);
            });
        }

        public static byte[] ReadBytesFromPersistenrDataPath(string path)
        {
            return (byte[])ReadFromPersistentDataPath(path, (url) => {
                Debug.Log("Read bytes from:" + url);
                return File.ReadAllBytes(url);
            });
        }

        public static void WriteStringToPersistentDataPath(string path, string str)
        {
            WriteToPersistentDataPath(path, (url) => {
                Debug.Log("Write text to:" + url);
                File.WriteAllText(url, str);
            });
        }

        public static void WriteBytesToPersistentDataPath(string path, byte[] bytes)
        {
            WriteToPersistentDataPath(path, (url) => {
                Debug.Log("Write bytes to:" + url);
                File.WriteAllBytes(url, bytes);
            });
        }

        private static object ReadFromPersistentDataPath(string path, System.Func<string, object> func)
        {
            string url = URL.persistentDataPath + "/" + path;
            if (File.Exists(url))
            {
                return func(url);
            }
            else
            {
                return null;
            }
        }

        private static void WriteToPersistentDataPath(string path, System.Action<string> action)
        {
            string url = URL.persistentDataPath;
            string newPath = path.Replace('\\', '/');
            string[] newPathBlocks = newPath.Split('/');
            int numBlocks = newPathBlocks.Length;
            for (int i = 0; i < numBlocks; ++i)
            {
                if (i == numBlocks - 1)
                {
                    action(url + "/" + newPath);
                }
                else
                {
                    string dir = url + "/" + newPathBlocks[i];
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
            }
        }
    }
}

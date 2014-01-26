using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace JC
{
    public class StringManager
    {
        public static StringManager GetInstance()
        {
            if (s_instance == null)
            {
                s_instance = new StringManager();
            }
            return s_instance;
        }

        public string GetString(string id)
        {
            string str = null;
            if (stringMap.TryGetValue(id, out str))
            {
                return str;
            }
            else
            {
                return null;
            }
        }

        public string GetFormattedString(string id, params string[] paramList)
        {
            string str = null;
            if (stringMap.TryGetValue(id, out str))
            {
                return string.Format(str, paramList);
            }
            else
            {
                return null;
            }
        }

        public string[] GetStringArray(string id)
        {
            string[] strArr = null;
            if (stringArrayMap.TryGetValue(id, out strArr))
            {
                return strArr;
            }
            else
            {
                return null;
            }
        }

        public bool AddStringConfig(string module, string configString)
        {
            if (string.IsNullOrEmpty(module))
            {
                return false;
            }
            if (string.IsNullOrEmpty(configString))
            {
                return false;
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(configString);

            XmlNodeList stringNodeList = doc.SelectNodes("root/string");
            if (stringNodeList != null)
            {
                foreach (XmlNode stringNode in stringNodeList)
                {
                    string id = stringNode.Attributes.GetNamedItem("id").Value;
                    string value = stringNode.InnerText;
                    stringMap.Add("@" + module + "/" + id, value);
                }
            }

            XmlNodeList stringArrayNodeList = doc.SelectNodes("root/stringArray");
            if (stringArrayNodeList != null)
            {
                foreach (XmlNode stringArrayNode in stringArrayNodeList)
                {
                    string id = stringArrayNode.Attributes.GetNamedItem("id").Value;
                    XmlNodeList itemNodeList = stringArrayNode.SelectNodes("item");
                    if (itemNodeList != null)
                    {
                        string[] stringList = new string[itemNodeList.Count];
                        int itemIndex = 0;
                        foreach (XmlNode itemNode in itemNodeList)
                        {
                            stringList[itemIndex++] = itemNode.InnerText;
                        }
                        stringArrayMap.Add(id, stringList);
                    }
                    else
                    {
                        stringArrayMap.Add(id, new string[0]);
                    }
                }
            }

            return true;
        }

        private StringManager()
        {
            stringMap = new Dictionary<string, string>();
            stringArrayMap = new Dictionary<string, string[]>();
        }

        private static StringManager s_instance = null;

        private Dictionary<string/*id*/, string/*string*/> stringMap = null;

        private Dictionary<string/*id*/, string[]/*stringArray*/> stringArrayMap = null;
    }
}

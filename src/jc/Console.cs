using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JC
{
    public class Console
    {
        public static void Log(object data)
        {
#if JC_CONSOLE
            Init();
            mono.Log(data);
#else
            Debug.Log(data);
#endif
        }

        public static void LogWarning(object data)
        {
#if JC_CONSOLE
            Init();
            mono.LogWarning(data);
#else
            Debug.LogWarning(data);
#endif
        }

        public static void LogError(object data)
        {
#if JC_CONSOLE
            Init();
            mono.LogError(data);
#else
            Debug.LogError(data);
#endif
        }

        public static void LogProtocol(object data)
        {
#if JC_CONSOLE
            Init();
            mono.LogProtocol(data);
#else
            Debug.LogError(data);
#endif
        }

        private static void Init()
        {
            if (go == null)
            {
                go = new GameObject();
                go.name = "__JC_Console__";
                mono = go.AddComponent<ConsoleMono>();

                Application.RegisterLogCallback(ApplicationLogCallback);
            }
        }

        private static void ApplicationLogCallback(string condition, string stackTrace, LogType type)
        {
            Init();
            if (type == LogType.Log)
            {
                mono.Log(condition + "\n" + stackTrace);
            }
            else if (type == LogType.Warning)
            {
                mono.LogWarning(condition + "\n" + stackTrace);
            }
            else
            {
                mono.LogError(condition + "\n" + stackTrace);
            }
        }

        private static GameObject go = null;

        private static ConsoleMono mono = null;
    }

    public class ConsoleMono : MonoBehaviour
    {
        public void Log(object data)
        {
            Init();
            logList[0].AddLog(data);
        }

        public void LogWarning(object data)
        {
            Init();
            logList[1].AddLog(data);
        }

        public void LogError(object data)
        {
            Init();
            logList[2].AddLog(data);
        }

        public void LogProtocol(object data)
        {
            Init();
            logList[3].AddLog(data);
        }

        private void Init()
        {
            if (typeList == null)
            {
                typeList = new List<string>(new string[]{
                    "Log", "Warning", "Error", "Protocol"
                });

                logList = new List<LogData>(new LogData[]{
                    new LogData(Color.white), 
                    new LogData(Color.yellow), 
                    new LogData(Color.red), 
                    new LogData(Color.green)
                });

                tabHeight = Screen.height * 0.05f;
                tabWidth = Screen.width * 0.5f / typeList.Count;
            }
        }

        private void OnGUI()
        {
            if (opened)
            {
                Rect tabRect = new Rect(0.0f, 0.0f, tabWidth, tabHeight);
                int tabIndex = 0;
                foreach (string type in typeList)
                {
                    GUI.color = tabIndex == selectedIndex ? logList[tabIndex].GetColor() : Color.white;
                    GUI.contentColor = logList[tabIndex].GetColor();
                    if (GUI.Button(tabRect, type + "(" + logList[tabIndex].GetLogAmout() + ")"))
                    {
                        selectedIndex = tabIndex;
                    }
                    tabRect.x += tabWidth;
                    ++tabIndex;
                }

                GUI.color = Color.white;
                GUI.contentColor = Color.white;
                if (GUI.Button(new Rect(tabRect.x, 0.0f, tabHeight, tabHeight), "C"))
                {
                    opened = false;
                }

                GUI.contentColor = logList[selectedIndex].GetColor();
                GUI.Box(new Rect(0.0f, tabHeight, tabWidth * logList.Count, Screen.height - tabHeight), string.Empty);
                GUILayout.Space(tabHeight);
                scrollView = GUILayout.BeginScrollView(scrollView, GUILayout.Width(tabWidth * logList.Count), GUILayout.Height(Screen.height - tabHeight));
                GUILayout.Label(logList[selectedIndex].GetCacheLogString());
                GUILayout.EndScrollView();
            }
            else
            {
                GUI.contentColor = Color.white;
                if (GUI.Button(new Rect(0.0f, 0.0f, tabHeight, tabHeight), "C"))
                {
                    opened = true;
                }

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Space(tabHeight);
                int tabIndex = 0;
                foreach (LogData log in logList)
                {
                    GUI.contentColor = logList[tabIndex].GetColor();
                    GUILayout.Label(log.GetLogAmout().ToString(), GUILayout.ExpandWidth(true));
                    ++tabIndex;
                }
                GUILayout.EndHorizontal();
            }
        }

        private List<string> typeList = null;

        private List<LogData> logList = null;

        private bool opened = false;

        private float tabHeight = 0.0f;

        private float tabWidth = 0.0f;

        private int selectedIndex = 0;

        private Vector2 scrollView = Vector2.zero;

        private class LogData
        {
            private const int SIZE = 150;

            private LinkedList<string> logList = null;

            private string cacheLogString = null;

            private Color color;

            private int amout = 0;

            public LogData(Color color)
            {
                this.color = color;
                logList = new LinkedList<string>();
                cacheLogString = string.Empty;
            }

            public void AddLog(object data)
            {
                logList.AddFirst(data == null ? "null" : data.ToString());
                if (logList.Count > SIZE)
                {
                    logList.RemoveLast();
                }

                cacheLogString = string.Empty;
                foreach (string log in logList)
                {
                    cacheLogString += log + "\n\n";
                }

                ++amout;
            }

            public int GetLogAmout()
            {
                return amout;
            }

            public string GetCacheLogString()
            {
                return cacheLogString;
            }

            public Color GetColor()
            {
                return color;
            }
        }
    }
}

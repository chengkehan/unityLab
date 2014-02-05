using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JC
{
    public class Subjects<MessageType> 
    {
        public delegate void Callback(MessageType msgType, object data);

        public Dictionary<MessageType, List<Callback>> msgMap = null;

        private List<Callback> callbackListTemp = null;

        public Subjects()
        {
            msgMap = new Dictionary<MessageType, List<Callback>>();
            callbackListTemp = new List<Callback>();
        }

        public bool Follow(MessageType msgType, Callback callback)
        {
            if (callback == null)
            {
                return false;
            }

            List<Callback> callbackList = null;
            if (!msgMap.TryGetValue(msgType, out callbackList))
            {
                callbackList = new List<Callback>();
                msgMap.Add(msgType, callbackList);
            }
            callbackList.Add(callback);

            return true;
        }

        public bool Unfollow(MessageType msgType, Callback callback)
        {
            if (callback == null)
            {
                return false;
            }

            List<Callback> callbackList = null;
            if (!msgMap.TryGetValue(msgType, out callbackList))
            {
                return false;
            }
            callbackList.Remove(callback);

            return true;
        }

        public void Notify(MessageType msgType, object data = null)
        {
            List<Callback> callbackList = null;
            if (!msgMap.TryGetValue(msgType, out callbackList))
            {
                return;
            }

            if (callbackList.Count == 0)
            {
                return;
            }

            callbackListTemp.Clear();
            callbackListTemp.AddRange(callbackList);

            foreach (Callback callback in callbackListTemp)
            {
                callback(msgType, data);
            }
        }
    }
}

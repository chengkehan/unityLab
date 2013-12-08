using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JC
{
    public class Subjects<MessageType> 
    {
        public delegate void Callback(MessageType msgType, object data);

        public Dictionary<MessageType, LinkedList<Callback>> msgMap = null;

        private LinkedList<Callback> callbackListTemp = null;

        public Subjects()
        {
            msgMap = new Dictionary<MessageType, LinkedList<Callback>>();
            callbackListTemp = new LinkedList<Callback>();
        }

        public bool Follow(MessageType msgType, Callback callback)
        {
            if (callback == null)
            {
                return false;
            }

            LinkedList<Callback> callbackList = null;
            if (!msgMap.TryGetValue(msgType, out callbackList))
            {
                callbackList = new LinkedList<Callback>();
                msgMap.Add(msgType, callbackList);
            }
            callbackList.AddLast(callback);

            return true;
        }

        public bool Unfollow(MessageType msgType, Callback callback)
        {
            if (callback == null)
            {
                return false;
            }

            LinkedList<Callback> callbackList = null;
            if (!msgMap.TryGetValue(msgType, out callbackList))
            {
                return false;
            }
            callbackList.Remove(callback);

            return true;
        }

        public void Notify(MessageType msgType, object data = null)
        {
            LinkedList<Callback> callbackList = null;
            if (!msgMap.TryGetValue(msgType, out callbackList))
            {
                return;
            }

            if (callbackList.Count == 0)
            {
                return;
            }

            callbackListTemp.Clear();
            foreach (Callback callback in callbackList)
            {
                callbackListTemp.AddLast(callback);
            }

            foreach (Callback callback in callbackListTemp)
            {
                callback(msgType, data);
            }
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

namespace JC
{
    public class StrongTypeSubjects
    {
        private Dictionary<System.Type/*type of Subject<Message>*/, object/*Subject*/> subjectsMap = null;

        public StrongTypeSubjects()
        {
            subjectsMap = new Dictionary<System.Type, object>();
        }

        public bool Follow<Message>(Subject<Message>.MessageCallback callback)
        {
            if (callback == null)
            {
                return false;
            }
            else
            {
                System.Type type = typeof(Subject<Message>);
                object subjectObj = null;
                Subject<Message> subject = null;

                if (subjectsMap.TryGetValue(type, out subjectObj))
                {
                    subject = (Subject<Message>)subjectObj;
                }
                else
                {
                    subject = new Subject<Message>();
                    subjectObj = (object)subject;
                    subjectsMap.Add(type, subjectObj);
                }

                return subject.Follow(callback);
            }
        }

        public bool Unfollow<Message>(Subject<Message>.MessageCallback callback)
        {
            if (callback == null)
            {
                return false;
            }
            else
            {
                System.Type type = typeof(Subject<Message>);
                object subjectObj = null;
                Subject<Message> subject = null;

                if (subjectsMap.TryGetValue(type, out subjectObj))
                {
                    subject = (Subject<Message>)subjectObj;
                    return subject.Unfollow(callback);
                }
                else
                {
                    return false;
                }
            }
        }

        public bool Notify<Message>(Message message = default(Message))
        {
            System.Type type = typeof(Subject<Message>);
            object subjectObj = null;
            Subject<Message> subject = null;

            if (subjectsMap.TryGetValue(type, out subjectObj))
            {
                subject = (Subject<Message>)subjectObj;
                subject.Notify(message);
                return true;
            }
            else
            {
                return false;
            }
        }

        public class Subject<Message>
        {
            public delegate void MessageCallback(Message message);

            private List<MessageCallback> callbackList = null;

            private List<MessageCallback> callbackCopyList = null;

            public Subject()
            {
                callbackList = new List<MessageCallback>();
                callbackCopyList = new List<MessageCallback>();
            }

            public bool Follow(MessageCallback callback)
            {
                if (callback == null)
                {
                    return false;
                }
                else
                {
                    callbackList.Add(callback);
                    return true;
                }
            }

            public bool Unfollow(MessageCallback callback)
            {
                if (callback == null)
                {
                    return false;
                }
                else
                {
                    return callbackList.Remove(callback);
                }
            }

            public void Notify(Message message = default(Message))
            {
                if (callbackList.Count > 0)
                {
                    callbackCopyList.Clear();
                    callbackCopyList.AddRange(callbackList);
                    foreach (MessageCallback callback in callbackCopyList)
                    {
                        callback(message);
                    }
                }
            }
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JC
{
    public class LoaderQueueBase<LoaderType> : ILoader
        where LoaderType : ILoader, new()
    {
        private LoaderType loader = default(LoaderType);

        private LinkedList<ItemOfQueue> requestList = null;

        private bool working = false;

        public LoaderQueueBase()
        {
            requestList = new LinkedList<ItemOfQueue>();
            loader = new LoaderType();
        }

        public bool Load(LoaderRequest request, LoaderCallback callback = null, object extraData = null)
        {
            if (request == null)
            {
                return false;
            }

            ItemOfQueue item = new ItemOfQueue();
            item.request = request;
            item.callback = callback;
            item.extraData = extraData;
            requestList.AddLast(item);

            LoadDo();

            return true;
        }

        public void Stop()
        {
            loader.Stop();
            requestList.Clear();
            working = false;
        }

        private void LoadDo()
        {
            if (working || requestList.Count == 0)
            {
                return;
            }

            working = true;
            ItemOfQueue item = requestList.First.Value;
            if (!loader.Load(item.request, LoaderCallback, item))
            {
                working = false;
                requestList.RemoveFirst();
                LoadDo();
            }
        }

        private void LoaderCallback(LoaderResponse response)
        {
            ItemOfQueue item = (ItemOfQueue)response.extraData;
            if (item.callback != null)
            {
                response.extraData = item.extraData;
                item.callback(response);
            }

            working = false;
            requestList.RemoveFirst();
            LoadDo();
        }

        private class ItemOfQueue
        {
            public LoaderRequest request = null;

            public LoaderCallback callback = null;

            public object extraData = null;
        }
    }
}
using UnityEngine;
using System.Collections;

namespace JC
{
    public class LoaderBundleBase<LoaderQueueType, LoaderType> : ILoader
        where LoaderQueueType : LoaderQueueBase<LoaderType>, new() 
        where LoaderType : ILoader, new()
    {
        public LoaderBundleBase()
        {
            loader = new LoaderQueueType();
        }

        public bool Load(LoaderRequest request, LoaderCallback callback = null, object extraData = null)
        {
            if (request == null || request.urlList == null || request.urlList.Length == 0)
            {
                return false;
            }

            Stop();

            response = new LoaderResponse();
            responseIndex = 0;
            response.responseList = new LoaderResponse[request.urlList.Length];

            int requestCount = request.urlList.Length;
            for (int i = 0; i < requestCount; ++i)
            {
                ExtraData data = new ExtraData();
                data.callback = callback;
                data.extraData = extraData;
                loader.Load(new LoaderRequest(request.urlList[i], request.typeList[i]), CompleteCallback, data);
            }

            return true;
        }

        public void Stop()
        {
            loader.Stop();
            response = null;
        }

        private void CompleteCallback(LoaderResponse response)
        {
            ExtraData data = (ExtraData)response.extraData;
            response.extraData = data.extraData;
            this.response.responseList[responseIndex++] = response;
            if (data.callback != null)
            {
                // Item Complete
                data.callback(response);

                // All Complete
                if (this.response.responseList[this.response.responseList.Length - 1] != null)
                {
                    data.callback(this.response);
                }
            }
        }

        private LoaderQueueType loader = default(LoaderQueueType);

        private LoaderResponse response = null;

        private int responseIndex = 0;

        private class ExtraData
        {
            public LoaderCallback callback = null;

            public object extraData = null;
        }
    }
}

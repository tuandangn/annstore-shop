using System;

namespace Annstore.Web.Infrastructure
{
    [Serializable]
    public sealed class AppRequest<T>
    {
        public AppRequest(){}

        public AppRequest(T data)
        {
            Data = data;
        }

        public T Data { get; set; }
    }
}

using System;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Services.Communication
{
    public interface IAsynCommService<in T> : ICommService<T>
        where T : ICommMessage
    {
        void SendCommunication(T message, Action<string> onSuccess, Action<Exception> onError);
    }
}

using System;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class GetActiveEnvironmentCallbackMessage : ICallBackMessage<IEnvironmentModel>
    {
        public Action<IEnvironmentModel> Callback { get; set; }

        public GetActiveEnvironmentCallbackMessage(Action<IEnvironmentModel> callback)
        {
            Callback = callback;
        }
    }

    public interface ICallBackMessage<T> where T : class
    {
        Action<T> Callback { get; set; }
    }
}

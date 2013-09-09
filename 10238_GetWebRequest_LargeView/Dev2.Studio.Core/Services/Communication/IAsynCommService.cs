using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Studio.Core.Services.Communication
{
    public interface IAsynCommService<in T> : ICommService<T> 
        where T : ICommMessage
    {
        void SendCommunication(T message, Action<string> onSuccess, Action<Exception> onError);
    }
}

using System;

namespace Dev2.Studio.Core.Services.Communication
{
    public interface ICommService<in T> where T: ICommMessage
    {
        void SendCommunication(T message);
    }
}

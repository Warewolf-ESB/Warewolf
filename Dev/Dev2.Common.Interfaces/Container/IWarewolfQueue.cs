using System;

namespace Dev2.Common.Interfaces.Container
{
    public interface IWarewolfQueue : IDisposable
    {
        IWarewolfQueueSession OpenSession();
    }

    public interface IWarewolfQueueSession : IDisposable
    {
        void Enqueue<T>(T ob);

        T Dequeue<T>() where T : class;

        void Flush();
    }
}

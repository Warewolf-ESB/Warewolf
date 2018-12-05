using System;

namespace Dev2.Common.Interfaces.Container
{
    public interface IWarewolfQueue : IDisposable
    {
        IWarewolfQueueSession OpenSession();
        bool IsEmpty();
    }

    public interface IWarewolfQueueSession : IDisposable
    {
        IWarewolfQueueSession Enqueue<T>(T ob);

        T Dequeue<T>() where T : class;

        void Flush();
    }
}

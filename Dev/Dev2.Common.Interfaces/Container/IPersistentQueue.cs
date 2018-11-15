using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Common.Interfaces.Container
{
    public interface IPersistentQueue : IDisposable
    {
        IPersistentQueueSession OpenSession();
    }

    public interface IPersistentQueueSession : IDisposable
    {
        void Enqueue<T>(T ob);

        T Dequeue<T>();

        void Flush();
    }
}

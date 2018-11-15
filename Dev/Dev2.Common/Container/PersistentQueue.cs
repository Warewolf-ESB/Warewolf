using Dev2.Common.Interfaces.Container;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace Dev2.Common.Container
{
    public class PersistentQueue : IPersistentQueue
    {
        readonly DiskQueue.PersistentQueue persistentQueue;

        public PersistentQueue(string name)
        {
            persistentQueue = new DiskQueue.PersistentQueue(name);
        }

        public IPersistentQueueSession OpenSession()
        {
            return new Session(persistentQueue.OpenSession());
        }

        private bool isDisposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    persistentQueue.Dispose();
                }
                isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }

    public class Session : IPersistentQueueSession {
        readonly DiskQueue.IPersistentQueueSession queueSession;
        public Session(DiskQueue.IPersistentQueueSession queueSession)
        {
            this.queueSession = queueSession;
        }
        public void Enqueue<T>(T ob)
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ob));
            queueSession.Enqueue(data);
        }
        public T Dequeue<T>()
        {
            var data = queueSession.Dequeue();
            if (data is null)
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
        }

        public void Flush()
        {
            queueSession.Flush();
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    queueSession.Dispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
    }
}

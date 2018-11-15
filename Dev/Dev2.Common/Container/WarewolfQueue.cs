using Dev2.Common.Interfaces.Container;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Common.Container
{
    public class WarewolfQueue : IWarewolfQueue
    {
        readonly ConcurrentQueue<byte[]> queue = new ConcurrentQueue<byte[]>();

        public IWarewolfQueueSession OpenSession()
        {
            return new WarewolfQueueSession(queue);
        }

        public void Dispose()
        {
        }
    }

    public class WarewolfQueueSession : IWarewolfQueueSession
    {
        readonly ConcurrentQueue<byte[]> queue;
        readonly IList<byte[]> buffer = new List<byte[]>();

        public WarewolfQueueSession(ConcurrentQueue<byte[]> queue)
        {
            this.queue = queue;
        }
        public void Enqueue<T>(T ob)
        {
            var json = JsonConvert.SerializeObject(ob);
            buffer.Add(Encoding.UTF8.GetBytes(json));
        }
        public T Dequeue<T>()
        {
            if (!queue.TryDequeue(out byte[] data))
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
        }

        public virtual void Flush()
        {
            var tmpCopy = buffer.ToList();
            foreach (var item in tmpCopy)
            {
                queue.Enqueue(item);
                buffer.Remove(item);
            }
        }

        public void Dispose()
        {
        }
    }
}

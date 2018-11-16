using Dev2.Common.Interfaces.Container;
using Dev2.Common.Serializers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Common.Container
{
    public class WarewolfQueue : IWarewolfQueue
    {
        readonly ConcurrentQueue<byte[]> _queue = new ConcurrentQueue<byte[]>();

        public IWarewolfQueueSession OpenSession()
        {
            return new WarewolfQueueSession(_queue);
        }

        public void Dispose()
        {
        }
    }

    public class WarewolfQueueSession : IWarewolfQueueSession
    {
        readonly ConcurrentQueue<byte[]> _queue;
        readonly IList<byte[]> _buffer = new List<byte[]>();

        public WarewolfQueueSession(ConcurrentQueue<byte[]> queue)
        {
            this._queue = queue;
        }
        public void Enqueue<T>(T ob)
        {
            var jsonSerializer = new Dev2JsonSerializer();
            var builder = jsonSerializer.Serialize<T>(ob);
            _buffer.Add(Encoding.UTF8.GetBytes(builder));
        }
        public T Dequeue<T>()
        {
            if (!_queue.TryDequeue(out byte[] data))
            {
                return default(T);
            }
            var jsonSerializer = new Dev2JsonSerializer();
            return jsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(data));
        }

        public virtual void Flush()
        {
            var tmpCopy = _buffer.ToList();
            foreach (var item in tmpCopy)
            {
                _queue.Enqueue(item);
                _buffer.Remove(item);
            }
        }

        public void Dispose()
        {
        }
    }
}

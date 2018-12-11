using Dev2.Common.Interfaces.Container;
using Dev2.Common.Serializers;
using MessagePack;
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

        public bool IsEmpty()
        {
            return !_queue.Any();
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
        public IWarewolfQueueSession Enqueue<T>(T ob)
        {
            _buffer.Add(MessagePackSerializer.Serialize<T>(ob));
            return this;
        }
        public T Dequeue<T>() where T : class
        {
            if (!_queue.TryDequeue(out byte[] data))
            {
                return default(T);
            }
            return MessagePackSerializer.Deserialize<T>(data);
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

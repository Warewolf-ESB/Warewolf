using System;
using System.Threading.Tasks;
using Warewolf.Data;

namespace Warewolf.Streams
{
    internal class DeserializingConsumer<T> : IConsumer
    {
        private readonly IConsumer<T> _consumer;
        private readonly IDeserializer _deserializer;

        public DeserializingConsumer(IDeserializer deserializer, IConsumer<T> consumer)
        {
            _deserializer = deserializer;
            _consumer = consumer;
        }

        public Task<ConsumerResult> Consume(byte[] body, object parameters)
        {
            var item = _deserializer.Deserialize<T>(body);
            return _consumer.Consume(item, parameters);
        }
    }
}

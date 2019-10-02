namespace Warewolf.Streams
{
    internal class SerializingPublisher<T> : IPublisher<T>
    {
        private IPublisher _publisher;
        private ISerializer _serializer;

        public SerializingPublisher(IPublisher publisher, ISerializer serializer)
        {
            _publisher = publisher;
            _serializer = serializer;
        }

        public void Publish(T value) => _publisher.Publish(_serializer.Serialize(value));
    }
}

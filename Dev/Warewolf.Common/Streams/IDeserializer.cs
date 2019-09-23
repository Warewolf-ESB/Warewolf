namespace Warewolf.Streams
{
    public interface IDeserializer
    {
        T Deserialize<T>(byte[] value);
    }
}

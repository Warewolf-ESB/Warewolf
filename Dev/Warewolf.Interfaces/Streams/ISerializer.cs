namespace Warewolf.Streams
{
    public interface ISerializer
    {
        byte[] Serialize<T>(T value);
    }
}

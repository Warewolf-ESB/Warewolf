using Warewolf.Data;

namespace Warewolf.Debugging
{
    public interface INotificationListener<in T> where T : INotification
    {
        void Write(T notification);
    }
}

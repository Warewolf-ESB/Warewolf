using Warewolf.Data;
using Warewolf.Debugging;

namespace Warewolf.Esb
{
    public interface IEsbHub : INotificationListener<ChangeNotification>
    {
    }
}
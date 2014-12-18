using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces
{
    public interface IServer
    {
        bool Connect();
        IList<IResource> Load();
        IList<IConnection> GetServerConnections();
    }
}
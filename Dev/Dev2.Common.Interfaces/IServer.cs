using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Toolbox;

namespace Dev2.Common.Interfaces
{
    public interface IServer
    {
        bool Connect();
        IList<IResource> Load();
        IList<IServer> GetServerConnections();
        IList<IToolDescriptor> LoadTools();
        bool IsConnected();
        void ReloadTools();
        void Disconnect();
        void Edit();
    }
}
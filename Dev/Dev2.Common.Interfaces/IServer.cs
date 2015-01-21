using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Toolbox;

namespace Dev2.Common.Interfaces
{
    public interface IServer
    {
        Task<bool> Connect();
        IList<IResource> Load();
        IList<IServer> GetServerConnections();
        IList<IToolDescriptor> LoadTools();
        IExplorerRepository ExplorerRepository { get; }
        bool IsConnected();
        void ReloadTools();
        void Disconnect();
        void Edit();
    }
}
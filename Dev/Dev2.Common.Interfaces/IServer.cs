using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Toolbox;
using Newtonsoft.Json;

namespace Dev2.Common.Interfaces
{
    public interface IServer:IResource
    {
        Task<bool> Connect();
        List<IResource> Load();
        Task<IExplorerItem> LoadExplorer();
        IList<IServer> GetServerConnections();
        IList<IToolDescriptor> LoadTools();
        [JsonIgnore]
        IExplorerRepository ExplorerRepository { get; }
        [JsonIgnore]
        IStudioUpdateManager UpdateRepository { get; }
        [JsonIgnore]
        IQueryManager QueryProxy { get; }
        bool IsConnected();
        void ReloadTools();
        void Disconnect();
        void Edit();
        List<IWindowsGroupPermission> Permissions { get; } 

        event PermissionsChanged PermissionsChanged;
        event NetworkStateChanged NetworkStateChanged;
        event ItemAddedEvent ItemAddedEvent;
       
        string GetServerVersion();

       
    }

    public delegate void PermissionsChanged(PermissionsChangedArgs args);
    public delegate void NetworkStateChanged(INetworkStateChangedEventArgs args);
    public delegate void ItemAddedEvent(IExplorerItem args);
}
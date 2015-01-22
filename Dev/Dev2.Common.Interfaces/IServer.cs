using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Toolbox;

namespace Dev2.Common.Interfaces
{
    public interface IServer:IResource
    {
        Task<bool> Connect();
        List<IResource> Load();
        Task<IExplorerItem> LoadExplorer();
        IList<IServer> GetServerConnections();
        IList<IToolDescriptor> LoadTools();
        bool IsConnected();
        void ReloadTools();
        void Disconnect();
        void Edit();

        event PermissionsChanged PermissionsChanged;
    }

    public delegate void PermissionsChanged(PermissionsChangedArgs args);

    public class PermissionsChangedArgs
    {
        public List<IWindowsGroupPermission> Permissions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public PermissionsChangedArgs(List<IWindowsGroupPermission> permissions)
        {
            Permissions = permissions;
        }
    }
}
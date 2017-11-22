using System.Collections.Generic;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;

namespace Dev2.Common.Interfaces
{
    public delegate void PermissionsChanged(PermissionsChangedArgs args);

    public class PermissionsChangedArgs
    {
        public List<IWindowsGroupPermission> WindowsGroupPermissions { get; set; }

        public PermissionsChangedArgs(List<IWindowsGroupPermission> windowsGroupPermissions)
        {
            WindowsGroupPermissions = windowsGroupPermissions;
        }
    }

    public delegate void ItemAddedEvent(IExplorerItem args);
}
using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure;

namespace Dev2.Common.Interfaces
{
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
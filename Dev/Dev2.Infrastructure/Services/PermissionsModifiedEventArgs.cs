using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure;

namespace Dev2.Services
{
    public class PermissionsModifiedEventArgs
    {
        public List<IWindowsGroupPermission> ModifiedWindowsGroupPermissions { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public PermissionsModifiedEventArgs(List<IWindowsGroupPermission> modifiedWindowsGroupPermissions)
        {
            ModifiedWindowsGroupPermissions = modifiedWindowsGroupPermissions;
        }
    }
}
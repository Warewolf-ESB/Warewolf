using System;
using System.Collections.Generic;

namespace Dev2.Services.Security
{
    public class SecuritySettingsTO
    {
        public SecuritySettingsTO()
        {
            WindowsGroupPermissions = new List<WindowsGroupPermission>();
        }

        public SecuritySettingsTO(IEnumerable<WindowsGroupPermission> permissions)
            : this()
        {
            if(permissions != null)
            {
                WindowsGroupPermissions.AddRange(permissions);
            }
        }

        public List<WindowsGroupPermission> WindowsGroupPermissions { get; private set; }
        public TimeSpan CacheTimeout { get; set; }
    }
}

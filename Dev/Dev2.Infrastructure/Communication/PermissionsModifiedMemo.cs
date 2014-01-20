using System.Collections.Generic;
using Dev2.Services.Security;

namespace Dev2.Communication
{
    public class PermissionsModifiedMemo : Memo
    {
        public PermissionsModifiedMemo()
        {
            ModifiedPermissions = new List<WindowsGroupPermission>();
        }
        public List<WindowsGroupPermission> ModifiedPermissions { get; set; }
    }
}
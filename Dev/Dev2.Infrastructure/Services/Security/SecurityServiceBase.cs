using System;
using System.Collections.Generic;

namespace Dev2.Services.Security
{
    public abstract class SecurityServiceBase : DisposableObject, ISecurityService
    {
        readonly List<WindowsGroupPermission> _permissions = new List<WindowsGroupPermission>();
        
        public event EventHandler PermissionsChanged;
        
        public IReadOnlyList<WindowsGroupPermission> Permissions { get { return _permissions; } }

        public virtual void Read()
        {
            _permissions.Clear();
            var json = ReadPermissions();
            if(json != null)
            {
                _permissions.AddRange(json);
            }

            RaisePermissionsChanged();
        }

        protected virtual void RaisePermissionsChanged()
        {
            if(PermissionsChanged != null)
            {
                PermissionsChanged(this, EventArgs.Empty);
            }
        }

        protected abstract List<WindowsGroupPermission> ReadPermissions();
    }
}
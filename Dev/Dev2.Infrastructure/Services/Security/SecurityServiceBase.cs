
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Dev2.Common.Interfaces.Infrastructure;

namespace Dev2.Services.Security
{
    public abstract class SecurityServiceBase : DisposableObject, ISecurityService
    {
        readonly ReaderWriterLockSlim _permissionsLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected List<IWindowsGroupPermission> _permissions = new List<IWindowsGroupPermission>();

        public event EventHandler PermissionsChanged;

        public event EventHandler<PermissionsModifiedEventArgs> PermissionsModified;

        public IReadOnlyList<IWindowsGroupPermission> Permissions
        {
            get
            {
                _permissionsLock.EnterReadLock();
                try
                {
                    return _permissions;
                }
                finally
                {
                    _permissionsLock.ExitReadLock();
                }
            }
            set
            {
                _permissionsLock.EnterReadLock();
                try
                {
                    _permissions = value.ToList();
                }
                finally
                {
                    _permissionsLock.ExitReadLock();
                }
            }
        }

        public TimeSpan TimeOutPeriod { get; set; }

        public void Remove(Guid resourceId)
        {
            LogStart();
            _permissionsLock.EnterWriteLock();
            try
            {
                var oldPermissions = new WindowsGroupPermission[_permissions.Count];
                _permissions.CopyTo(oldPermissions);

                var removedCount = _permissions.RemoveAll(p => !p.IsServer && p.ResourceID == resourceId);

                if(removedCount > 0)
                {
                    RaisePermissionsModified(oldPermissions, _permissions);

                    // This will trigger a FileSystemWatcher file changed event
                    // which in turn will cause the permissions to be re-read.
                    WritePermissions(_permissions);
                }
            }
            finally
            {
                _permissionsLock.ExitWriteLock();
            }
            LogEnd();
        }

        public virtual void Read()
        {
            LogStart();
            _permissionsLock.EnterWriteLock();
            var previousPermissions = _permissions.ToList();
            List<IWindowsGroupPermission> newPermissions;
            try
            {
                newPermissions = ReadPermissions();
                _permissions.Clear();
                if(newPermissions != null)
                {
                    _permissions.AddRange(newPermissions);
                }
            }
            finally
            {
                _permissionsLock.ExitWriteLock();
            }
            if(newPermissions != null)
            {
                RaisePermissionsModified(previousPermissions, newPermissions);
            }
            RaisePermissionsChanged();
            LogEnd();
        }

        void RaisePermissionsModified(IEnumerable<IWindowsGroupPermission> oldPermissions, IEnumerable<IWindowsGroupPermission> newPermissions)
        {
            if(oldPermissions != null && newPermissions != null)
            {
                RaisePermissionsModified(new PermissionsModifiedEventArgs(newPermissions.ToList()));
            }
        }

        protected virtual void RaisePermissionsChanged()
        {
            LogStart();
            var handler = PermissionsChanged;
            if(handler != null)
            {
                handler(this, EventArgs.Empty);
            }
            LogEnd();
        }

        protected virtual void RaisePermissionsModified(PermissionsModifiedEventArgs e)
        {
            LogStart();
            var handler = PermissionsModified;
            if(handler != null)
            {
                handler(this, e);
            }
            LogEnd();
        }

        protected abstract List<IWindowsGroupPermission> ReadPermissions();
        protected abstract void WritePermissions(List<IWindowsGroupPermission> permissions);
        protected abstract void LogStart([CallerMemberName] string methodName = null);
        protected abstract void LogEnd([CallerMemberName] string methodName = null);

    }
}

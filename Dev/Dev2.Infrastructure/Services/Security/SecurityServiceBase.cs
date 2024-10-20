#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using Warewolf;
using Warewolf.Data;

namespace Dev2.Services.Security
{
    public abstract class SecurityServiceBase : DisposableObject, ISecurityService
    {
        readonly ReaderWriterLockSlim _permissionsLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected List<WindowsGroupPermission> _permissions = new List<WindowsGroupPermission>();
        protected INamedGuid _overrideResource = new NamedGuid();
        private string _secretKey = "";
        public event EventHandler PermissionsChanged;

        public event EventHandler<PermissionsModifiedEventArgs> PermissionsModified;

        public string SecretKey
        {
            get { return _secretKey; }
            set { _secretKey = value; }
        }

        public INamedGuid OverrideResource
        {
            get { return _overrideResource; }
            set { _overrideResource = value; }
        }

        public IReadOnlyList<WindowsGroupPermission> Permissions
        {
            get
            {
                _permissionsLock.EnterReadLock();
                try
                {
                    return _permissions.ToList();
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

                if (removedCount > 0)
                {
                    RaisePermissionsModified(oldPermissions, _permissions);

                    // This will trigger a FileSystemWatcher file changed event
                    // which in turn will cause the permissions to be re-read.
                    WritePermissions(_permissions, _overrideResource,_secretKey);
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
            List<WindowsGroupPermission> newPermissions;
            try
            {
                newPermissions = ReadPermissions();
                _permissions.Clear();
                if (newPermissions != null)
                {
                    _permissions.AddRange(newPermissions);
                }
            }
            finally
            {
                _permissionsLock.ExitWriteLock();
            }

            if (newPermissions != null)
            {
                RaisePermissionsModified(previousPermissions, newPermissions);
            }

            RaisePermissionsChanged();
            LogEnd();
        }

        void RaisePermissionsModified(IEnumerable<WindowsGroupPermission> oldPermissions, IEnumerable<WindowsGroupPermission> newPermissions)
        {
            if (oldPermissions != null && newPermissions != null)
            {
                RaisePermissionsModified(new PermissionsModifiedEventArgs(newPermissions.ToList()));
            }
        }

        protected virtual void RaisePermissionsChanged()
        {
            LogStart();
            var handler = PermissionsChanged;
            handler?.Invoke(this, EventArgs.Empty);
            LogEnd();
        }

        protected virtual void RaisePermissionsModified(PermissionsModifiedEventArgs e)
        {
            LogStart();
            PermissionsModified?.Invoke(this, e);
            LogEnd();
        }

        protected abstract List<WindowsGroupPermission> ReadPermissions();
        protected abstract void WritePermissions(List<WindowsGroupPermission> permissions, INamedGuid overrideResource,string secretKey);
        protected abstract void LogStart([CallerMemberName] string methodName = null);
        protected abstract void LogEnd([CallerMemberName] string methodName = null);
    }

    public class PermissionsModifiedEventArgs
    {
        public List<WindowsGroupPermission> ModifiedWindowsGroupPermissions { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public PermissionsModifiedEventArgs(List<WindowsGroupPermission> modifiedWindowsGroupPermissions)
        {
            ModifiedWindowsGroupPermissions = modifiedWindowsGroupPermissions;
        }
    }
}
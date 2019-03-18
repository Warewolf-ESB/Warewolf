#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Services.Security
{
    public abstract class SecurityServiceBase : DisposableObject, ISecurityService
    {
        readonly ReaderWriterLockSlim _permissionsLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected List<WindowsGroupPermission> _permissions = new List<WindowsGroupPermission>();

        public event EventHandler PermissionsChanged;

        public event EventHandler<PermissionsModifiedEventArgs> PermissionsModified;

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
            List<WindowsGroupPermission> newPermissions;
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

        void RaisePermissionsModified(IEnumerable<WindowsGroupPermission> oldPermissions, IEnumerable<WindowsGroupPermission> newPermissions)
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
        protected abstract void WritePermissions(List<WindowsGroupPermission> permissions);
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

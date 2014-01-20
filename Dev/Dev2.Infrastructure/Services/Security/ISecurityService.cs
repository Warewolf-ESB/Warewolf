using System;
using System.Collections.Generic;

namespace Dev2.Services.Security
{
    public interface ISecurityService : IDisposable
    {
        event EventHandler PermissionsChanged;

        IReadOnlyList<WindowsGroupPermission> Permissions { get; }
        TimeSpan TimeOutPeriod { get; set; }

        void Read();
        event EventHandler<PermissionsModifiedEventArgs> PermissionsModified;

        void Remove(Guid resourceID);
    }
}
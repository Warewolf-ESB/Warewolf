using System;
using System.Collections.Generic;

namespace Dev2.Services.Security
{
    public interface ISecurityService : IDisposable
    {
        event EventHandler PermissionsChanged;

        IReadOnlyList<WindowsGroupPermission> Permissions { get; }

        void Read();
    }
}
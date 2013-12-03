using System;
using System.Collections.Generic;

namespace Dev2.Services.Security
{
    public interface ISecurityService : IDisposable
    {
        event EventHandler Changed;
        IReadOnlyList<WindowsGroupPermission> Permissions { get; }
    }
}
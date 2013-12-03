using System;
using System.Collections.Generic;

namespace Dev2.Services.Security
{
    public interface ISecurityConfigService : IDisposable
    {
        event EventHandler Changed;
        IReadOnlyList<WindowsGroupPermission> Permissions { get; }
    }
}
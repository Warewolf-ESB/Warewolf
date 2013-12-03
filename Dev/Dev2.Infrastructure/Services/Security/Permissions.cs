using System;

namespace Dev2.Services.Security
{
    [Flags]
    public enum Permissions
    {
        None = 0,
        View = 1,
        Execute = 2,
        Contribute = 4,
        DeployTo = 8,
        DeployFrom = 16,
        Administrator = 32
    }
}
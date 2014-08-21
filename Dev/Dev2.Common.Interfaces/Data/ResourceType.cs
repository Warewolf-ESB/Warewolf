using System;

namespace Dev2.Common.Interfaces.Data
{
    [Flags]
    public enum ResourceType
    {
        Unknown = 0,
        WorkflowService = 1,
        DbService = 2,
        Version = 3,
        PluginService = 4,
        WebService = 8,
        DbSource = 16,
        PluginSource = 32,
        WebSource = 64,
        EmailSource = 128,
        ServerSource = 256,
        Folder = 512,
        Server = 1024,
        ReservedService = 2048,
        Message = 3069,
    }
}
using System;

// ReSharper disable CheckNamespace
namespace Dev2.Data.ServiceModel
// ReSharper restore CheckNamespace
{
    [Flags]
    public enum ResourceType
    {
        Unknown = 0,
        WorkflowService = 1,
        DbService = 2,
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
    }
}
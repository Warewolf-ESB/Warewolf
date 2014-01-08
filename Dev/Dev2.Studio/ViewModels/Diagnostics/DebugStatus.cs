using System.ComponentModel;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.Diagnostics
{
    public enum DebugStatus
    {
        [Description("Ready")]
        Ready,

        [Description("Configuring")]
        Configure,

        [Description("Executing")]
        Executing,

        [Description("Stopping")]
        Stopping,

        [Description("Ready")]
        Finished
    }
}

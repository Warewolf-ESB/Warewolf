using System.ComponentModel;

namespace Dev2.Settings.Logging
{
    public enum LogLevel
    {
        // ReSharper disable InconsistentNaming
        [Description("None: No logging")]
        OFF,
        [Description("Fatal: Only log events that are fatal")]
        FATAL,
        [Description("Error: Log fatal and error events")]
        ERROR,
        [Description("Warn: Log error, fatal and warning events")]
        WARN,
        [Description("Info: Log system info including pulse data, fatal, error and warning events")]
        INFO,
        [Description("Debug: Log all system activity including executions. Also logs fatal, error, warning and info events")]
        DEBUG,
        [Description("Trace: Log detailed system information. Includes events from all the levels above")]
        TRACE
    }
}
using System;
using System.ComponentModel;

namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    [Serializable]
    public enum DebugItemResultType
    {
        Label,
        Variable,
        Value
    }

    public enum ActivityType
    {
        [Description("Workflow")]
        Workflow,
        [Description("Step")]
        Step,
        [Description("Service")]
        Service
    }

    public enum ExecutionOrigin
    {
        [Description("Unknown")]
        Unknown,
        [Description("Workflow")]
        Workflow,
        [Description("Debug")]
        Debug,
        [Description("External")]
        External
    }

    [Flags]
    public enum StateType
    {
        None = 0,
        Before = 1,
        After = 2,
        Append = 4,
        Message = 8,
        Clear = 16,
        Start = 32,
        End = 64,
        All = 128
    }
}


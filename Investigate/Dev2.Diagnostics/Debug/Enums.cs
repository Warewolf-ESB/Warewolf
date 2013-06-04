
using System;
using System.ComponentModel;
using Dev2.Common;

namespace Dev2.Diagnostics
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


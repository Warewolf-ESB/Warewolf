
using System;
using Dev2.Common;

namespace Dev2.Enums {
    [Flags]
    public enum StateType {
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

    [EnumDisplayString("Workflow", "Step", "Service")]
    public enum ActivityType {
        Workflow,
        Step,
        Service
    }
}
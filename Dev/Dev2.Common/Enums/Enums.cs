
using System;
using Dev2.Common;

namespace Dev2.Enums {
    [Flags]
    public enum StateType {
        None = 0,
        Before = 1,
        After = 2,
        Append = 4,
        All = 8
    }

    [EnumDisplayString("Workflow", "Step", "Service")]
    public enum ActivityType {
        Workflow,
        Step,
        Service
    }
}

using System;
using Dev2.Common;

namespace Dev2.Enums {
    [Flags]
    public enum StateType {
        None = 0,
        Before = 1,
        After = 2,
        All = 3
    }

    [EnumDisplayString("Workflow", "Step")]
    public enum ActivityType {
        Workflow,
        Step
    }
}
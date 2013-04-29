using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

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

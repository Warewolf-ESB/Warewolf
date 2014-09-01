using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Common.Interfaces.Scheduler.Interfaces
{
    public interface IResourceHistory
    {
        string WorkflowOutput { get; }
        IList<IDebugState> DebugOutput { get; }
        IEventInfo TaskHistoryOutput { get; }
        string UserName { get; set; }
    }
}

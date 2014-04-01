using Dev2.Diagnostics;
using System.Collections.Generic;

namespace Dev2.Scheduler.Interfaces
{
    public interface IResourceHistory
    {
        string WorkflowOutput { get; }
        IList<DebugState> DebugOutput { get; }
        IEventInfo TaskHistoryOutput { get; }
        string UserName { get; set; }
    }
}

using System.Collections.Generic;
using Dev2.Diagnostics.Debug;

namespace Dev2.Scheduler.Interfaces
{
    public interface IResourceHistory
    {
        string WorkflowOutput { get; }
        IList<IDebugState> DebugOutput { get; }
        IEventInfo TaskHistoryOutput { get; }
        string UserName { get; set; }
    }
}

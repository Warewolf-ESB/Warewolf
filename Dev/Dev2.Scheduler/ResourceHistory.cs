using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Scheduler.Interfaces;

namespace Dev2.Scheduler
{
    public class ResourceHistory : IResourceHistory
    {
        public ResourceHistory(string workflowOutput, IList<IDebugState> debugOutput, IEventInfo taskHistoryOutput, string userName)
        {
            TaskHistoryOutput = taskHistoryOutput;
            DebugOutput = debugOutput;
            WorkflowOutput = workflowOutput;
            UserName = userName;
        }

        public string WorkflowOutput { get; private set; }
        public IList<IDebugState> DebugOutput { get; private set; }
        public IEventInfo TaskHistoryOutput { get; private set; }
        public string UserName { get; set; }
    }
}
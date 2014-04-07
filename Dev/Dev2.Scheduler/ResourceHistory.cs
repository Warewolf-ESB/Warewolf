using System.Collections.Generic;
using Dev2.Diagnostics;
using Dev2.Scheduler.Interfaces;

namespace Dev2.Scheduler
{
    public class ResourceHistory : IResourceHistory
    {
        public ResourceHistory(string workflowOutput, IList<DebugState> debugOutput, IEventInfo taskHistoryOutput, string userName)
        {
            TaskHistoryOutput = taskHistoryOutput;
            DebugOutput = debugOutput;
            WorkflowOutput = workflowOutput;
            UserName = userName;
        }

        public string WorkflowOutput { get; private set; }
        public IList<DebugState> DebugOutput { get; private set; }
        public IEventInfo TaskHistoryOutput { get; private set; }
        public string UserName { get; set; }
    }
}
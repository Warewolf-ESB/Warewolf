using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using System.Collections.Generic;

namespace Dev2.Core.Tests.Triggers
{
    public class ResourceHistoryForTest : IResourceHistory
    {
        public string WorkflowOutput { get; private set; }
        public IList<IDebugState> DebugOutput { get; private set; }
        public IEventInfo TaskHistoryOutput { get; private set; }
        public string UserName { get; set; }
    }
}

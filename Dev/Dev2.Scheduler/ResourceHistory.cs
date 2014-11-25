
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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

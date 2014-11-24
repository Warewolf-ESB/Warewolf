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
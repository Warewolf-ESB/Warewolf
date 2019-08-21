/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.Queue;
using Dev2.Triggers;

namespace Dev2.Studio.Interfaces.Trigger
{
    public interface ITriggerQueueView : ITriggerQueue
    {
        bool IsDirty { get; set; }
        string OldQueueName { get; set; }
        QueueStatus Status { get; set; }
        IErrorResultTO Errors { get; set; }
        bool IsNewQueue { get; set; }
        string NameForDisplay { get; set; }
    }
}

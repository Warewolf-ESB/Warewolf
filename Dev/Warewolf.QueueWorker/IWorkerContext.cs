/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Resources;
using Warewolf.Data;
using Warewolf.Triggers;

namespace QueueWorker
{
    internal interface IWorkerContext
    {
        string WorkflowUrl { get; }
        string Username { get; }
        string Password { get; }
        IQueueSource Source { get; }
        IQueueSource DeadLetterSink { get; }
        IQueueConfig QueueConfig { get; }
        string QueueName { get; }
        IServiceInputBase[] Inputs { get; }
    }
}

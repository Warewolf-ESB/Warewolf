/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;
using Warewolf.Options;

namespace Dev2.Triggers
{
    public interface ITriggerQueue : ITrigger, IEquatable<ITriggerQueue>
    {
        Guid QueueSourceId { get; set; }
        string QueueName { get; set; }
        string WorkflowName { get; set; }
        int Concurrency { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        IOption[] Options { get; set; }
        Guid QueueSinkId { get; set; }
        string DeadLetterQueue { get; set; }
        IOption[] DeadLetterOptions { get; set; }
        ICollection<IServiceInput> Inputs { get; set; }
        Guid ResourceId { get; set; }
    }
}
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
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Queue;
using Dev2.Studio.Interfaces.Trigger;
using Dev2.Triggers;
using System;
using System.Collections.Generic;
using Warewolf.Options;

namespace Warewolf.Trigger
{
    public class DummyTriggerQueueView : ITriggerQueueView
    {
        public DummyTriggerQueueView()
        {
            NameForDisplay = "'";
            IsNewQueue = true;
        }

        public bool IsDirty { get; set; }
        public string OldQueueName { get; set; }
        public QueueStatus Status { get; set; }
        public IErrorResultTO Errors { get; set; }
        public bool IsNewQueue { get; set; }
        public string NameForDisplay { get; set; }
        public Guid TriggerId { get; set; }
        public Guid QueueSourceId { get; set; }
        public string QueueName { get; set; }
        public string WorkflowName { get; set; }
        public int Concurrency { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public IOption[] Options { get; set; }
        public Guid QueueSinkId { get; set; }
        public string DeadLetterQueue { get; set; }
        public IOption[] DeadLetterOptions { get; set; }
        public ICollection<IServiceInput> Inputs { get; set; }
        public Guid ResourceId { get; set; }
        public string Name { get; set; }

        public bool Equals(ITriggerQueue other)
        {
            throw new NotImplementedException();
        }
    }
}

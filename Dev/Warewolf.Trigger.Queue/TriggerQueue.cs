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
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Triggers;
using Warewolf.Options;

namespace Warewolf.Trigger
{
    public class TriggerQueue : ITriggerQueue
    {
        public IResource QueueSource { get; set; }
        public string QueueName { get; set; }
        public string WorkflowName { get; set; }
        public int Concurrency { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public IOption[] Options { get; set; }
        public IResource QueueSink { get; set; }
        public string DeadLetterQueue { get; set; }
        public IOption[] DeadLetterOptions { get; set; }
        public ICollection<IServiceInput> Inputs { get; set; }
        public Guid ResourceId { get; set; }

        public bool Equals(ITriggerQueue other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var equals = true;
            equals &= QueueSource == other.QueueSource;
            equals &= string.Equals(QueueName, other.QueueName);
            equals &= string.Equals(WorkflowName, other.WorkflowName);
            equals &= Concurrency == other.Concurrency;
            equals &= string.Equals(UserName, other.UserName);
            equals &= string.Equals(Password, other.Password);
            equals &= Options == other.Options;
            equals &= QueueSink == other.QueueSink;
            equals &= string.Equals(DeadLetterQueue, other.DeadLetterQueue);
            equals &= DeadLetterOptions == other.DeadLetterOptions;
            equals &= Inputs == other.Inputs;

            return equals;
        }
    }
}

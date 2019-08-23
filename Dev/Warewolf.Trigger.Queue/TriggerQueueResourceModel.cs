/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Queue;
using Dev2.Triggers;
using System;
using System.Collections.Generic;

namespace Warewolf.Trigger
{
    public class TriggerQueueResourceModel : ITriggerQueueResourceModel
    {
        public IList<ITriggerQueue> QueueResources { get; set; }

        public IList<IExecutionHistory> CreateHistory(ITriggerQueue resource)
        {
            throw new NotImplementedException();
        }

        public void DeleteQueue(ITriggerQueue resource)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IList<ITriggerQueue> GetQueueResources()
        {
            throw new NotImplementedException();
        }

        public bool Save(ITriggerQueue resource, out string errorMessage)
        {
            throw new NotImplementedException();
        }

        public void Save(ITriggerQueue resource, string userName, string password)
        {
            throw new NotImplementedException();
        }
    }
}

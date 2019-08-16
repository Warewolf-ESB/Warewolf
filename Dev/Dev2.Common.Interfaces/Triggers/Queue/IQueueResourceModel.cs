/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Triggers;
using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Queue
{
    public interface IQueueResourceModel : IDisposable
    {
        IList<ITriggerQueue> QueueResources { get; set; }
        IList<ITriggerQueue> GetQueueResources();
        void DeleteQueue(ITriggerQueue resource);
        bool Save(ITriggerQueue resource, out string errorMessage);
        void Save(ITriggerQueue resource, string userName, string password);
        IList<IExecutionHistory> CreateHistory(ITriggerQueue resource);
    }
}
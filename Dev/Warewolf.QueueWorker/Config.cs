/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.DB;
using Dev2.Runtime.Triggers;
using Dev2.Triggers;
using System.Collections.Generic;
using Warewolf.Data;

namespace QueueWorker
{
    internal class Config : IConfig
    {
        ITriggerQueue _triggerQueue;

        public Config(IArgs processArgs)
        {
            var catalog = TriggersCatalog.Instance;
            _triggerQueue = catalog.LoadQueueTriggerFromFile(processArgs.Filename);
        }

        public string WorkflowUrl { get => _triggerQueue.WorkflowName; }
        public ICollection<IServiceInput> ValueKeys { get => _triggerQueue.Inputs; }
    }
}
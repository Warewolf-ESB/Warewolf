/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Triggers;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Triggers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dev2.Runtime.Triggers
{
    public class TriggersCatalog : ITriggersCatalog
    {
        readonly IDirectory _directoryWrapper;

        public TriggersCatalog()
        {
            _directoryWrapper = new DirectoryWrapper();
            Queues = new List<ITriggerQueue>();
        }

        public List<ITriggerQueue> Queues { get; set; }

        public void DeleteAllQueues()
        {
            
        }

        public void DeleteQueue(Guid resourceId, ITriggerQueue triggerQueue)
        {
            
        }

        public void Load()
        {
            Queues = new List<ITriggerQueue>();
            var resourceTestDirectories = _directoryWrapper.GetDirectories(EnvironmentVariables.TriggersPath);
            foreach (var resourceTestDirectory in resourceTestDirectories)
            {
                var resIdString = _directoryWrapper.GetDirectoryName(resourceTestDirectory);
                if (Guid.TryParse(resIdString, out Guid resId))
                {
                    //Queues
                }
            }
        }

        public void SaveQueue(Guid resourceId, ITriggerQueue triggerQueue)
        {
            var source = triggerQueue.QueueSource.ResourceName;
            var queue = triggerQueue.QueueName;
            var workflowName = triggerQueue.WorkflowName;
            var dirPath = EnvironmentVariables.TriggersPath;
            var filePath = $"{source}_{queue}_{workflowName}";

            var queueFilePath = Path.Combine(dirPath, $"{filePath}.bite");
            _directoryWrapper.CreateIfNotExists(queueFilePath);
        }
    }
}

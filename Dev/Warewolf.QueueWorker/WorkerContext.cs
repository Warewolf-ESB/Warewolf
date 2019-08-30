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
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Resources;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Triggers;
using Dev2.Triggers;
using System.Collections.Generic;
using Warewolf.Common;
using Warewolf.Data;
using Warewolf.Triggers;

namespace QueueWorker
{
    internal class WorkerContext : IWorkerContext
    {
        ITriggerQueue _triggerQueue;
        IResourceCatalogProxy _resourceCatalogProxy;

        public WorkerContext(IArgs processArgs, IResourceCatalogProxy resourceCatalogProxy)
        {
            var catalog = TriggersCatalog.Instance;
            _triggerQueue = catalog.LoadQueueTriggerFromFile(processArgs.Filename);
            _resourceCatalogProxy = resourceCatalogProxy;
        }

        public string WorkflowUrl { get => _triggerQueue.WorkflowName; }
        public ICollection<IServiceInput> ValueKeys { get => _triggerQueue.Inputs; }

        IQueueSource _queueSource;
        public IQueueSource Source
        {
            get
            {
                if (_queueSource is null)
                {
                    var qid = _triggerQueue.QueueSourceId;
                    _queueSource = _resourceCatalogProxy.GetResourceById<RabbitMQSource>(GlobalConstants.ServerWorkspaceID, qid);
                }

                return _queueSource;
            }
        }

        public IPublisher DeadLetterPublisher
        {
            get => null;
        }
        public IQueueConfig QueueConfig
        {
            get => null;
        }
    }
}
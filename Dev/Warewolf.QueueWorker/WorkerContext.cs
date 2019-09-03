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
using Newtonsoft.Json;
using System.Collections.Generic;
using Warewolf.Common;
using Warewolf.Data;
using Warewolf.Driver.RabbitMQ;
using Warewolf.Options;
using Warewolf.Triggers;

using RabbitMQSource = Dev2.Data.ServiceModel.RabbitMQSource;

namespace QueueWorker
{
    internal class WorkerContext : IWorkerContext
    {
        readonly ITriggerQueue _triggerQueue;
        readonly IResourceCatalogProxy _resourceCatalogProxy;

        public WorkerContext(IArgs processArgs, IResourceCatalogProxy resourceCatalogProxy)
        {
            var catalog = TriggersCatalog.Instance;
            var path = TriggersCatalog.PathFromResourceId(processArgs.TriggerId);
            _triggerQueue = catalog.LoadQueueTriggerFromFile(path);
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

        IQueueSource _deadLetterSink;
        public IQueueSource DeadLetterSink
        {
            get
            {
                if (_deadLetterSink is null)
                {
                    var qid = _triggerQueue.QueueSinkId;
                    _deadLetterSink = _resourceCatalogProxy.GetResourceById<RabbitMQSource>(GlobalConstants.ServerWorkspaceID, qid);
                }
                return _deadLetterSink;
            }
        }
        public IQueueConfig QueueConfig
        {
            get
            {
                var options = _triggerQueue.Options;
                var result = OptionTo<RabbitConfig>(options);
                result.QueueName = _triggerQueue.QueueName;
                return result;
            }
        }

        public string HostName { get => Source.ResourceName; }
        public string QueueName { get => _triggerQueue.QueueName; }
        public string Inputs { get => JsonConvert.SerializeObject(_triggerQueue.Inputs); }
        
        private static T OptionTo<T>(IOption[] options) where T : new()
        {
            var result = new T();

            if (options is null)
            {
                return result;
            }

            foreach (var p in result.GetType().GetProperties())
            {
                foreach (var option in options)
                {
                    if (option.Name != p.Name) continue;

                    if (option is IOptionInt intOption)
                    {
                        p.SetValue(result, intOption.Value);
                    }
                    else if (option is IOptionAutocomplete optionAutocomplete)
                    {
                        p.SetValue(result, optionAutocomplete.Value);
                    }
                    else if (option is IOptionBool optionBool)
                    {
                        p.SetValue(result, optionBool.Value);
                    }
                }
            }

            return result;
        }
    }
}
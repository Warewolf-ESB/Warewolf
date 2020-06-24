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
using Dev2.Common.Interfaces.Resources;
using Dev2.Common.Interfaces.Wrappers;
using System;
using System.IO;
using System.Linq;
using Warewolf.Common;
using Warewolf.Data;
using Warewolf.Driver.RabbitMQ;
using Warewolf.Options;
using Warewolf.OS.IO;
using Warewolf.Streams;
using Warewolf.Triggers;

using RabbitMQSource = Dev2.Data.ServiceModel.RabbitMQSource;

namespace QueueWorker
{
    internal interface IWorkerContextFactory
    {
        IWorkerContext New(IArgs processArgs, IResourceCatalogProxy resourceCatalogProxy, ITriggersCatalog triggerCatalog, IFilePath filePath);
    }

    internal class WorkerContextFactory : IWorkerContextFactory
    {
        public IWorkerContext New(IArgs processArgs, IResourceCatalogProxy resourceCatalogProxy, ITriggersCatalog triggerCatalog, IFilePath filePath)
        {
            return new WorkerContext(processArgs, resourceCatalogProxy, triggerCatalog, filePath);
        }
    }

    internal class WorkerContext : IWorkerContext
    {
        readonly Uri _serverUri;
        readonly ITriggerQueue _triggerQueue;
        readonly IResourceCatalogProxy _resourceCatalogProxy;
        readonly string _path;
        readonly IFilePath _filePath;


        public WorkerContext(IArgs processArgs, IResourceCatalogProxy resourceCatalogProxy, ITriggersCatalog triggersCatalog, IFilePath filePath)
        {
            var catalog = triggersCatalog;
            _path = triggersCatalog.PathFromResourceId(processArgs.TriggerId);
            _serverUri = processArgs.ServerEndpoint;
            _triggerQueue = catalog.LoadQueueTriggerFromFile(_path);
            _resourceCatalogProxy = resourceCatalogProxy;

            _filePath = filePath;
        }

        public void WatchTriggerResource(IFileSystemWatcher watcher)
        {
            var path = _filePath.GetDirectoryName(_path);
            var filename = _filePath.GetFileName(_path);

            watcher.EnableRaisingEvents = false;

            watcher.Path = path;
            watcher.Filter = filename;
            watcher.NotifyFilter = NotifyFilters.LastWrite
                    | NotifyFilters.FileName
                    | NotifyFilters.DirectoryName;

            watcher.EnableRaisingEvents = true;
        }

        public Guid TriggerId { get => _triggerQueue.Id; }

        public string WorkflowUrl { get => $"{_serverUri}/secure/{_triggerQueue.WorkflowName}.json"; }

        public string Username { get => _triggerQueue.UserName; }
        public string Password { get => _triggerQueue.Password; }

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
        public IStreamConfig QueueConfig
        {
            get
            {
                var options = _triggerQueue.Options;
                var result = OptionTo<RabbitConfig>(options);
                result.QueueName = _triggerQueue.QueueName;
                return result;
            }
        }

        public IStreamConfig DeadLetterConfig
        {
            get
            {
                var options = _triggerQueue.DeadLetterOptions;
                var result = OptionTo<RabbitConfig>(options);
                result.QueueName = _triggerQueue.DeadLetterQueue;
                return result;
            }
        }
        
        public string QueueName { get => _triggerQueue.QueueName; }
        public IServiceInputBase[] Inputs => _triggerQueue.Inputs.ToArray();
        public bool MapEntireMessage => _triggerQueue.MapEntireMessage;

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
                    else if (option is IOptionEnum optionEnum)
                    {
                        p.SetValue(result, optionEnum.Value);
                    }
                }
            }

            return result;
        }
    }
}
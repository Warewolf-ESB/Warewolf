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
using System.Linq;
using System.Text;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.State;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using RabbitMQ.Client;
using Warewolf.Driver.RabbitMQ;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using Warewolf.Streams;
using Warewolf.Triggers;
using RabbitMQSource = Dev2.Data.ServiceModel.RabbitMQSource;

namespace Dev2.Activities.RabbitMQ.Consume
{

    public class DsfConsumeRabbitMQActivity_upgrade : DsfBaseActivity , IEquatable<DsfConsumeRabbitMQActivity_upgrade>
    {
        private readonly bool _shouldSerializeConsumer;
        private readonly bool _shouldSerializeConnection;
        private readonly bool _shouldSerializeConnectionFactory;
        private readonly bool _shouldSerializeRabbitSource;
        private readonly bool _shouldSerializeChannel;

        internal List<string> _messages;
        private string _result;
        private int _timeOut;
        private ushort _prefetch;

        public DsfConsumeRabbitMQActivity_upgrade()
            :this(new ResponseManager())
        {
            DisplayName = "RabbitMQ Consume";
            _messages = new List<string>();
            Prefetch = "1";

            _shouldSerializeConnection = false;
            _shouldSerializeConnectionFactory = false;
            _shouldSerializeConsumer = false;
            _shouldSerializeRabbitSource = false;
            _shouldSerializeChannel = false;
        }

        public DsfConsumeRabbitMQActivity_upgrade(IResourceCatalog resourceCatalog)
            :this()
        {
            Prefetch = string.Empty;
            ResourceCatalog = resourceCatalog;
        }

        public DsfConsumeRabbitMQActivity_upgrade(IResponseManager responseManager)
        {
            ResponseManager = responseManager;
        }

        public Guid RabbitMQSourceResourceId { get; internal set; }
        public string QueueName { get; internal set; }
        public string Prefetch { get; internal set; }
        public bool IsObject { get; set; }
        public string ObjectName { get; internal set; }
        public IBasicConsumer Consumer { get; internal set; }
        public string TimeOut { get; internal set; }
        public bool ReQueue { get; internal set; }
        public bool Acknowledge { get; internal set; }
        public string Response { get; internal set; }
        public RabbitMQSource RabbitSource { get; set; }
        public IQueueConnection Connection { get; private set; }
        public IPublisher Publisher { get; private set; }
        public IModel Channel { get; private set; }

        public override List<string> GetOutputs() => new List<string> { Response, Result };

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (env == null)
            {
                return new List<DebugItem>();
            }
            var debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Requeue"), debugItem);
            var value = ReQueue ? "True" : "False";
            AddDebugItem(new DebugEvalResult(value, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            return _debugInputs;
        }

        protected override void AssignResult(IDSFDataObject dataObject, int update)
        {
            if (IsObject)
            {
                ResponseManager.IsObject = IsObject;
                ResponseManager.ObjectName = ObjectName;
            }
            base.AssignResult(dataObject, update);
            if (!string.IsNullOrEmpty(ObjectName) && IsObject && (_messages?.Any() ?? false))
            {
                foreach (var message in _messages)
                {
                    ResponseManager.PushResponseIntoEnvironment(message, update, dataObject);
                }
            }

            if (!string.IsNullOrEmpty(Response) && !IsObject)
            {
                UpdateEnvironment(dataObject, update);
            }
        }

        private void UpdateEnvironment(IDSFDataObject dataObject, int update)
        {
            if (DataListUtil.IsValueScalar(Response))
            {
                UpdateEnvironmentScalars(dataObject, update);
            }
            else
            {
                UpdateEnvironmentRecordsets(dataObject, update);
            }
        }

        private void UpdateEnvironmentScalars(IDSFDataObject dataObject, int update)
        {
            if (_messages != null && _messages.Count > 0)
            {
                dataObject.Environment.Assign(Response, _messages.Last(), update);
            }
        }

        private void UpdateEnvironmentRecordsets(IDSFDataObject dataObject, int update)
        {
            if (_messages != null)
            {
                foreach (var message in _messages)
                {
                    dataObject.Environment.Assign(Response, message, update);
                }
            }
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            base.GetDebugOutputs(env, update);

            if (env != null && !string.IsNullOrEmpty(Response) && !IsObject)
            {
                var debugItem = new DebugItem();
                AddDebugItem(new DebugEvalResult(Response, "", env, update), debugItem);
                _debugOutputs.Add(debugItem);
            }

            if (env != null && !string.IsNullOrEmpty(ObjectName) && IsObject)
            {
                var debugItem = new DebugItem();
                AddDebugItem(new DebugEvalResult(ObjectName, "", env, update), debugItem);
                _debugOutputs.Add(debugItem);
            }

            return _debugOutputs?.Any() ?? false ? _debugOutputs : new List<DebugItem>();
        }

        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            _messages = new List<string>();
            try
            {
                RabbitSource = ResourceCatalog.GetResource<RabbitMQSource>(GlobalConstants.ServerWorkspaceID, RabbitMQSourceResourceId);
                if (RabbitSource == null || RabbitSource.ResourceType != enSourceType.RabbitMQSource.ToString())
                {
                    _messages.Add(ErrorResource.RabbitSourceHasBeenDeleted);
                    return _messages;
                }

                if (!evaluatedValues.TryGetValue(nameof(QueueName), out var queueName))
                {
                    _messages.Add(ErrorResource.RabbitQueueNameRequired);
                    return _messages;
                }
                if (!evaluatedValues.TryGetValue(nameof(Prefetch), out var prefetch))
                {
                    prefetch = string.Empty;
                }

                var source = new RabbitMQSource
                {
                    HostName = RabbitSource.HostName,
                    Port = RabbitSource.Port,
                    UserName = RabbitSource.UserName,
                    Password = RabbitSource.Password,
                    VirtualHost = RabbitSource.VirtualHost
                };

                using (Connection = source.NewConnection())
                {
                    PerformExecutionOnChannel(queueName, prefetch);
                }
                return new List<string> { _result };
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("ConsumeRabbitMQActivity", ex, GlobalConstants.WarewolfError);
                throw new Exception(ex.GetAllMessages());
            }
        }

        private void PerformExecutionOnChannel(string queueName, string prefetch)
        {
            if (!string.IsNullOrEmpty(TimeOut))
            {
                _timeOut = int.Parse(TimeOut);
            }

            _prefetch = string.IsNullOrEmpty(prefetch) ? (ushort)0 : ushort.Parse(prefetch);
            if (_prefetch == 0)
            {
                _prefetch = ushort.MaxValue;
            }

            var config = new RabbitConfig
            {
                QueueName = queueName,
                PrefetchSize = 0,
                PrefetchCount = _prefetch,
                Acknwoledge = false
            };

            Connection.BasicQos(config);

            var msgCount = 0;
            if (ReQueue)
            {
                BasicGetResult response;
                try
                {
                    response = Connection.BasicGet(queueName, acknowledge: false);
                }
                catch (Exception)
                {
                    throw new Exception(string.Format(ErrorResource.RabbitQueueNotFound, queueName));
                }

                while (response != null && _prefetch > msgCount)
                {
                    _messages.Add(Encoding.Default.GetString(response.Body));
                    msgCount++;
                    try
                    {
                        response = Connection.BasicGet(queueName, false);
                    }
                    catch (Exception)
                    {
                        throw new Exception(string.Format(ErrorResource.RabbitQueueNotFound, queueName));
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(TimeOut))
                {
                    ExecuteWithTimeout(config);
                }
                else
                {
                    var consumer = new QueueConsumer();
                    try
                    {
                        Connection.StartConsuming(config, consumer);

                        for (int j = 0; j < consumer.Messages.Count; j++)
                        {
                            _messages.Add(consumer.Messages[j]);
                        }

                        if (consumer.Messages.Count == 0)
                        {
                            _result = "Empty";
                        }
                    }
                    catch (Exception)
                    {
                        throw new Exception(string.Format(ErrorResource.RabbitQueueNotFound, queueName));
                    }
                }
            }
        }

        private void ExecuteWithTimeout(RabbitConfig config)
        {
            var consumer = new QueueConsumer();

            try
            {
                Connection.StartConsumingWithTimeOut(new EventingBasicConsumerFactory(), new ManualResetEventFactory(), config, consumer, _timeOut);
            }
            catch (Exception)
            {
                throw new Exception(string.Format(ErrorResource.RabbitQueueNotFound, config.QueueName));
            }

            if (consumer.Messages.Count == 0)
            {
                _result = $"Empty, timeout: {_timeOut} second(s)";
            }
            else
            {
                for (int i = 0; i < consumer.Messages.Count; i++)
                {
                    _messages.Add(consumer.Messages[i]);
                }
            }
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[] {
                new StateVariable
                {
                    Name = nameof(QueueName),
                    Value = QueueName,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(Acknowledge),
                    Value = Acknowledge.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(IsObject),
                    Value = IsObject.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(ObjectName),
                    Value = ObjectName,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(Prefetch),
                    Value = Prefetch,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(RabbitMQSourceResourceId),
                    Value = RabbitMQSourceResourceId.ToString(),
                    Type = StateVariable.StateType.Input
                },new StateVariable
                {
                    Name = nameof(ReQueue),
                    Value = ReQueue.ToString(),
                    Type = StateVariable.StateType.Input
                },new StateVariable
                {
                    Name = nameof(TimeOut),
                    Value = TimeOut,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name=nameof(Result),
                    Value = Result,
                    Type = StateVariable.StateType.Output
                }
                ,
                new StateVariable
                {
                    Name=nameof(Response),
                    Value = Response,
                    Type = StateVariable.StateType.Output
                }
            };
        }

        public bool ShouldSerializeConsumer() => _shouldSerializeConsumer;
        public bool ShouldSerializeConnection() => _shouldSerializeConnection;
        public bool ShouldSerializeConnectionFactory() => _shouldSerializeConnectionFactory;
        public bool ShouldSerializeRabbitSource() => _shouldSerializeRabbitSource;
        public bool ShouldSerializeChannel() => _shouldSerializeChannel;

        public bool Equals(DsfConsumeRabbitMQActivity other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var isSourceEqual = CommonEqualityOps.AreObjectsEqual<IResource>(RabbitSource, other.RabbitSource);
            return base.Equals(other)
                && string.Equals(Result, other.Result)
                && Prefetch == other.Prefetch
                && TimeOut == other.TimeOut
                && IsObject == other.IsObject
                && string.Equals(ObjectName, other.ObjectName)
                && RabbitMQSourceResourceId.Equals(other.RabbitMQSourceResourceId)
                && string.Equals(QueueName, other.QueueName)
                && string.Equals(DisplayName, other.DisplayName)
                && string.Equals(Response, other.Response)
                && string.Equals(Prefetch, other.Prefetch)
                && Acknowledge == other.Acknowledge
                && string.Equals(TimeOut, other.TimeOut)
                && ReQueue == other.ReQueue
                && isSourceEqual;
        }


        public bool Equals(DsfConsumeRabbitMQActivity_upgrade other)
        {
            if (other is null || other.GetType() != GetType())
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_messages != null ? _messages.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _timeOut;
                hashCode = (hashCode * 397) ^ IsObject.GetHashCode();
                hashCode = (hashCode * 397) ^ (ObjectName != null ? ObjectName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ RabbitMQSourceResourceId.GetHashCode();
                hashCode = (hashCode * 397) ^ (QueueName != null ? QueueName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Response != null ? Response.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Prefetch != null ? Prefetch.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Acknowledge.GetHashCode();
                hashCode = (hashCode * 397) ^ (TimeOut != null ? TimeOut.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ReQueue.GetHashCode();
                hashCode = (hashCode * 397) ^ (Consumer != null ? Consumer.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Connection != null ? Connection.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Channel != null ? Channel.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RabbitSource != null ? RabbitSource.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}

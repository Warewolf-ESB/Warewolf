#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.ServiceModel;
using Dev2.Diagnostics;
using Dev2.Util;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using Dev2.Common.State;

namespace Dev2.Activities.RabbitMQ.Consume
{
    [ToolDescriptorInfo("RabbitMq", "RabbitMQ Consume", ToolType.Native, "406ea660-64cf-4c82-b6f0-42d48172a799",
        "Dev2.Activities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml",
        "Tool_Utility_Rabbit_MQ_Consume")]
    public class DsfConsumeRabbitMQActivity : DsfBaseActivity, IEquatable<DsfConsumeRabbitMQActivity>
    {
        internal List<string> _messages;
        string _result = "Success";
        ushort _prefetch;
        int _timeOut;
        public bool IsObject { get; set; }
        [FindMissing] public string ObjectName { get; set; }

        public DsfConsumeRabbitMQActivity()
            : this(new ResponseManager())
        {
            DisplayName = "RabbitMQ Consume";
            _messages = new List<string>();
            Prefetch = "1";
        }

        public DsfConsumeRabbitMQActivity(IResourceCatalog resourceCatalog)
        {
            DisplayName = "RabbitMQ Consume";
            _messages = new List<string>();
            ResourceCatalog = resourceCatalog;
        }

        public DsfConsumeRabbitMQActivity(IResponseManager responseManager)
        {
            ResponseManager = responseManager;
        }


        public Guid RabbitMQSourceResourceId { get; set; }

        [Inputs("Queue Name")] [FindMissing] public string QueueName { get; set; }

        [FindMissing] public string Response { get; set; }

        [FindMissing] [Inputs("Prefetch")] public string Prefetch { get; set; }

        public bool Acknowledge { get; set; }

        [FindMissing] public string TimeOut { get; set; }

        public bool ReQueue { get; set; }

        public QueueingBasicConsumer Consumer { get; set; }

        public bool ShouldSerializeConsumer() => false;

        [NonSerialized] ConnectionFactory _connectionFactory;

        internal ConnectionFactory ConnectionFactory
        {
            get { return _connectionFactory ?? (_connectionFactory = new ConnectionFactory()); }
            set { _connectionFactory = value; }
        }

        public bool ShouldSerializeConnectionFactory() => false;

        internal IConnection Connection { get; set; }

        public bool ShouldSerializeConnection() => false;

        internal IModel Channel { get; set; }
        public bool ShouldSerializeChannel() => false;

        public RabbitMQSource RabbitSource { get; set; }
        public bool ShouldSerializeRabbitSource() => false;

        public override IEnumerable<StateVariable> GetState()
        {
            return new[]
            {
                new StateVariable
                {
                    Name = "QueueName",
                    Value = QueueName,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "Acknowledge",
                    Value = Acknowledge.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "IsObject",
                    Value = IsObject.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "ObjectName",
                    Value = ObjectName,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "Prefetch",
                    Value = Prefetch,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "RabbitMQSourceResourceId",
                    Value = RabbitMQSourceResourceId.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "ReQueue",
                    Value = ReQueue.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "TimeOut",
                    Value = TimeOut,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "Result",
                    Value = Result,
                    Type = StateVariable.StateType.Output
                },
                new StateVariable
                {
                    Name = "Response",
                    Value = Response,
                    Type = StateVariable.StateType.Output
                }
            };
        }


        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            _messages = new List<string>();
            try
            {
                RabbitSource =
                    ResourceCatalog.GetResource<RabbitMQSource>(GlobalConstants.ServerWorkspaceID,
                        RabbitMQSourceResourceId);
                if (RabbitSource == null || RabbitSource.ResourceType != enSourceType.RabbitMQSource.ToString())
                {
                    _messages.Add(ErrorResource.RabbitSourceHasBeenDeleted);
                    return _messages;
                }

                if (!evaluatedValues.TryGetValue("QueueName", out var queueName))
                {
                    _messages.Add(ErrorResource.RabbitQueueNameRequired);
                    return _messages;
                }

                if (!evaluatedValues.TryGetValue("Prefetch", out var prefetch))
                {
                    prefetch = string.Empty;
                }

                ConnectionFactory.HostName = RabbitSource.HostName;
                ConnectionFactory.Port = RabbitSource.Port;
                ConnectionFactory.UserName = RabbitSource.UserName;
                ConnectionFactory.Password = RabbitSource.Password;
                ConnectionFactory.VirtualHost = RabbitSource.VirtualHost;

                using (Connection = ConnectionFactory.CreateConnection())
                {
                    using (Channel = Connection.CreateModel())
                    {
                        PerformExecutionOnChannel(queueName, prefetch);
                    }
                }

                return new List<string> {_result};
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("ConsumeRabbitMQActivity", ex, GlobalConstants.WarewolfError);
                throw new Exception(ex.GetAllMessages());
            }
        }

        private string CorrelationID { get; set; } = "";
#pragma warning disable S1541 // Methods and properties should not be too complex
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        private void PerformExecutionOnChannel(string queueName, string prefetch)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (!string.IsNullOrEmpty(TimeOut))
            {
                _timeOut = int.Parse(TimeOut);
            }

            _prefetch = string.IsNullOrEmpty(prefetch) ? (ushort) 0 : ushort.Parse(prefetch);
            if (_prefetch == 0)
            {
                _prefetch = ushort.MaxValue;
            }

            Channel.BasicQos(0, _prefetch, Acknowledge);
            var msgCount = 0;
            if (ReQueue)
            {
                BasicGetResult response;
                try
                {
                    response = Channel.BasicGet(queueName, false);
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
                        response = Channel.BasicGet(queueName, false);
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
                    ExecuteWithTimeout(queueName, msgCount);
                }
                else
                {
                    uint messageCount;
                    try
                    {
                        messageCount = Channel.MessageCount(queueName);
                    }
                    catch (Exception)
                    {
                        messageCount = 0;
                    }

                    Consumer = new QueueingBasicConsumer(Channel);
                    try
                    {
                        Channel.BasicConsume(queueName, false, Consumer);
                    }
                    catch (Exception)
                    {
                        throw new Exception(string.Format(ErrorResource.RabbitQueueNotFound, queueName));
                    }

                    ulong? tag = null;
                    for (int i = 0; i < messageCount && _prefetch > msgCount; i++)
                    {
                        var ea = Consumer.Queue.Dequeue();
                        var body = ea.Body;
                        _messages.Add(Encoding.Default.GetString(body));
                        CorrelationID = ea.BasicProperties.CorrelationId;
                        tag = ea.DeliveryTag;
                        msgCount++;
                    }

                    if (tag.HasValue)
                    {
                        Channel.BasicAck(tag.Value, _prefetch != 1);
                    }
                    else
                    {
                        _result = "Empty";
                    }
                }
            }
            Dev2Logger.Debug($"Message consumed from queue {queueName} CorrelationId: {CorrelationID} ",
                GlobalConstants.WarewolfDebug);
        }

        private int ExecuteWithTimeout(string queueName, int msgCount)
        {
            Consumer = new QueueingBasicConsumer(Channel);
            try
            {
                Channel.BasicConsume(queueName, false, Consumer);
            }
            catch (Exception)
            {
                throw new Exception(string.Format(ErrorResource.RabbitQueueNotFound, queueName));
            }
            ulong? tag = null;
            while (Consumer.Queue.Dequeue((int) TimeSpan.FromSeconds(_timeOut).TotalMilliseconds,
                       out var basicDeliverEventArgs) && _prefetch > msgCount)
            {
                if (basicDeliverEventArgs == null)
                {
                    _result = $"Empty, timeout: {_timeOut} second(s)";
                }
                else
                {
                    var body = basicDeliverEventArgs.Body;
                    _messages.Add(Encoding.Default.GetString(body));
                    CorrelationID = basicDeliverEventArgs.BasicProperties.CorrelationId;
                    tag = basicDeliverEventArgs.DeliveryTag;
                }
                msgCount++;
            }
            if (tag.HasValue)
            {
                Channel.BasicAck(tag.Value, _prefetch != 1);
            }
            Dev2Logger.Debug($"Message consumed from queue {queueName} CorrelationId: {CorrelationID} ",
                GlobalConstants.WarewolfDebug);
            return msgCount;
        }


        #region Overrides of DsfBaseActivity

        public override List<string> GetOutputs() => new List<string> {Response, Result};

        #endregion

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


        protected override void AssignResult(IDSFDataObject dataObject, int update)
        {
            if (IsObject)
            {
                ResponseManager.IsObject = IsObject;
                ResponseManager.ObjectName = ObjectName;
            }
            dataObject.CustomTransactionID = CorrelationID;
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

#pragma warning disable S1541 // Methods and properties should not be too complex
        public bool Equals(DsfConsumeRabbitMQActivity other)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (ReferenceEquals(null, other))
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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DsfConsumeRabbitMQActivity) obj);
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        public override int GetHashCode()
#pragma warning restore S1541 // Methods and properties should not be too complex
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
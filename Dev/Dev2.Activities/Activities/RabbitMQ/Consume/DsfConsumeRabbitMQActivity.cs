/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using RabbitMQ.Client.Events;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;











namespace Dev2.Activities.RabbitMQ.Consume
{
    [ToolDescriptorInfo("RabbitMq", "RabbitMQ Consume", ToolType.Native, "406ea660-64cf-4c82-b6f0-42d48172a799", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_Rabbit_MQ_Consume")]
    public class DsfConsumeRabbitMQActivity : DsfBaseActivity, IEquatable<DsfConsumeRabbitMQActivity>
    {
        public List<string> _messages;
        public string _result = "Success";
        public ushort _prefetch;
        public int _timeOut;
        public bool IsObject { get; set; }
        [FindMissing]
        public string ObjectName { get; set; }
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

        [Inputs("Queue Name")]
        [FindMissing]
        public string QueueName { get; set; }

        [FindMissing]
        public string Response { get; set; }

        [FindMissing]
        [Inputs("Prefetch")]
        public string Prefetch { get; set; }

        public bool Acknowledge { get; set; }

        [FindMissing]
        public string TimeOut { get; set; }

        public bool ReQueue { get; set; }

        public QueueingBasicConsumer Consumer { get; set; }

        public bool ShouldSerializeConsumer()
        {
            return false;
        }

        [NonSerialized]
        private ConnectionFactory _connectionFactory;

        internal ConnectionFactory ConnectionFactory
        {
            get
            {
                return _connectionFactory ?? (_connectionFactory = new ConnectionFactory());
            }
            set
            {
                _connectionFactory = value;
            }
        }

        public bool ShouldSerializeConnectionFactory()
        {
            return false;
        }

        internal IConnection Connection { get; set; }

        public bool ShouldSerializeConnection()
        {
            return false;
        }


        internal IModel Channel { get; set; }
        public bool ShouldSerializeChannel()
        {
            return false;
        }

        internal RabbitMQSource RabbitSource { get; set; }
        public bool ShouldSerializeRabbitSource()
        {
            return false;
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

                string queueName;
                if (!evaluatedValues.TryGetValue("QueueName", out queueName))
                {
                    _messages.Add(ErrorResource.RabbitQueueNameRequired);
                    return _messages;
                }
                string prefetch;
                if (!evaluatedValues.TryGetValue("Prefetch", out prefetch))
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
                        if (!string.IsNullOrEmpty(TimeOut))
                            _timeOut = int.Parse(TimeOut);
                        _prefetch = string.IsNullOrEmpty(prefetch) ? (ushort)0 : ushort.Parse(prefetch);
                        if (_prefetch == 0)
                        {
                            _prefetch = ushort.MaxValue;
                        }
                        Channel.BasicQos(0, _prefetch, Acknowledge);
                        int msgCount = 0;
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
                                Consumer = new QueueingBasicConsumer(Channel);
                                try
                                {
                                    Channel.BasicConsume(queueName, false, Consumer);
                                }
                                catch (Exception)
                                {
                                    throw new Exception(string.Format(ErrorResource.RabbitQueueNotFound, queueName));
                                }
                                BasicDeliverEventArgs basicDeliverEventArgs;
                                ulong? tag = null;
                                while (Consumer.Queue.Dequeue((int)TimeSpan.FromSeconds(_timeOut).TotalMilliseconds, out basicDeliverEventArgs) && _prefetch > msgCount)
                                {
                                    if (basicDeliverEventArgs == null)
                                    {
                                        _result = string.Format("Empty, timeout: {0} second(s)", _timeOut);
                                    }
                                    else
                                    {
                                        var body = basicDeliverEventArgs.Body;
                                        _messages.Add(Encoding.Default.GetString(body));
                                        tag = basicDeliverEventArgs.DeliveryTag;
                                    }
                                    msgCount++;
                                }
                                if (tag.HasValue)
                                {
                                    Channel.BasicAck(tag.Value, _prefetch != 1);
                                }
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
                    }
                }
                return new List<string> { _result };
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("ConsumeRabbitMQActivity", ex, GlobalConstants.WarewolfError);
                throw new Exception(ex.GetAllMessages());
            }
        }



        #region Overrides of DsfBaseActivity

        public override List<string> GetOutputs()
        {
            return new List<string> { Response, Result };
        }

        #endregion

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (env == null)
            {
                return new List<DebugItem>();
            }
            DebugItem debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Requeue"), debugItem);
            string value = ReQueue ? "True" : "False";
            AddDebugItem(new DebugEvalResult(value, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            return _debugInputs;
        }


        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {
            base.GetDebugOutputs(dataList, update);

            if (dataList != null && !string.IsNullOrEmpty(Response))
            {
                if (!IsObject)
                {
                    DebugItem debugItem = new DebugItem();
                    AddDebugItem(new DebugEvalResult(Response, "", dataList, update), debugItem);
                    _debugOutputs.Add(debugItem);
                }
            }
            if (dataList != null && !string.IsNullOrEmpty(ObjectName))
            {
                if (IsObject)
                {
                    DebugItem debugItem = new DebugItem();
                    AddDebugItem(new DebugEvalResult(ObjectName, "", dataList, update), debugItem);
                    _debugOutputs.Add(debugItem);
                }
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
            base.AssignResult(dataObject, update);
            if (!string.IsNullOrEmpty(ObjectName))
            {
                if (IsObject)
                {
                    if (_messages?.Any() ?? false)
                    {
                        foreach (var message in _messages)
                        {
                            ResponseManager.PushResponseIntoEnvironment(message, update, dataObject);
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(Response) && !IsObject)
            {
                if (DataListUtil.IsValueScalar(Response))
                {
                    if (_messages != null)
                        dataObject.Environment.Assign(Response, _messages.Last(), update);
                }
                else
                {
                    if (_messages != null)
                        foreach (var message in _messages)
                        {
                            dataObject.Environment.Assign(Response, message, update);
                        }
                }
            }
        }

        public bool Equals(DsfConsumeRabbitMQActivity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            bool sourceIsSame;
            var b = RabbitSource == null && other.RabbitSource != null;
            var b1 = other.RabbitSource == null && RabbitSource != null;
            if (b || b1)
            {
                sourceIsSame = false;
            }
            else if(RabbitSource == null && other.RabbitSource == null)
            {
                sourceIsSame = true;
            }
            else
            {
                sourceIsSame = RabbitSource?.Equals(other.RabbitSource) ?? false;
            }
            //var sourceIsSame = (RabbitSource?.Equals(other.RabbitSource) ?? false);
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
                && sourceIsSame;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DsfConsumeRabbitMQActivity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_messages != null ? _messages.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Prefetch.GetHashCode();
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
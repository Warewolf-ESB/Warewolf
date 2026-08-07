#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.ServiceModel;
using Dev2.Util;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Hosting;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Dev2.Common.State;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Warewolf.Data.Options;
using Dev2.Common.X6;
using Dev2.WorkflowConverters;

namespace Dev2.Activities.RabbitMQ.Publish
{
    [ToolDescriptorInfo("RabbitMq", "RabbitMQ Publish", ToolType.Native, "FFEC6885-597E-49A2-A1AD-AE81E33DF809",
        "Dev2.Activities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml",
        "Tool_Utility_Rabbit_MQ_Publish")]
    public class PublishRabbitMQActivity : DsfBaseActivity, IEquatable<PublishRabbitMQActivity>
    {
        public PublishRabbitMQActivity()
            : this(Dev2.Runtime.Hosting.ResourceCatalog.Instance, new ConnectionFactory())
        {
        }

        public PublishRabbitMQActivity(IResourceCatalog resourceCatalog, ConnectionFactory connectionFactory)
        {
            ResourceCatalog = resourceCatalog;
            ConnectionFactory = connectionFactory;
            DisplayName = "RabbitMQ Publish";
            if (BasicProperties is null)
            {
                BasicProperties = new RabbitMqPublishOptions();
            }
        }

        public Guid RabbitMQSourceResourceId { get; set; }
        public RabbitMqPublishOptions BasicProperties { get; set; }

        [Inputs("Queue Name")] [FindMissing] public string QueueName { get; set; }

        [FindMissing] public bool IsDurable { get; set; }

        [FindMissing] public bool IsExclusive { get; set; }

        [FindMissing] public bool IsAutoDelete { get; set; }

        [Inputs("Message")] [FindMissing] public string Message { get; set; }

        [NonSerialized] ConnectionFactory _connectionFactory;

        private IDSFDataObject _dataObject { get; set; }
        private int _update { get; set; }


        internal ConnectionFactory ConnectionFactory
        {
            get => _connectionFactory ?? (_connectionFactory = new ConnectionFactory());
            set => _connectionFactory = value;
        }

        internal IConnection Connection { get; set; }

        internal IModel Channel { get; set; }

        public RabbitMQSource RabbitMQSource { get; set; }


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
                    Type = StateVariable.StateType.Input,
                    Name = nameof(BasicProperties),
                    Value = BasicProperties?.ToString(),
                },
                new StateVariable
                {
                    Name = "IsDurable",
                    Value = IsDurable.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "IsExclusive",
                    Value = IsExclusive.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "Message",
                    Value = Message,
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
                    Name = "IsAutoDelete",
                    Value = IsAutoDelete.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "Result",
                    Value = Result,
                    Type = StateVariable.StateType.Output
                }
            };
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, _update);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _dataObject = dataObject;
            _update = update;
            base.ExecuteTool(dataObject, update);
        }

        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            try
            {
                var CorrelationID = GetCorrelationID();
                RabbitMQSource =
                    ResourceCatalog.GetResource<RabbitMQSource>(GlobalConstants.ServerWorkspaceID,
                        RabbitMQSourceResourceId);

                if (RabbitMQSource == null
                    && AmbientSourceLoader.Current?.EnsureSourceLoaded(RabbitMQSourceResourceId) == true
                    && ResourceCatalog.WorkspaceResources
                           .TryGetValue(GlobalConstants.ServerWorkspaceID, out var ws))
                {
                    lock (ws)
                        RabbitMQSource = ws.OfType<RabbitMQSource>().FirstOrDefault(r => r.ResourceID == RabbitMQSourceResourceId);
                }

                if (RabbitMQSource == null)
                {
                    return new List<string> { ErrorResource.RabbitSourceHasBeenDeleted };
                }

                if (!evaluatedValues.TryGetValue("QueueName", out string queueName) ||
                    !evaluatedValues.TryGetValue("Message", out string message))
                {
                    return new List<string> { ErrorResource.RabbitQueueNameAndMessageRequired };
                }

                ConnectionFactory.HostName = RabbitMQSource.HostName;
                ConnectionFactory.Port = RabbitMQSource.Port;
                ConnectionFactory.UserName = RabbitMQSource.UserName;
                ConnectionFactory.Password = RabbitMQSource.Password;
                ConnectionFactory.VirtualHost = RabbitMQSource.VirtualHost;

                using (Connection = ConnectionFactory.CreateConnection())
                {
                    Channel = Connection.CreateModel();
                    try
                    {
                        bool newExchangeOrQueue = false;
						try
						{
							// Check if the exchange exists
							Channel.ExchangeDeclarePassive(queueName);
						}
						catch (OperationInterruptedException)
						{
							// A failed passive declare closes the channel server-side (the
							// broker's 404 NOT_FOUND is a channel-level AMQP exception), so
							// the active declare must run on a brand-new channel - retrying
							// on the one the broker just closed throws AlreadyClosedException
							// instead of creating the exchange (see also
							// RabbitMqDeadLetterPublisher.EnsureChannelAsync, which documents
							// and avoids this same pitfall).
							Channel.Dispose();
							Channel = Connection.CreateModel();
							// The exchange does not exist, so declare it
							Channel.ExchangeDeclare(queueName, ExchangeType.Direct, IsDurable, IsAutoDelete, null);
                            newExchangeOrQueue = true;
						}
						try
						{
							// Check if the queue exists
							Channel.QueueDeclarePassive(queueName);
						}
						catch (OperationInterruptedException)
						{
							// Same reasoning as above: the queue's passive-declare 404 also
							// closes this channel, so its active declare needs a fresh one too.
							Channel.Dispose();
							Channel = Connection.CreateModel();
							// The queue does not exist, so declare it
							Channel.QueueDeclare(queueName, IsDurable, IsExclusive, IsAutoDelete, null);
							newExchangeOrQueue = true;
						}
                        if (newExchangeOrQueue)
                        {
                            Channel.QueueBind(queueName, queueName, "", new Dictionary<string, object>());
                        }

                        var basicProperties = Channel.CreateBasicProperties();
                        basicProperties.Persistent = true;
                        basicProperties.CorrelationId = CorrelationID;
                        Channel.BasicPublish(queueName, "", basicProperties, Encoding.UTF8.GetBytes(message));
                    }
                    finally
                    {
                        Channel?.Dispose();
                    }
                }

                Dev2Logger.Debug($"Message published to queue {queueName} CorrelationId: {CorrelationID} ",
                    GlobalConstants.WarewolfDebug);
                return new List<string> { "Success" };
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("PublishRabbitMQActivity", ex, GlobalConstants.WarewolfError);
                throw new Exception(ex.GetAllMessages());
            }
        }

        private string GetCorrelationID()
        {
            if (BasicProperties.AutoCorrelation is Manual properties)
            {
                var expr = _dataObject.Environment.EvalToExpression(properties.CorrelationID, _update);
                return Warewolf.Storage.ExecutionEnvironment.WarewolfEvalResultToString(_dataObject.Environment.Eval(expr, _update, false, true));
            }
            else
            {
                if (BasicProperties.AutoCorrelation is CustomTransactionID || !string.IsNullOrEmpty(_dataObject.CustomTransactionID))
                {
                    return _dataObject.CustomTransactionID;
                }

                if (BasicProperties.AutoCorrelation is ExecutionID)
                {
                    return _dataObject.ExecutionID.ToString();
                }

                return _dataObject.ExecutionID.ToString();
            }
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        public bool Equals(PublishRabbitMQActivity other)
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

            var isSourceEqual = CommonEqualityOps.AreObjectsEqual<IResource>(RabbitMQSource, other.RabbitMQSource);
            return base.Equals(other)
                   && RabbitMQSourceResourceId.Equals(other.RabbitMQSourceResourceId)
                   && string.Equals(QueueName, other.QueueName)
                   && IsDurable == other.IsDurable
                   && IsExclusive == other.IsExclusive
                   && IsAutoDelete == other.IsAutoDelete
                   && string.Equals(Message, other.Message)
                   && string.Equals(DisplayName, other.DisplayName)
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

            return Equals((PublishRabbitMQActivity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ RabbitMQSourceResourceId.GetHashCode();
                hashCode = (hashCode * 397) ^ (QueueName != null ? QueueName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsDurable.GetHashCode();
                hashCode = (hashCode * 397) ^ IsExclusive.GetHashCode();
                hashCode = (hashCode * 397) ^ IsAutoDelete.GetHashCode();
                hashCode = (hashCode * 397) ^ (Message != null ? Message.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Connection != null ? Connection.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Channel != null ? Channel.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RabbitMQSource != null ? RabbitMQSource.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override void ToX6Json(Cell cell)
        {
            if (cell.data == null) cell.data = new System.Collections.Generic.Dictionary<string, object>();

            base.ToX6Json(cell);

            cell.shape = Constants.RABBITMQPUBLISHACTIVITY;
            cell.data[Constants.TYPE] = Constants.RABBITMQPUBLISHACTIVITY.ToLower();
            cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_RABBITMQPUBLISH;
            cell.data[Constants.UNIQUEID] = UniqueID;

            cell.data.TryAdd(Constants.RABBITMQPUBLISH_SOURCEID, RabbitMQSourceResourceId);
            cell.data.TryAdd(Constants.RABBITMQPUBLISH_QUEUENAME, QueueName);
            cell.data.TryAdd(Constants.RABBITMQPUBLISH_SETTINGS_DURABLE, IsDurable);
            cell.data.TryAdd(Constants.RABBITMQPUBLISH_SETTINGS_EXCLUSIVE, IsExclusive);
            cell.data.TryAdd(Constants.RABBITMQPUBLISH_SETTINGS_AUTODELETE, IsAutoDelete);
            cell.data.TryAdd(Constants.RABBITMQPUBLISH_MESSAGE, Message);
            cell.data.TryAdd(Constants.RABBITMQPUBLISH_BASICPROPERTIES, BasicProperties);
            cell.data.TryAdd(Constants.RESULT, Result);
        }

        public override void FromX6Json(Cell cell)
        {
            if (cell == null || cell.data == null) return;

            base.FromX6Json(cell);

            if (cell.data.TryGetString(Constants.DISPLAYNAME, out var displayName)) DisplayName = displayName;
            if (cell.data.TryGetString(Constants.UNIQUEID, out var uniqueId)) UniqueID = uniqueId;

            if (cell.data.TryGetGuid(Constants.RABBITMQPUBLISH_SOURCEID, out var resourceid)) RabbitMQSourceResourceId = resourceid;
            if (cell.data.TryGetString(Constants.RABBITMQPUBLISH_QUEUENAME, out var queuename)) QueueName = queuename;
            if (cell.data.TryGetBool(Constants.RABBITMQPUBLISH_SETTINGS_DURABLE, out var isdurable)) IsDurable = isdurable;
            if (cell.data.TryGetBool(Constants.RABBITMQPUBLISH_SETTINGS_EXCLUSIVE, out var isexclusive)) IsExclusive = isexclusive;
            if (cell.data.TryGetBool(Constants.RABBITMQPUBLISH_SETTINGS_AUTODELETE, out var isautodelete)) IsAutoDelete = isautodelete;
            if (cell.data.TryGetString(Constants.RABBITMQPUBLISH_MESSAGE, out var message)) Message = message;
            if (cell.data.TryGetRabbitMqPublishOptions(out var basicproperties)) BasicProperties = basicproperties;
            if (cell.data.TryGetString(Constants.RESULT, out var result)) Result = result;

        }
    }
}
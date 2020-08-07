#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using System;
using System.Activities;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Dev2.Common.State;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Warewolf.Data.Options;

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

        private IDSFDataObject DataObject { get; set; }

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            DataObject = dataObject;
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
                    using (Channel = Connection.CreateModel())
                    {
                        Channel.ExchangeDeclare(queueName, ExchangeType.Direct, IsDurable, IsAutoDelete, null);
                        Channel.QueueDeclare(queueName, IsDurable, IsExclusive, IsAutoDelete, null);
                        Channel.QueueBind(queueName, queueName, "", new Dictionary<string, object>());

                        var basicProperties = Channel.CreateBasicProperties();
                        basicProperties.Persistent = true;
                        basicProperties.CorrelationId = CorrelationID;
                        Channel.BasicPublish(queueName, "", basicProperties, Encoding.UTF8.GetBytes(message));
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
                var expr = DataObject.Environment.EvalToExpression(properties.CorrelationID, 0);
                return Warewolf.Storage.ExecutionEnvironment.WarewolfEvalResultToString(DataObject.Environment.Eval(expr, 0, false, true));
            }
            else
            {
                if (BasicProperties.AutoCorrelation is CustomTransactionID || DataObject.CustomTransactionID.Length > 0)
                {
                    return DataObject.CustomTransactionID;
                }

                if (BasicProperties.AutoCorrelation is ExecutionID)
                {
                    return DataObject.ExecutionID.ToString();
                }

                return DataObject.ExecutionID.ToString();
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
    }
}
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

using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.ServiceModel;
using Dev2.Util;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Dev2.Common.State;

namespace Dev2.Activities.RabbitMQ.Publish
{
    public class DsfPublishRabbitMQActivity : DsfBaseActivity, IEquatable<DsfPublishRabbitMQActivity>
    {
        #region Ctor

        public DsfPublishRabbitMQActivity()
        {
            DisplayName = "RabbitMQ Publish";
        }

        #endregion Ctor

        public Guid RabbitMQSourceResourceId { get; set; }

        [Inputs("Queue Name")]
        [FindMissing]
        public string QueueName { get; set; }

        [FindMissing]
        public bool IsDurable { get; set; }

        [FindMissing]
        public bool IsExclusive { get; set; }

        [FindMissing]
        public bool IsAutoDelete { get; set; }

        [Inputs("Message")]
        [FindMissing]
        public string Message { get; set; }

        [NonSerialized]
        ConnectionFactory _connectionFactory;

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

        internal IConnection Connection { get; set; }

        internal IModel Channel { get; set; }

        public RabbitMQSource RabbitMQSource { get; set; }


        public override IEnumerable<StateVariable> GetState()
        {
            return new[] {
                new StateVariable
                {
                    Name = "QueueName",
                    Value = QueueName,
                    Type = StateVariable.StateType.Input
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
                },new StateVariable
                {
                    Name = "IsAutoDelete",
                    Value = IsAutoDelete.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name="Result",
                    Value = Result,
                    Type = StateVariable.StateType.Output
                }                
            };
        }

        #region Overrides of DsfBaseActivity

        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            try
            {
                RabbitMQSource = ResourceCatalog.GetResource<RabbitMQSource>(GlobalConstants.ServerWorkspaceID, RabbitMQSourceResourceId);
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
                        Channel.BasicPublish(queueName, "", basicProperties, Encoding.UTF8.GetBytes(message));
                    }
                }
                Dev2Logger.Debug($"Message published to queue {queueName}", GlobalConstants.WarewolfDebug);
                return new List<string> { "Success" };
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("PublishRabbitMQActivity", ex, GlobalConstants.WarewolfError);
                throw new Exception(ex.GetAllMessages());
            }
        }

        #endregion Overrides of DsfBaseActivity

#pragma warning disable S1541 // Methods and properties should not be too complex
        public bool Equals(DsfPublishRabbitMQActivity other)
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

            var isSourceEqual = CommonEqualityOps.AreObjectsEqual<IResource>(RabbitMQSource,other.RabbitMQSource);
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

            return Equals((DsfPublishRabbitMQActivity)obj);
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
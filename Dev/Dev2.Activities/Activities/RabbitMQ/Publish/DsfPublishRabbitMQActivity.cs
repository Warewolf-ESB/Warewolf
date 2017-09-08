/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;

namespace Dev2.Activities.RabbitMQ.Publish
{
    [ToolDescriptorInfo("RabbitMq", "RabbitMQ Publish", ToolType.Native, "FFEC6885-597E-49A2-A1AD-AE81E33DF809", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_Rabbit_MQ_Publish")]
    public class DsfPublishRabbitMQActivity : DsfBaseActivity,IEquatable<DsfPublishRabbitMQActivity>
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

        internal IConnection Connection { get; set; }

        internal IModel Channel { get; set; }

        internal RabbitMQSource RabbitMQSource { get; set; }

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

                string queueName, message;
                if (!evaluatedValues.TryGetValue("QueueName", out queueName) ||
                    !evaluatedValues.TryGetValue("Message", out message))
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

                        IBasicProperties basicProperties = Channel.CreateBasicProperties();
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

        public bool Equals(DsfPublishRabbitMQActivity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            bool sourceIsSame;
            var b = RabbitMQSource == null && other.RabbitMQSource != null;
            var b1 = other.RabbitMQSource == null && RabbitMQSource != null;
            if (b || b1)
            {
                sourceIsSame = false;
            }
            else if (RabbitMQSource == null && other.RabbitMQSource == null)
            {
                sourceIsSame = true;
            }
            else
            {
                sourceIsSame = RabbitMQSource?.Equals(other.RabbitMQSource) ?? false;
            }
            return base.Equals(other)
                && RabbitMQSourceResourceId.Equals(other.RabbitMQSourceResourceId) 
                && string.Equals(QueueName, other.QueueName) 
                && IsDurable == other.IsDurable 
                && IsExclusive == other.IsExclusive 
                && IsAutoDelete == other.IsAutoDelete
                && string.Equals(Message, other.Message)  
                && string.Equals(DisplayName, other.DisplayName)  
                && sourceIsSame;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DsfPublishRabbitMQActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
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
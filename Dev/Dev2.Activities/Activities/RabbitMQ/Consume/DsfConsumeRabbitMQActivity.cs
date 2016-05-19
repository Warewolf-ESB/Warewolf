/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

// ReSharper disable VirtualMemberCallInContructor
// ReSharper disable InconsistentNaming
namespace Dev2.Activities.RabbitMQ.Consume
{
    [ToolDescriptorInfo("RabbitMq", "RabbitMQ Consume", ToolType.Native, "406ea660-64cf-4c82-b6f0-42d48172a799", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfConsumeRabbitMQActivity : DsfBaseActivity
    {
        #region Ctor

        public DsfConsumeRabbitMQActivity()
        {
            DisplayName = "RabbitMQ Consume";
        }

        #endregion Ctor

        public Guid RabbitMQSourceResourceId { get; set; }

        [Inputs("Queue Name")]
        [FindMissing]
        public string QueueName { get; set; }
        
        public QueueingBasicConsumer Consumer { get; set; }

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

        public override string DisplayName { get; set; }

        protected override string PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            try
            {
                RabbitMQSource = ResourceCatalog.GetResource<RabbitMQSource>(GlobalConstants.ServerWorkspaceID, RabbitMQSourceResourceId);
                if (RabbitMQSource == null || RabbitMQSource.ResourceType != "RabbitMQSource")
                {
                    return "Failure: Source has been deleted.";
                }

                string queueName;
                if (!evaluatedValues.TryGetValue("QueueName", out queueName))
                {
                    return "Failure: Queue Name is required.";
                }
                QueueName = queueName;
                ConnectionFactory.HostName = RabbitMQSource.HostName;
                ConnectionFactory.Port = RabbitMQSource.Port;
                ConnectionFactory.UserName = RabbitMQSource.UserName;
                ConnectionFactory.Password = RabbitMQSource.Password;
                ConnectionFactory.VirtualHost = RabbitMQSource.VirtualHost;

                using (Connection = ConnectionFactory.CreateConnection())
                {
                    using (Channel = Connection.CreateModel())
                    {
                        Channel.BasicQos(0,1,false);

                        Consumer = new QueueingBasicConsumer(Channel);
                        var consumer = Consumer;
                        Channel.BasicConsume(queue: queueName,
                            noAck: false,
                            consumer: consumer);
                        //var deleveryArgs = consumer.Queue.Dequeue();
                        //Channel.BasicAck(deleveryArgs.DeliveryTag, false);
                    }
                }
                Dev2Logger.Debug(String.Format("Message consumed from queue {0}", queueName));
                return "Success";
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("ConsumeRabbitMQActivity", ex);
                throw new Exception(ex.GetAllMessages());
            }
        }

        #endregion Overrides of DsfBaseActivity
    }
}
﻿/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.ServiceModel;
using Dev2.Util;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;

// ReSharper disable InconsistentNaming

namespace Dev2.Activities.RabbitMQ.Publish
{
    [ToolDescriptorInfo("Utility-PublishRabbitMQ", "RabbitMQ Publish", ToolType.Native, "FFEC6885-597E-49A2-A1AD-AE81E33DF809", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfPublishRabbitMQActivity : DsfBaseActivity
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

        public override string DisplayName { get; set; }

        protected override string PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            try
            {
                RabbitMQSource = ResourceCatalog.GetResource<RabbitMQSource>(GlobalConstants.ServerWorkspaceID, RabbitMQSourceResourceId);
                if (RabbitMQSource == null || RabbitMQSource.ResourceType != ResourceType.RabbitMQSource)
                {
                    return "Failure: Source has been deleted.";
                }

                string queueName, message;
                if (!evaluatedValues.TryGetValue("QueueName", out queueName) ||
                    !evaluatedValues.TryGetValue("Message", out message))
                {
                    return "Failure: Queue Name and Message are required.";
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
                Dev2Logger.Debug(String.Format("Message published to queue {0}", queueName));
                return "Success";
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("PublishRabbitMQActivity", ex);
                return "Failure";
            }
        }

        #endregion Overrides of DsfBaseActivity
    }
}
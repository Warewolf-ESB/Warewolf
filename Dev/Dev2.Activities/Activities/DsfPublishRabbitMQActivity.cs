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

namespace Dev2.Activities
{
    [ToolDescriptorInfo("Utility-PublishRabbitMQ", "RabbitMQ Publish", ToolType.Native, "FFEC6885-597E-49A2-A1AD-AE81E33DF809", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    // ReSharper disable InconsistentNaming
    public class DsfPublishRabbitMQActivity : DsfBaseActivity
    // ReSharper restore InconsistentNaming
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

        [Inputs("Is Durable")]
        [FindMissing]
        public bool IsDurable { get; set; }

        [Inputs("Is Exclusive")]
        [FindMissing]
        public bool IsExclusive { get; set; }

        [Inputs("Is Auto Delete")]
        [FindMissing]
        public bool IsAutoDelete { get; set; }

        [Inputs("Message")]
        [FindMissing]
        public string Message { get; set; }

        #region Overrides of DsfBaseActivity

        public override string DisplayName { get; set; }

        protected override string PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            try
            {
                RabbitMQSource rabbitMQSource = ResourceCatalog.GetResource<RabbitMQSource>(GlobalConstants.ServerWorkspaceID, RabbitMQSourceResourceId);
                if (rabbitMQSource == null || rabbitMQSource.ResourceType != ResourceType.RabbitMQSource)
                {
                    return "Failure: Source has been deleted.";
                }

                ConnectionFactory factory = new ConnectionFactory()
                {
                    HostName = rabbitMQSource.Host,
                    Port = rabbitMQSource.Port,
                    UserName = rabbitMQSource.UserName,
                    Password = rabbitMQSource.Password,
                    VirtualHost = rabbitMQSource.VirtualHost
                };

                string queueName = evaluatedValues["QueueName"], message = evaluatedValues["Message"];
                bool isDurable, isExclusive, isAutoDelete;

                if (!bool.TryParse(evaluatedValues["IsDurable"], out isDurable))
                    isDurable = false;

                if (!bool.TryParse(evaluatedValues["IsExclusive"], out isExclusive))
                    isExclusive = false;

                if (!bool.TryParse(evaluatedValues["IsAutoDelete"], out isAutoDelete))
                    isAutoDelete = false;

                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: queueName,
                                                durable: isDurable,
                                                exclusive: isExclusive,
                                                autoDelete: isAutoDelete,
                                                arguments: null);

                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(exchange: "",
                            routingKey: queueName,
                            basicProperties: null,
                            body: body);
                    }
                }
                Dev2Logger.Debug(String.Format("Message published to queue {0}", queueName));
                return "Success";
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("SharepointReadListActivity", ex);
                return "Failure";
            }
        }

        #endregion Overrides of DsfBaseActivity
    }
}
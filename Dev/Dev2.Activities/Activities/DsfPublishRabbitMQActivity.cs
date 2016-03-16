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

        // ReSharper disable InconsistentNaming
        public Guid RabbitMQSourceResourceId { get; set; }
        // ReSharper restore InconsistentNaming

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

        #region Overrides of DsfBaseActivity

        public override string DisplayName { get; set; }

        protected override string PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            try
            {
                // ReSharper disable InconsistentNaming
                RabbitMQSource rabbitMQSource = new RabbitMQSource()
                {
                    ResourceID = new Guid("00000000-0000-0000-0000-000000000001"),
                    ResourceType = ResourceType.RabbitMQSource,
                    ResourceName = "Test (localhost)",
                    Host = "localhost",
                    UserName = "guest",
                    Password = "guest"
                };

                // TODO: Remove the above stub and uncomment below when new RabbitMQSource has been implemented WOLF-1523
                //RabbitMQSource rabbitMQSource = ResourceCatalog.GetResource<RabbitMQSource>(GlobalConstants.ServerWorkspaceID, RabbitMQSourceResourceId);
                if (rabbitMQSource == null || rabbitMQSource.ResourceType != ResourceType.RabbitMQSource)
                {
                    return "Failure: Source has been deleted.";
                }
                // ReSharper restore InconsistentNaming

                ConnectionFactory factory = new ConnectionFactory()
                {
                    HostName = rabbitMQSource.Host,
                    Port = rabbitMQSource.Port,
                    UserName = rabbitMQSource.UserName,
                    Password = rabbitMQSource.Password,
                    VirtualHost = rabbitMQSource.VirtualHost
                };

                string queueName = evaluatedValues["QueueName"], message = evaluatedValues["Message"];

                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: queueName,
                                                durable: IsDurable,
                                                exclusive: IsExclusive,
                                                autoDelete: IsAutoDelete,
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
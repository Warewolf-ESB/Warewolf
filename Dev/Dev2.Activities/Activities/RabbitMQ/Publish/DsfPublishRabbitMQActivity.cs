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
using Dev2.Common.Interfaces;
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

namespace Dev2.Activities.RabbitMQ.Publish
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

        [NonSerialized]
        private IConnectionFactory _connectionFactory;

        internal IConnectionFactory ConnectionFactory
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

        // ReSharper disable InconsistentNaming
        internal IRabbitMQSource RabbitMQSource { get; set; }

        // TODO: Remove the below fake when new RabbitMQSource has been implemented - WOLF-1523
        [NonSerialized]
        private IFakeRabbitMQSourceCatalog _fakeRabbitMQSourceCatalog;

        internal IFakeRabbitMQSourceCatalog FakeRabbitMQSourceCatalog { get { return _fakeRabbitMQSourceCatalog ?? (_fakeRabbitMQSourceCatalog = new FakeRabbitMQSourceCatalog()); } set { _fakeRabbitMQSourceCatalog = value; } }

        // ReSharper restore InconsistentNaming

        #region Overrides of DsfBaseActivity

        public override string DisplayName { get; set; }

        protected override string PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            try
            {
                RabbitMQSource = FakeRabbitMQSourceCatalog.GetRabbitMQSourceStub();
                // TODO: Remove the above stub and uncomment below when new RabbitMQSource has been implemented - WOLF-1523
                //RabbitMQSource = ResourceCatalog.GetResource<RabbitMQSource>(GlobalConstants.ServerWorkspaceID, RabbitMQSourceResourceId);
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

                using (Connection = ConnectionFactory.CreateConnection())
                {
                    using (Channel = Connection.CreateModel())
                    {
                        Channel.QueueDeclare(queue: queueName,
                                                durable: IsDurable,
                                                exclusive: IsExclusive,
                                                autoDelete: IsAutoDelete,
                                                arguments: null);

                        byte[] body = Encoding.UTF8.GetBytes(message);

                        Channel.BasicPublish(exchange: "",
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
                Dev2Logger.Error("PublishRabbitMQActivity", ex);
                return "Failure";
            }
        }

        #endregion Overrides of DsfBaseActivity
    }

    // TODO: Remove the belwo fake when new RabbitMQSource has been implemented - WOLF-1523
    // ReSharper disable InconsistentNaming
    public interface IFakeRabbitMQSourceCatalog { IRabbitMQSource GetRabbitMQSourceStub(); }

    public class FakeRabbitMQSourceCatalog : IFakeRabbitMQSourceCatalog
    {
        public IRabbitMQSource GetRabbitMQSourceStub()
        // ReSharper restore InconsistentNaming
        {
            return new RabbitMQSource()
            {
                ResourceID = new Guid("00000000-0000-0000-0000-000000000001"),
                ResourceType = ResourceType.RabbitMQSource,
                ResourceName = "Test (localhost)",
                Host = "localhost",
                UserName = "guest",
                Password = "guest"
            };
        }
    }
}
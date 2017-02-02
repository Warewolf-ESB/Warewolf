/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using RabbitMQ.Client;
using System;
using System.Text;
using Dev2.Runtime.Interfaces;

// ReSharper disable InconsistentNaming
namespace Dev2.Runtime.ServiceModel
{
    public class RabbitMQSources : ExceptionManager
    {
        private readonly IResourceCatalog _resourceCatalog;

        #region CTOR

        public RabbitMQSources()
            : this(ResourceCatalog.Instance)
        {
        }

        public RabbitMQSources(IResourceCatalog resourceCatalog)
        {
            if (resourceCatalog == null)
            {
                throw new ArgumentNullException("resourceCatalog");
            }
            _resourceCatalog = resourceCatalog;
        }

        #endregion CTOR

        #region Test

        public ValidationResult Test(RabbitMQSource source)
        {
            try
            {
                return CanConnectServer(source);
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                return new ValidationResult { IsValid = false, ErrorMessage = ex.Message };
            }
        }

        #endregion Test

        #region CanConnectServer

        private ValidationResult CanConnectServer(RabbitMQSource rabbitMQSource)
        {
            try
            {
                IConnectionFactory connectionFactory = new ConnectionFactory()
                {
                    HostName = rabbitMQSource.HostName,
                    Port = rabbitMQSource.Port,
                    UserName = rabbitMQSource.UserName,
                    Password = rabbitMQSource.Password,
                    VirtualHost = rabbitMQSource.VirtualHost                  
                };

                using (IConnection connection = connectionFactory.CreateConnection())
                {
                    using (IModel channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: "TestRabbitMQServiceSource",
                                                durable: false,
                                                exclusive: false,
                                                autoDelete: false,
                                                arguments: null);

                        channel.BasicPublish(exchange: "",
                            routingKey: "TestRabbitMQServiceSource",
                            basicProperties: null,
                            body: Encoding.UTF8.GetBytes("Test Message"));

                        channel.QueueDeleteNoWait("TestRabbitMQServiceSource", true, false);
                    }
                }
                return new ValidationResult
                {
                    IsValid = true
                };
            }
            catch (Exception e)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = e.InnerException!=null? string.Join(Environment.NewLine,e.Message,e.InnerException.Message) : e.Message
                };
            }
        }

        #endregion CanConnectServer
    }
}
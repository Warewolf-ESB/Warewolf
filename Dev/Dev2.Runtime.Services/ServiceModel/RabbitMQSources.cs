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
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Xml.Linq;

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

        #region Get

        // POST: Service/WebSources/Get
        public RabbitMQSource Get(string resourceId, Guid workspaceId, Guid dataListId)
        {
            var result = new RabbitMQSource();
            try
            {
                var xmlStr = ResourceCatalog.Instance.GetResourceContents(workspaceId, Guid.Parse(resourceId)).ToString();
                if (!string.IsNullOrEmpty(xmlStr))
                {
                    var xml = XElement.Parse(xmlStr);
                    result = new RabbitMQSource(xml);
                }
            }
            catch (Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        #endregion Get

        #region Save

        // POST: Service/WebSources/Save
        public string Save(string args, Guid workspaceId, Guid dataListId)
        {
            try
            {
                var source = JsonConvert.DeserializeObject<RabbitMQSource>(args);

                _resourceCatalog.SaveResource(workspaceId, source);
                if (workspaceId != GlobalConstants.ServerWorkspaceID)
                {
                    _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, source);
                }

                return source.ToString();
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                return new ValidationResult { IsValid = false, ErrorMessage = ex.Message }.ToString();
            }
        }

        #endregion Save

        #region Test

        // POST: Service/WebSources/Test
        //public ValidationResult Test(string args, Guid workspaceId, Guid dataListId)
        //{
        //    try
        //    {
        //        var source = JsonConvert.DeserializeObject<RabbitMQSource>(args);
        //        return CanConnectServer(source);
        //    }
        //    catch (Exception ex)
        //    {
        //        RaiseError(ex);
        //        return new ValidationResult { IsValid = false, ErrorMessage = ex.Message };
        //    }
        //}

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
                IConnectionFactory connectionFactory = new ConnectionFactory();
                using (IConnection connection = connectionFactory.CreateConnection())
                {
                    using (IModel channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: "TestRabbitMQServiceSource",
                                                durable: false,
                                                exclusive: false,
                                                autoDelete: false,
                                                arguments: null);

                        byte[] body = Encoding.UTF8.GetBytes("Test Message");

                        channel.BasicPublish(exchange: "",
                            routingKey: "TestRabbitMQServiceSource",
                            basicProperties: null,
                            body: body);

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
                    ErrorMessage = e.Message
                };
            }
        }

        #endregion CanConnectServer
    }
}
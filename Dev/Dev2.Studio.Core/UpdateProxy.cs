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

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Interfaces;
using Warewolf.Resource.Errors;
using Warewolf.Service;

namespace Dev2.Studio.Core
{
    public class UpdateProxy : ProxyBase, IUpdateManager
    {
        #region Implementation of IUpdateManager

        public UpdateProxy(ICommunicationControllerFactory communicationControllerFactory, IEnvironmentConnection connection)
            : base(communicationControllerFactory, connection)
        {
        }

        public void SaveServerSource(IServerSource resource, Guid workspaceId)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveServerSourceService");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("ServerSource", serialiser.SerializeToBuilder(resource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
            {
                throw new WarewolfSaveException(ErrorResource.UnableToContactServer, null);
            }

            if (output.HasError)
            {
                throw new WarewolfSaveException(output.Message.ToString(), null);
            }
        }
        
        public void TestConnection(IServerSource resource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestConnectionService");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("ServerSource", serialiser.SerializeToBuilder(resource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
            {
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            }

            if (output.HasError)
            {
                throw new WarewolfTestException(output.Message.ToString(), null);
            }
        }

        public IList<string> TestDbConnection(IDbSource resource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestDbSourceService");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("DbSource", serialiser.SerializeToBuilder(resource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
            {
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            }

            if (output.HasError)
            {
                throw new WarewolfTestException(output.Message.ToString(), null);
            }

            return serialiser.Deserialize<List<string>>(output.Message);
        }

		public IList<string> TestSqliteConnection(ISqliteDBSource resource)
		{
			var con = Connection;
			var comsController = CommunicationControllerFactory.CreateController("TestSqliteService");
			var serialiser = new Dev2JsonSerializer();
			comsController.AddPayloadArgument("SqliteSource", serialiser.SerializeToBuilder(resource));
			var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
			if (output == null)
			{
				throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
			}

			if (output.HasError)
			{
				throw new WarewolfTestException(output.Message.ToString(), null);
			}

			return serialiser.Deserialize<List<string>>(output.Message);
		}

	public void SaveDbSource(IDbSource toDbSource, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveDbSourceService");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("DbSource", serialiser.SerializeToBuilder(toDbSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
            {
                throw new WarewolfSaveException(output.Message.ToString(), null);
            }
        }
        
        public void SaveDbService(IDatabaseService dbService)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveDbService");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("DbService", serialiser.SerializeToBuilder(dbService));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
            {
                throw new WarewolfSaveException(output.Message.ToString(), null);
            }
        }
        
        public DataTable TestDbService(IDatabaseService inputValues)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestDbService");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("DbService", serialiser.SerializeToBuilder(inputValues));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
            {
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            }

            if (output.HasError)
            {
                throw new WarewolfTestException(output.Message.ToString(), null);
            }

            return serialiser.Deserialize<DataTable>(output.Message);
        }
        
        public void SaveWebserviceSource(IWebServiceSource resource, Guid serverWorkspaceId)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveWebserviceSource");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("WebserviceSource", serialiser.SerializeToBuilder(resource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
            {
                throw new WarewolfSaveException(output.Message.ToString(), null);
            }
        }
        
        public void TestConnection(IWebServiceSource resource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestWebserviceSource");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("WebserviceSource", serialiser.SerializeToBuilder(resource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
            {
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            }

            if (output.HasError)
            {
                throw new WarewolfTestException(output.Message.ToString(), null);
            }
        }

        public void SaveRedisServiceSource(IRedisServiceSource redisServiceSource, Guid serverWorkspaceId)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController(nameof(SaveRedisSource));
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument(SaveRedisSource.RedisSource, serialiser.SerializeToBuilder(redisServiceSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
            {
                throw new WarewolfSaveException(output.Message.ToString(), null);
            }
        }

        public void SaveElasticsearchServiceSource(IElasticsearchSourceDefinition elasticsearchServiceSource, Guid serverWorkspaceId)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController(nameof(SaveElasticsearchSource));
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument(SaveElasticsearchSource.ElasticsearchSource, serialiser.SerializeToBuilder(elasticsearchServiceSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
            {
                throw new WarewolfSaveException(output.Message.ToString(), null);
            }
        }
        public void TestConnection(IElasticsearchSourceDefinition elasticsearchServiceSource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController(nameof(TestElasticsearchSource));
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument(TestElasticsearchSource.ElasticsearchSource, serialiser.SerializeToBuilder(elasticsearchServiceSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
            {
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            }

            if (output.HasError)
            {
                throw new WarewolfTestException(output.Message.ToString(), null);
            }
        }
        public void TestConnection(IRedisServiceSource redisServiceSource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController(nameof(TestRedisSource));
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument(TestRedisSource.RedisSource, serialiser.SerializeToBuilder(redisServiceSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
            {
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            }

            if (output.HasError)
            {
                throw new WarewolfTestException(output.Message.ToString(), null);
            }
        }

        public void SaveSharePointServiceSource(ISharepointServerSource resource, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveSharepointServerService");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("SharepointServer", serialiser.SerializeToBuilder(resource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
            {
                throw new WarewolfSaveException(output.Message.ToString(), null);
            }
        }

        public void TestConnection(ISharepointServerSource resource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestSharepointServerService");
            var serialiser = new Dev2JsonSerializer();
            var sharepointSource = new SharepointSource
            {
                AuthenticationType = resource.AuthenticationType,
                Password = resource.Password,
                Server = resource.Server,
                UserName = resource.UserName,
                ResourceName = resource.Name,
                ResourceID = resource.Id
            };
            comsController.AddPayloadArgument("SharepointServer", serialiser.SerializeToBuilder(sharepointSource));
            var output = comsController.ExecuteCommand<SharepointSourceTo>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null || output.TestMessage.Contains("Failed"))
            {
                if (output == null)
                {
                    throw new WarewolfTestException("No Test Response returned", null);
                }
                throw new WarewolfTestException(ErrorResource.UnableToContactServer + " : " + output.TestMessage, null);
            }
            resource.IsSharepointOnline = output.IsSharepointOnline;
        }

        public string TestWebService(IWebService inputValues)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestWebService");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("WebService", serialiser.SerializeToBuilder(inputValues));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
            {
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            }

            if (output.HasError)
            {
                throw new WarewolfTestException(output.Message.ToString(), null);
            }

            return output.Message.ToString();
        }

        public void SaveWebservice(IWebService model, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("Save" +
                                                                                 "WebService");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("Webservice", serialiser.SerializeToBuilder(model));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
            {
                throw new WarewolfSaveException(output.Message.ToString(), null);
            }
        }

        public void SavePluginSource(IPluginSource source, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SavePluginSource");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("PluginSource", serialiser.SerializeToBuilder(source));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
            {
                throw new WarewolfSaveException(output.Message.ToString(), null);
            }
        }


        public void SaveComPluginSource(IComPluginSource source, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveComPluginSource");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("ComPluginSource", serialiser.SerializeToBuilder(source));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
            {
                throw new WarewolfSaveException(output.Message.ToString(), null);
            }
        }

        public void SaveOAuthSource(IOAuthSource source, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveOAuthSource");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("OAuthSource", serialiser.SerializeToBuilder(source));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
            {
                throw new WarewolfSaveException(output.Message.ToString(), null);
            }
        }

        public string TestPluginService(IPluginService inputValues)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestPluginService");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("PluginService", serialiser.SerializeToBuilder(inputValues));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
            {
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            }

            if (output.HasError)
            {
                throw new WarewolfTestException(output.Message.ToString(), null);
            }

            return output.Message.ToString();
        }

        public string TestComPluginService(IComPluginService inputValues)
        {

            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestComPluginService");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("ComPluginService", serialiser.SerializeToBuilder(inputValues));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
            {
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            }

            if (output.HasError)
            {
                throw new WarewolfTestException(output.Message.ToString(), null);
            }

            return output.Message.ToString();
        }

        public string TestEmailServiceSource(IEmailServiceSource emailServiceSource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestEmailServiceSource");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("EmailServiceSource", serialiser.SerializeToBuilder(emailServiceSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
            {
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            }

            if (output.HasError)
            {
                throw new WarewolfTestException(output.Message.ToString(), null);
            }

            return output.Message.ToString();
        }

        public string TestExchangeServiceSource(IExchangeSource emailServiceSource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestExchangeServiceSource");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("ExchangeSource", serialiser.SerializeToBuilder(emailServiceSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
            {
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            }

            if (output.HasError)
            {
                throw new WarewolfTestException(output.Message.ToString(), null);
            }

            return output.Message.ToString();
        }

        public void SaveEmailServiceSource(IEmailServiceSource emailServiceSource, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveEmailServiceSource");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("EmailServiceSource", serialiser.SerializeToBuilder(emailServiceSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
            {
                throw new WarewolfSaveException("No response from server. Please ensure server is connected.", null);
            }
            if (output.HasError)
            {
                throw new WarewolfSaveException(output.Message.ToString(), null);
            }
        }

        public void SaveExchangeSource(IExchangeSource exchangeSource, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveExchangeServiceSource");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("ExchangeSource", serialiser.SerializeToBuilder(exchangeSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
            {
                throw new WarewolfSaveException(output.Message.ToString(), null);
            }
        }

        public void SaveRabbitMQServiceSource(IRabbitMQServiceSourceDefinition rabbitMqServiceSource, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveRabbitMQServiceSource");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("RabbitMQServiceSource", serialiser.SerializeToBuilder(rabbitMqServiceSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
            {
                throw new WarewolfSaveException(output.Message.ToString(), null);
            }
        }


        public string TestRabbitMQServiceSource(IRabbitMQServiceSourceDefinition rabbitMqServiceSource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestRabbitMQServiceSource");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("RabbitMQServiceSource", serialiser.SerializeToBuilder(rabbitMqServiceSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
            {
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            }

            if (output.HasError)
            {
                throw new WarewolfTestException(output.Message.ToString(), null);
            }

            return output.Message.ToString();
        }

        public void SaveWcfSource(IWcfServerSource wcfSource, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveWcfServiceSource");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("WcfSource", serialiser.SerializeToBuilder(wcfSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
            {
                throw new WarewolfSaveException(output.Message.ToString(), null);
            }
        }

        public string TestWcfServiceSource(IWcfServerSource wcfServerSource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestWcfServiceSource");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("WcfSource", serialiser.SerializeToBuilder(wcfServerSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
            {
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            }

            if (output.HasError)
            {
                throw new WarewolfTestException(output.Message.ToString(), null);
            }

            return output.Message.ToString();
        }

        public string TestWcfService(IWcfService service)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestWcfService");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("wcfService", serialiser.SerializeToBuilder(service));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
            {
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            }

            if (output.HasError)
            {
                throw new WarewolfTestException(output.Message.ToString(), null);
            }

            return output.Message.ToString();
        }

        #region Implementation of IUpdateManager

        public List<IDeployResult> Deploy(List<Guid> resourceIDsToDeploy, bool deployTests, IConnection destinationEnvironmentId)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("DirectDeploy");
            var serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("resourceIDsToDeploy", serialiser.SerializeToBuilder(resourceIDsToDeploy));
            comsController.AddPayloadArgument("deployTests", new StringBuilder(deployTests.ToString()));
            comsController.AddPayloadArgument("destinationEnvironmentId", serialiser.SerializeToBuilder(destinationEnvironmentId));
            var output = comsController.ExecuteCommand<List<IDeployResult>>(con, GlobalConstants.ServerWorkspaceID);
            return output;
        }

        #endregion

        #endregion Implementation of IUpdateManager
    }

    public class WarewolfSaveException : WarewolfException
    {
        public WarewolfSaveException(string message, Exception innerException)
            : base(message, innerException, ExceptionType.Execution, ExceptionSeverity.Error)
        {
        }
    }

    public class WarewolfTestException : WarewolfException
    {
        public WarewolfTestException(string message, Exception innerException)
            : base(message, innerException, ExceptionType.Execution, ExceptionSeverity.User)
        {
        }
    }

    public class WarewolfSupportServiceException : WarewolfException
    {
        public WarewolfSupportServiceException(string message, Exception innerException)
            : base(message, innerException, ExceptionType.Execution, ExceptionSeverity.Minor)
        {
        }
    }
}

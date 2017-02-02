using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ErrorHandling;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Warewolf.Resource.Errors;

namespace Warewolf.Studio.ServerProxyLayer
{
    public class UpdateProxy : ProxyBase, IUpdateManager
    {
        #region Implementation of IUpdateManager

        public UpdateProxy(ICommunicationControllerFactory communicationControllerFactory, IEnvironmentConnection connection)
            : base(communicationControllerFactory, connection)
        {
        }

        /// <summary>
        /// Save a resource to the server
        /// </summary>
        /// <param name="resource">resource to save</param>
        /// <param name="workspaceId">the workspace to save to</param>
        /// <exception cref="WarewolfSaveException">Unable to contact server</exception>
        public void SaveServerSource(IServerSource resource, Guid workspaceId)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveServerSourceService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("ServerSource", serialiser.SerializeToBuilder(resource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
                throw new WarewolfSaveException(ErrorResource.UnableToContactServer, null);
            if (output.HasError)
                throw new WarewolfSaveException(output.Message.ToString(), null);
        }

        /// <summary>
        /// Tests if a valid connection to a server can be made
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public void TestConnection(IServerSource resource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestConnectionService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("ServerSource", serialiser.SerializeToBuilder(resource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            if (output.HasError)
                throw new WarewolfTestException(output.Message.ToString(), null);
        }

        /// <summary>
        /// Tests if a valid connection to a server can be made returns 'Success' on a successful connection
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        /// <exception cref="WarewolfTestException">Unable to contact Server</exception>
        public IList<string> TestDbConnection(IDbSource resource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestDbSourceService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("DbSource", serialiser.SerializeToBuilder(resource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            if (output.HasError)
                throw new WarewolfTestException(output.Message.ToString(), null);
            return serialiser.Deserialize<List<string>>(output.Message);
        }

        /// <exception cref="WarewolfSaveException">When saving the Database Source errors.</exception>
        public void SaveDbSource(IDbSource toDbSource, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveDbSourceService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("DbSource", serialiser.SerializeToBuilder(toDbSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
                throw new WarewolfSaveException(output.Message.ToString(), null);
        }

        /// <exception cref="WarewolfSaveException">Thrown when saving the Database service fails.</exception>
        public void SaveDbService(IDatabaseService dbService)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveDbService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("DbService", serialiser.SerializeToBuilder(dbService));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
                throw new WarewolfSaveException(output.Message.ToString(), null);
        }

        /// <exception cref="WarewolfTestException">Unable to contact Server</exception>
        public DataTable TestDbService(IDatabaseService service)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestDbService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("DbService", serialiser.SerializeToBuilder(service));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            if (output.HasError)
                throw new WarewolfTestException(output.Message.ToString(), null);
            return serialiser.Deserialize<DataTable>(output.Message);
        }

        /// <exception cref="WarewolfSaveException">Thrown when an error occurs saving the Webservice Source.</exception>
        public void SaveWebserviceSource(IWebServiceSource resource, Guid serverWorkspaceId)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveWebserviceSource");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("WebserviceSource", serialiser.SerializeToBuilder(resource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
                throw new WarewolfSaveException(output.Message.ToString(), null);
        }

        /// <exception cref="WarewolfTestException">Unable to contact Server</exception>
        public void TestConnection(IWebServiceSource resource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestWebserviceSource");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("WebserviceSource", serialiser.SerializeToBuilder(resource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            if (output.HasError)
                throw new WarewolfTestException(output.Message.ToString(), null);
        }

        public void SaveSharePointServiceSource(ISharepointServerSource resource, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveSharepointServerService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("SharepointServer", serialiser.SerializeToBuilder(resource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
                throw new WarewolfSaveException(output.Message.ToString(), null);
        }

        public void TestConnection(ISharepointServerSource resource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestSharepointServerService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
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

        public string TestWebService(IWebService service)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestWebService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("WebService", serialiser.SerializeToBuilder(service));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            if (output.HasError)
                throw new WarewolfTestException(output.Message.ToString(), null);
            return output.Message.ToString();
        }

        public void SaveWebservice(IWebService model, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("Save" +
                                                                                 "WebService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("Webservice", serialiser.SerializeToBuilder(model));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
                throw new WarewolfSaveException(output.Message.ToString(), null);
        }

        public void SavePluginSource(IPluginSource source, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SavePluginSource");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("PluginSource", serialiser.SerializeToBuilder(source));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
                throw new WarewolfSaveException(output.Message.ToString(), null);
        }


        public void SaveComPluginSource(IComPluginSource source, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveComPluginSource");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("ComPluginSource", serialiser.SerializeToBuilder(source));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
                throw new WarewolfSaveException(output.Message.ToString(), null);
        }

        public void SaveOAuthSource(IOAuthSource source, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveOAuthSource");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("OAuthSource", serialiser.SerializeToBuilder(source));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
                throw new WarewolfSaveException(output.Message.ToString(), null);
        }

        public string TestPluginService(IPluginService plugin)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestPluginService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("PluginService", serialiser.SerializeToBuilder(plugin));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            if (output.HasError)
                throw new WarewolfTestException(output.Message.ToString(), null);
            return output.Message.ToString();
        }

        public string TestComPluginService(IComPluginService plugin)
        {

            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestComPluginService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("ComPluginService", serialiser.SerializeToBuilder(plugin));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            if (output.HasError)
                throw new WarewolfTestException(output.Message.ToString(), null);
            return output.Message.ToString();
        }

        public void SavePluginService(IPluginService service)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SavePluginService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("PluginService", serialiser.SerializeToBuilder(service));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
                throw new WarewolfSaveException(output.Message.ToString(), null);
        }

        public void SaveComPluginService(IComPluginService service)
        {
            throw new NotImplementedException();
            /*var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SavePluginService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("PluginService", serialiser.SerializeToBuilder(service));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
                throw new WarewolfSaveException(output.Message.ToString(), null);*/
        }

        public string TestEmailServiceSource(IEmailServiceSource emailServiceSource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestEmailServiceSource");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("EmailServiceSource", serialiser.SerializeToBuilder(emailServiceSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            if (output.HasError)
                throw new WarewolfTestException(output.Message.ToString(), null);
            return output.Message.ToString();
        }

        public string TestExchangeServiceSource(IExchangeSource emailServiceSource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestExchangeServiceSource");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("ExchangeSource", serialiser.SerializeToBuilder(emailServiceSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            if (output.HasError)
                throw new WarewolfTestException(output.Message.ToString(), null);
            return output.Message.ToString();
        }

        public void SaveEmailServiceSource(IEmailServiceSource model, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveEmailServiceSource");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("EmailServiceSource", serialiser.SerializeToBuilder(model));
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

        public void SaveExchangeSource(IExchangeSource model, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveExchangeServiceSource");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("ExchangeSource", serialiser.SerializeToBuilder(model));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
                throw new WarewolfSaveException(output.Message.ToString(), null);
        }

        public void SaveRabbitMQServiceSource(IRabbitMQServiceSourceDefinition rabbitMqServiceSource, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveRabbitMQServiceSource");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("RabbitMQServiceSource", serialiser.SerializeToBuilder(rabbitMqServiceSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
                throw new WarewolfSaveException(output.Message.ToString(), null);
        }

        // ReSharper disable once InconsistentNaming
        public string TestRabbitMQServiceSource(IRabbitMQServiceSourceDefinition rabbitMqServiceSource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestRabbitMQServiceSource");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("RabbitMQServiceSource", serialiser.SerializeToBuilder(rabbitMqServiceSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            if (output.HasError)
                throw new WarewolfTestException(output.Message.ToString(), null);
            return output.Message.ToString();
        }

        public void SaveWcfSource(IWcfServerSource model, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveWcfServiceSource");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("WcfSource", serialiser.SerializeToBuilder(model));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output.HasError)
                throw new WarewolfSaveException(output.Message.ToString(), null);
        }

        public string TestWcfServiceSource(IWcfServerSource wcfServerSource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestWcfServiceSource");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("WcfSource", serialiser.SerializeToBuilder(wcfServerSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            if (output.HasError)
                throw new WarewolfTestException(output.Message.ToString(), null);
            return output.Message.ToString();
        }

        public string TestWcfService(IWcfService wcfService)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestWcfService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("wcfService", serialiser.SerializeToBuilder(wcfService));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
                throw new WarewolfTestException(ErrorResource.UnableToContactServer, null);
            if (output.HasError)
                throw new WarewolfTestException(output.Message.ToString(), null);
            return output.Message.ToString();
        }

        #region Implementation of IUpdateManager

        public void Deploy(List<Guid> resourceIDsToDeploy, bool deployTests, IConnection destinationEnvironment)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("DirectDeploy");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("resourceIDsToDeploy", serialiser.SerializeToBuilder(resourceIDsToDeploy));
            comsController.AddPayloadArgument("deployTests", new StringBuilder(deployTests.ToString()));
            comsController.AddPayloadArgument("destinationEnvironmentId", serialiser.SerializeToBuilder(destinationEnvironment));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
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
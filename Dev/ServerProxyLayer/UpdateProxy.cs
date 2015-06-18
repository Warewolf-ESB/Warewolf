using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Email;
using Dev2.Common.Interfaces.ErrorHandling;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.ServerDialogue;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;

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
        /// Deploy a resource. order of execution is gauranteed
        /// </summary>
        /// <param name="resource"></param>
        /// <exception cref="ArgumentNullException"><paramref name="resource"/> is <see langword="null" />.</exception>
        public void DeployItem(IResource resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            var comsController = CommunicationControllerFactory.CreateController("DeployResourceService");
            comsController.AddPayloadArgument("Roles", "*");

            var con = Connection;
            comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
        }

        /// <summary>
        /// Save a resource to the server
        /// </summary>
        /// <param name="resource">resource to save</param>
        /// <param name="workspaceId">workspace to save to </param>
        public void SaveResource(StringBuilder resource, Guid workspaceId)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveResourceService");
            comsController.AddPayloadArgument("ResourceXml", resource);
            comsController.ExecuteCommand<IExecuteMessage>(con, workspaceId);
        }

        /// <summary>
        /// Save a resource to the server
        /// </summary>
        /// <param name="resource">resource to save</param>
        /// <param name="workspaceId">the workspace to save to</param>
        /// <exception cref="WarewolfSaveException">Thrown when error occurs.</exception>
        public void SaveResource(IResource resource, Guid workspaceId)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveResourceService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("ServerSource", serialiser.SerializeToBuilder(resource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, workspaceId);
            if (output.HasError)
                throw new WarewolfSaveException(output.Message.ToString(), null);

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
                throw new WarewolfSaveException( "Unable to contact server",null);
            if (output.HasError)
                throw new WarewolfSaveException(output.Message.ToString(), null);

            
        }

        /// <summary>
        /// Tests if a valid connection to a server can be made
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public string TestConnection(IServerSource resource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestConnectionService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("ServerSource", serialiser.SerializeToBuilder(resource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
                return "Unable to contact server";
            if (output.HasError)
                return output.Message.ToString();

            return output.Message.ToString();


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
                throw new WarewolfTestException("Unable to contact Server", null);
            if(output.HasError)
                throw new WarewolfTestException(output.Message.ToString(),null);
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
            if(output.HasError)
                throw  new WarewolfSaveException(output.Message.ToString(),null);
            
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
                throw new WarewolfTestException("Unable to contact Server", null);
            if (output.HasError)
                throw new WarewolfTestException(output.Message.ToString(), null);
            return serialiser.Deserialize<DataTable>(output.Message);
        }

        /// <exception cref="WarewolfSaveException">Thrown when an error occurs saving the Webservice Source.</exception>
        public void SaveWebserviceSource(IWebServiceSource resource, Guid serverWorkspaceID)
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
                throw new WarewolfTestException("Unable to contact Server", null);
            if (output.HasError)
                throw new WarewolfTestException(output.Message.ToString(), null);
        }

        public string TestWebService(IWebService service)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestWebService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("WebService", serialiser.SerializeToBuilder(service));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
                throw new WarewolfTestException("Unable to contact Server", null);
            if (output.HasError)
                throw new WarewolfTestException(output.Message.ToString(), null);
            return output.Message.ToString();
        }

        public void SaveWebservice(IWebService model, Guid serverWorkspaceID)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveWebService");
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

        public string TestPluginService(IPluginService plugin)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestPluginService");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("PluginService", serialiser.SerializeToBuilder(plugin));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
                throw new WarewolfTestException("Unable to contact Server", null);
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

        public string TestEmailServiceSource(IEmailServiceSource emailServiceSource)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("TestEmailServiceSource");
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            comsController.AddPayloadArgument("EmailServiceSource", serialiser.SerializeToBuilder(emailServiceSource));
            var output = comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
            if (output == null)
                throw new WarewolfTestException("Unable to contact Server", null);
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
            if (output.HasError)
                throw new WarewolfSaveException(output.Message.ToString(), null);
        }

        #endregion
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

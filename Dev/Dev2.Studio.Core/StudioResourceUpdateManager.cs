using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Common.Interfaces.WebServices;
using Dev2.ConnectionHelpers;
using Dev2.Controller;
using Dev2.Studio.Interfaces;



namespace Dev2.Studio.Core
{
    public class StudioResourceUpdateManager : IStudioUpdateManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="controllerFactory"/> is <see langword="null" />.</exception>
        public StudioResourceUpdateManager(ICommunicationControllerFactory controllerFactory, IEnvironmentConnection environmentConnection)
        {
            if (controllerFactory == null)
            {
                throw new ArgumentNullException(nameof(controllerFactory));
            }
            if (environmentConnection == null)
            {
                throw new ArgumentNullException(nameof(environmentConnection));
            }

            UpdateManagerProxy = new UpdateProxy(controllerFactory, environmentConnection);
        }

        public void FireServerSaved(Guid savedServerID) => FireServerSaved(savedServerID, false);
        public void FireServerSaved(Guid savedServerID, bool isDeleted)
        {
            if (ServerSaved != null)
            {
                var handler = ServerSaved;
                handler.Invoke(savedServerID, isDeleted);
            }
        }

        private IUpdateManager UpdateManagerProxy { get; set; }

        public void Save(IServerSource serverSource)
        {
            UpdateManagerProxy.SaveServerSource(serverSource, GlobalConstants.ServerWorkspaceID);
            ConnectControlSingleton.Instance.ReloadServer();
            FireServerSaved(serverSource.ID);
        }

        public void Save(IPluginSource source)
        {
            UpdateManagerProxy.SavePluginSource(source, GlobalConstants.ServerWorkspaceID);
        }

        public void Save(IComPluginSource source)
        {
            UpdateManagerProxy.SaveComPluginSource(source, GlobalConstants.ServerWorkspaceID);
        }

        public void Save(IOAuthSource source)
        {
            UpdateManagerProxy.SaveOAuthSource(source, GlobalConstants.ServerWorkspaceID);
        }

        public void Save(IEmailServiceSource emailServiceSource)
        {
            UpdateManagerProxy.SaveEmailServiceSource(emailServiceSource, GlobalConstants.ServerWorkspaceID);
        }


        public void Save(IRabbitMQServiceSourceDefinition rabbitMqServiceSource)
        {
            UpdateManagerProxy.SaveRabbitMQServiceSource(rabbitMqServiceSource, GlobalConstants.ServerWorkspaceID);
        }
        public void Save(IExchangeSource exchangeSource)
        {
            UpdateManagerProxy.SaveExchangeSource(exchangeSource, GlobalConstants.ServerWorkspaceID);
        }

        public void TestConnection(IServerSource serverSource)
        {
            UpdateManagerProxy.TestConnection(serverSource);
        }

        public string TestConnection(IEmailServiceSource emailServiceSource)
        {
            return UpdateManagerProxy.TestEmailServiceSource(emailServiceSource);
        }


        public string TestConnection(IRabbitMQServiceSourceDefinition rabbitMqServiceSource)
        {
            return UpdateManagerProxy.TestRabbitMQServiceSource(rabbitMqServiceSource);
        }
        public string TestConnection(IExchangeSource emailServiceSourceSource)
        {
            return UpdateManagerProxy.TestExchangeServiceSource(emailServiceSourceSource);
        }

        public void TestConnection(IWebServiceSource resource)
        {
            UpdateManagerProxy.TestConnection(resource);
        }

        public void TestConnection(ISharepointServerSource resource)
        {
            UpdateManagerProxy.TestConnection(resource);
        }

        public IList<string> TestDbConnection(IDbSource serverSource)
        {
            return UpdateManagerProxy.TestDbConnection(serverSource);
        }

        public void Save(IDbSource toDbSource)
        {
            UpdateManagerProxy.SaveDbSource(toDbSource, GlobalConstants.ServerWorkspaceID);
        }

        public void Save(IWebService model)
        {
            UpdateManagerProxy.SaveWebservice(model, GlobalConstants.ServerWorkspaceID);
        }

        public void Save(IWebServiceSource resource)
        {
            try
            {
                UpdateManagerProxy.SaveWebserviceSource(resource, GlobalConstants.ServerWorkspaceID);
            }
            catch (Exception)
            {
                //
            }
        }

        public void Save(ISharepointServerSource resource)
        {
            try
            {
                UpdateManagerProxy.SaveSharePointServiceSource(resource, GlobalConstants.ServerWorkspaceID);
            }
            catch (Exception)
            {
                //
            }
        }

        public void Save(IDatabaseService toDbSource)
        {
            UpdateManagerProxy.SaveDbService(toDbSource);
        }

        public DataTable TestDbService(IDatabaseService inputValues)
        {
            return UpdateManagerProxy.TestDbService(inputValues);
        }

        public string TestWebService(IWebService inputValues)
        {
            return UpdateManagerProxy.TestWebService(inputValues);
        }

        public string TestPluginService(IPluginService inputValues)
        {
            return UpdateManagerProxy.TestPluginService(inputValues);
        }

        public string TestPluginService(IComPluginService inputValues)
        {
            return UpdateManagerProxy.TestComPluginService(inputValues);
        }

        public void Save(IWcfServerSource wcfSource)
        {
            UpdateManagerProxy.SaveWcfSource(wcfSource, GlobalConstants.ServerWorkspaceID);
        }

        public string TestWcfService(IWcfService inputValues)
        {
            return UpdateManagerProxy.TestWcfService(inputValues);
        }

        public string TestConnection(IWcfServerSource wcfServerSource)
        {
            return UpdateManagerProxy.TestWcfServiceSource(wcfServerSource);
        }

        public Action<Guid, bool> ServerSaved { get; set; }

        #region Implementation of IStudioUpdateManager

        public List<IDeployResult> Deploy(List<Guid> resourceIDsToDeploy, bool deployTests, IConnection destinationEnvironment)
        {
            return UpdateManagerProxy.Deploy(resourceIDsToDeploy, deployTests, destinationEnvironment);
        }

        #endregion
    }
}
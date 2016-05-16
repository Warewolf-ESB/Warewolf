using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Controller;
using Dev2.Studio.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using Warewolf.Studio.ServerProxyLayer;

namespace Warewolf.Studio.AntiCorruptionLayer
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
                throw new ArgumentNullException("controllerFactory");
            }
            if (environmentConnection == null)
            {
                throw new ArgumentNullException("environmentConnection");
            }

            UpdateManagerProxy = new UpdateProxy(controllerFactory, environmentConnection);
        }

        public event ItemSaved ItemSaved;

        public event ServerSaved ServerSaved;

        public void FireItemSaved()
        {
            var handler = ItemSaved;
            if (handler != null)
            {
                handler();
            }
        }

        public void FireServerSaved()
        {
            var handler = ServerSaved;
            if (handler != null)
            {
                handler();
            }
        }

        private IUpdateManager UpdateManagerProxy { get; set; }

        public void Save(IServerSource serverSource)
        {
            UpdateManagerProxy.SaveServerSource(serverSource, GlobalConstants.ServerWorkspaceID);
            if (ItemSaved != null)
            {
                ItemSaved();
            }
            if (ServerSaved != null)
            {
                ServerSaved();
            }
        }

        public void Save(IPluginSource source)
        {
            UpdateManagerProxy.SavePluginSource(source, GlobalConstants.ServerWorkspaceID);
            if (PluginServiceSourceSaved != null)
            {
                PluginServiceSourceSaved(source);
            }
            if (ItemSaved != null)
            {
                ItemSaved();
            }
        }

        public void Save(IEmailServiceSource emailServiceSource)
        {
            UpdateManagerProxy.SaveEmailServiceSource(emailServiceSource, GlobalConstants.ServerWorkspaceID);
            if (EmailServiceSourceSaved != null)
            {
                EmailServiceSourceSaved(emailServiceSource);
            }
            if (ItemSaved != null)
            {
                ItemSaved();
            }
        }

        // ReSharper disable once InconsistentNaming
        public void Save(IRabbitMQServiceSourceDefinition rabbitMQServiceSource)
        {
            UpdateManagerProxy.SaveRabbitMQServiceSource(rabbitMQServiceSource, GlobalConstants.ServerWorkspaceID);
            if (RabbitMQServiceSourceSaved != null)
            {
                RabbitMQServiceSourceSaved(rabbitMQServiceSource);
            }
            if (ItemSaved != null)
            {
                ItemSaved();
            }
        }
        public void Save(IExchangeSource exchangeSource)
        {
            UpdateManagerProxy.SaveExchangeSource(exchangeSource, GlobalConstants.ServerWorkspaceID);
            if (ExchangedServiceSourceSaved != null)
            {
                ExchangedServiceSourceSaved(exchangeSource);
            }
            if (ItemSaved != null)
            {
                ItemSaved();
            }
        }


        public void TestConnection(IServerSource serverSource)
        {
            UpdateManagerProxy.TestConnection(serverSource);
        }

        public string TestConnection(IEmailServiceSource emailServiceSource)
        {
            return UpdateManagerProxy.TestEmailServiceSource(emailServiceSource);
        }

        // ReSharper disable once InconsistentNaming
        public string TestConnection(IRabbitMQServiceSourceDefinition rabbitMQServiceSource)
        {
            return UpdateManagerProxy.TestRabbitMQServiceSource(rabbitMQServiceSource);
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
            if (DatabaseServiceSourceSaved != null)
            {
                DatabaseServiceSourceSaved(toDbSource);
            }
            if (ItemSaved != null)
            {
                ItemSaved();
            }
        }

        public void Save(IWebService model)
        {
            UpdateManagerProxy.SaveWebservice(model, GlobalConstants.ServerWorkspaceID);
            if (ItemSaved != null)
            {
                ItemSaved();
            }
        }

        public void Save(IWebServiceSource resource)
        {
            try
            {
                UpdateManagerProxy.SaveWebserviceSource(resource, GlobalConstants.ServerWorkspaceID);
                if (WebServiceSourceSaved != null)
                {
                    WebServiceSourceSaved(resource);
                }
                if (ItemSaved != null)
                {
                    ItemSaved();
                }
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
                if (SharePointServiceSourceSaved != null)
                {
                    SharePointServiceSourceSaved(resource);
                }
                if (ItemSaved != null)
                {
                    ItemSaved();
                }
            }
            catch (Exception)
            {
                //
            }
        }

        public void Save(IDatabaseService toDbSource)
        {
            UpdateManagerProxy.SaveDbService(toDbSource);
            if (ItemSaved != null)
            {
                ItemSaved();
            }
        }

        public DataTable TestDbService(IDatabaseService inputValues)
        {
            return UpdateManagerProxy.TestDbService(inputValues);
        }

        public string TestWebService(IWebService inputValues)
        {
            return UpdateManagerProxy.TestWebService(inputValues);
        }

        public event Action<IWebServiceSource> WebServiceSourceSaved;

        public event Action<IDbSource> DatabaseServiceSourceSaved;

        public event Action<IPluginSource> PluginServiceSourceSaved;

        public event Action<IEmailServiceSource> EmailServiceSourceSaved;
        public event Action<IExchangeSource> ExchangedServiceSourceSaved;

        public event Action<IRabbitMQServiceSourceDefinition> RabbitMQServiceSourceSaved;

        public event Action<ISharepointServerSource> SharePointServiceSourceSaved;

        public string TestPluginService(IPluginService inputValues)
        {
            return UpdateManagerProxy.TestPluginService(inputValues);
        }

        public void Save(IPluginService toDbSource)
        {
            UpdateManagerProxy.SavePluginService(toDbSource);
            if (ItemSaved != null)
            {
                ItemSaved();
            }
        }
    }
}
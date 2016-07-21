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
                throw new ArgumentNullException(nameof(controllerFactory));
            }
            if (environmentConnection == null)
            {
                throw new ArgumentNullException(nameof(environmentConnection));
            }

            UpdateManagerProxy = new UpdateProxy(controllerFactory, environmentConnection);
        }

        public event ItemSaved ItemSaved;

        public event ServerSaved ServerSaved;

        public void FireItemSaved()
        {
            var handler = ItemSaved;
            handler?.Invoke();
        }

        public void FireServerSaved()
        {
            var handler = ServerSaved;
            handler?.Invoke();
        }

        private IUpdateManager UpdateManagerProxy { get; set; }

        public void Save(IServerSource serverSource)
        {
            UpdateManagerProxy.SaveServerSource(serverSource, GlobalConstants.ServerWorkspaceID);
            ItemSaved?.Invoke();
            ServerSaved?.Invoke();
        }

        public void Save(IPluginSource source)
        {
            UpdateManagerProxy.SavePluginSource(source, GlobalConstants.ServerWorkspaceID);
            PluginServiceSourceSaved?.Invoke(source);
            ItemSaved?.Invoke();
        }

        public void Save(IComPluginSource source)
        {
            UpdateManagerProxy.SaveComPluginSource(source, GlobalConstants.ServerWorkspaceID);
            ComPluginServiceSourceSaved?.Invoke(source);
            ItemSaved?.Invoke();
        }

        public void Save(IOAuthSource source)
        {
            UpdateManagerProxy.SaveOAuthSource(source, GlobalConstants.ServerWorkspaceID);
            OAuthSourceSaved?.Invoke(source);
            ItemSaved?.Invoke();
        }

        public void Save(IEmailServiceSource emailServiceSource)
        {
            UpdateManagerProxy.SaveEmailServiceSource(emailServiceSource, GlobalConstants.ServerWorkspaceID);
            EmailServiceSourceSaved?.Invoke(emailServiceSource);
            ItemSaved?.Invoke();
        }

        // ReSharper disable once InconsistentNaming
        public void Save(IRabbitMQServiceSourceDefinition rabbitMqServiceSource)
        {
            UpdateManagerProxy.SaveRabbitMQServiceSource(rabbitMqServiceSource, GlobalConstants.ServerWorkspaceID);
            RabbitMQServiceSourceSaved?.Invoke(rabbitMqServiceSource);
            ItemSaved?.Invoke();
        }
        public void Save(IExchangeSource exchangeSource)
        {
            UpdateManagerProxy.SaveExchangeSource(exchangeSource, GlobalConstants.ServerWorkspaceID);
            ExchangedServiceSourceSaved?.Invoke(exchangeSource);
            ItemSaved?.Invoke();
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
            DatabaseServiceSourceSaved?.Invoke(toDbSource);
            ItemSaved?.Invoke();
        }

        public void Save(IWebService model)
        {
            UpdateManagerProxy.SaveWebservice(model, GlobalConstants.ServerWorkspaceID);
            ItemSaved?.Invoke();
        }

        public void Save(IWebServiceSource resource)
        {
            try
            {
                UpdateManagerProxy.SaveWebserviceSource(resource, GlobalConstants.ServerWorkspaceID);
                WebServiceSourceSaved?.Invoke(resource);
                ItemSaved?.Invoke();
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
                SharePointServiceSourceSaved?.Invoke(resource);
                ItemSaved?.Invoke();
            }
            catch (Exception)
            {
                //
            }
        }

        public void Save(IDatabaseService toDbSource)
        {
            UpdateManagerProxy.SaveDbService(toDbSource);
            ItemSaved?.Invoke();
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
        public event Action<IComPluginSource> ComPluginServiceSourceSaved;

        public event Action<IOAuthSource> OAuthSourceSaved;
        public event Action<IEmailServiceSource> EmailServiceSourceSaved;
        public event Action<IExchangeSource> ExchangedServiceSourceSaved;

        public event Action<IRabbitMQServiceSourceDefinition> RabbitMQServiceSourceSaved;

        public event Action<ISharepointServerSource> SharePointServiceSourceSaved;

        public event Action<IWcfServerSource> WcfSourceSaved;

        public string TestPluginService(IPluginService inputValues)
        {
            return UpdateManagerProxy.TestPluginService(inputValues);
        }

        public string TestPluginService(IComPluginService inputValues)
        {
            return UpdateManagerProxy.TestComPluginService(inputValues);
        }



        public void Save(IPluginService toDbSource)
        {
            UpdateManagerProxy.SavePluginService(toDbSource);
            ItemSaved?.Invoke();
        }

        public void Save(IWcfService toSource)
        {
            throw new NotImplementedException();
        }

        public void Save(IWcfServerSource wcfSource)
        {
            UpdateManagerProxy.SaveWcfSource(wcfSource, GlobalConstants.ServerWorkspaceID);
            WcfSourceSaved?.Invoke(wcfSource);
            ItemSaved?.Invoke();
        }

        public string TestWcfService(IWcfService inputValues)
        {
            return UpdateManagerProxy.TestWcfService(inputValues);
        }

        public string TestConnection(IWcfServerSource wcfServerSource)
        {
            return UpdateManagerProxy.TestWcfServiceSource(wcfServerSource);
        }
    }
}
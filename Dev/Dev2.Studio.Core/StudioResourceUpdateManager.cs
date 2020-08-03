#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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

        IUpdateManager UpdateManagerProxy { get; set; }

        public void Save(IServerSource serverSource)
        {
            UpdateManagerProxy.SaveServerSource(serverSource, GlobalConstants.ServerWorkspaceID);
            ConnectControlSingleton.Instance.ReloadServer();
            FireServerSaved(serverSource.ID);
        }

        public void Save(IPluginSource source) => UpdateManagerProxy.SavePluginSource(source, GlobalConstants.ServerWorkspaceID);

        public void Save(IComPluginSource source) => UpdateManagerProxy.SaveComPluginSource(source, GlobalConstants.ServerWorkspaceID);

        public void Save(IOAuthSource sharePointServiceSource) => UpdateManagerProxy.SaveOAuthSource(sharePointServiceSource, GlobalConstants.ServerWorkspaceID);

        public void Save(IEmailServiceSource emailServiceSource) => UpdateManagerProxy.SaveEmailServiceSource(emailServiceSource, GlobalConstants.ServerWorkspaceID);

        public void Save(IRabbitMQServiceSourceDefinition rabbitMqServiceSource) => UpdateManagerProxy.SaveRabbitMQServiceSource(rabbitMqServiceSource, GlobalConstants.ServerWorkspaceID);

        public void Save(IExchangeSource emailServiceSource) => UpdateManagerProxy.SaveExchangeSource(emailServiceSource, GlobalConstants.ServerWorkspaceID);

        public void TestConnection(IServerSource serverSource) => UpdateManagerProxy.TestConnection(serverSource);

        public string TestConnection(IEmailServiceSource emailServiceSource) => UpdateManagerProxy.TestEmailServiceSource(emailServiceSource);

        public string TestConnection(IRabbitMQServiceSourceDefinition rabbitMqServiceSource) => UpdateManagerProxy.TestRabbitMQServiceSource(rabbitMqServiceSource);

        public string TestConnection(IExchangeSource emailServiceSource) => UpdateManagerProxy.TestExchangeServiceSource(emailServiceSource);

        public void TestConnection(IWebServiceSource serverSource) => UpdateManagerProxy.TestConnection(serverSource);

        public void TestConnection(IRedisServiceSource redisServiceSource) => UpdateManagerProxy.TestConnection(redisServiceSource);
        public string TestConnection(IElasticsearchSourceDefinition elasticServiceSource) => UpdateManagerProxy.TestConnection(elasticServiceSource);

        public void TestConnection(ISharepointServerSource sharePointServiceSource) => UpdateManagerProxy.TestConnection(sharePointServiceSource);

        public IList<string> TestDbConnection(IDbSource serverSource) => UpdateManagerProxy.TestDbConnection(serverSource);
		public IList<string> TestSqliteConnection(ISqliteDBSource serverSource) => UpdateManagerProxy.TestSqliteConnection(serverSource);

		public void Save(IDbSource toDbSource) => UpdateManagerProxy.SaveDbSource(toDbSource, GlobalConstants.ServerWorkspaceID);

        public void Save(IWebService model) => UpdateManagerProxy.SaveWebservice(model, GlobalConstants.ServerWorkspaceID);

        public void Save(IWebServiceSource model)
        {
            try
            {
                UpdateManagerProxy.SaveWebserviceSource(model, GlobalConstants.ServerWorkspaceID);
            }
            catch (Exception)
            {
                //
            }
        }

        public void Save(IRedisServiceSource redisServiceSource) => UpdateManagerProxy.SaveRedisServiceSource(redisServiceSource, GlobalConstants.ServerWorkspaceID);
        
        public void Save(IElasticsearchSourceDefinition elasticsearchServiceSource) => UpdateManagerProxy.SaveElasticsearchServiceSource(elasticsearchServiceSource, GlobalConstants.ServerWorkspaceID);

        public void Save(ISharepointServerSource sharePointServiceSource)
        {
            try
            {
                UpdateManagerProxy.SaveSharePointServiceSource(sharePointServiceSource, GlobalConstants.ServerWorkspaceID);
            }
            catch (Exception)
            {
                //
            }
        }

        public void Save(IDatabaseService toDbSource) => UpdateManagerProxy.SaveDbService(toDbSource);

        public DataTable TestDbService(IDatabaseService inputValues) => UpdateManagerProxy.TestDbService(inputValues);

        public string TestWebService(IWebService inputValues) => UpdateManagerProxy.TestWebService(inputValues);

        public string TestPluginService(IPluginService inputValues) => UpdateManagerProxy.TestPluginService(inputValues);

        public string TestPluginService(IComPluginService inputValues) => UpdateManagerProxy.TestComPluginService(inputValues);

        public void Save(IWcfServerSource wcfSource) => UpdateManagerProxy.SaveWcfSource(wcfSource, GlobalConstants.ServerWorkspaceID);

        public string TestWcfService(IWcfService inputValues) => UpdateManagerProxy.TestWcfService(inputValues);

        public string TestConnection(IWcfServerSource wcfServerSource) => UpdateManagerProxy.TestWcfServiceSource(wcfServerSource);

        public Action<Guid, bool> ServerSaved { get; set; }

        public List<IDeployResult> Deploy(List<Guid> resourceIDsToDeploy, bool deployTests, bool deployTriggers, IConnection destinationEnvironment) => UpdateManagerProxy.Deploy(resourceIDsToDeploy, deployTests, deployTriggers, destinationEnvironment);
    }
}
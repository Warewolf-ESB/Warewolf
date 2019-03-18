#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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

        #region Implementation of IStudioUpdateManager

        public List<IDeployResult> Deploy(List<Guid> resourceIDsToDeploy, bool deployTests, IConnection destinationEnvironment) => UpdateManagerProxy.Deploy(resourceIDsToDeploy, deployTests, destinationEnvironment);

        #endregion
    }
}
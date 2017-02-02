using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.WebServices;
using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;

namespace Dev2.Common.Interfaces.ServerProxyLayer
{
    public interface IUpdateManagerSave
    {
        void SaveServerSource(IServerSource resource, Guid workspaceId);
        void SaveDbSource(IDbSource toDbSource, Guid serverWorkspaceID);
        void SaveDbService(IDatabaseService dbService);
        void SaveWebserviceSource(IWebServiceSource resource, Guid serverWorkspaceId);
        void SaveSharePointServiceSource(ISharepointServerSource resource, Guid serverWorkspaceID);
        void SaveWebservice(IWebService model, Guid serverWorkspaceID);
        void SavePluginSource(IPluginSource source, Guid serverWorkspaceID);
        void SaveComPluginSource(IComPluginSource source, Guid serverWorkspaceID);
        void SaveOAuthSource(IOAuthSource source, Guid serverWorkspaceID);
        void SavePluginService(IPluginService toSource);
        void SaveComPluginService(IComPluginService toSource);
        void SaveEmailServiceSource(IEmailServiceSource emailServiceSource, Guid serverWorkspaceID);
        void SaveExchangeSource(IExchangeSource exchangeSource, Guid serverWorkspaceID);
        // ReSharper disable once InconsistentNaming
        void SaveRabbitMQServiceSource(IRabbitMQServiceSourceDefinition rabbitMqServiceSource, Guid serverWorkspaceID);
        void SaveWcfSource(IWcfServerSource wcfSource, Guid serverWorkspaceID);
    }

    public interface IUpdateManagerTest
    {
        void TestConnection(IServerSource resource);
        IList<string> TestDbConnection(IDbSource resource);
        DataTable TestDbService(IDatabaseService inputValues);
        void TestConnection(IWebServiceSource resource);
        void TestConnection(ISharepointServerSource resource);
        string TestWebService(IWebService inputValues);
        string TestPluginService(IPluginService inputValues);
        string TestComPluginService(IComPluginService inputValues);
        string TestEmailServiceSource(IEmailServiceSource emailServiceSource);
        string TestExchangeServiceSource(IExchangeSource emailServiceSource);
        // ReSharper disable once InconsistentNaming
        string TestRabbitMQServiceSource(IRabbitMQServiceSourceDefinition rabbitMqServiceSource);
        string TestWcfServiceSource(IWcfServerSource wcfServerSource);
        string TestWcfService(IWcfService service);
    }

    /// <summary>
    /// Updates resources on a warewolf servers. Order of execution is gaurenteed
    /// </summary>
    public interface IUpdateManager : IUpdateManagerSave, IUpdateManagerTest
    {
        void Deploy(List<Guid> resourceIDsToDeploy, bool deployTests, IConnection destinationEnvironmentId);
    }
}
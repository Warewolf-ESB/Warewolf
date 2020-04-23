#pragma warning disable
 using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.WebServices;
using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;


namespace Dev2.Common.Interfaces.ServerProxyLayer
{
    public interface IUpdateManagerSave
    {
        void SaveServerSource(IServerSource resource, Guid workspaceId);
        void SaveDbSource(IDbSource toDbSource, Guid serverWorkspaceID);
        void SaveDbService(IDatabaseService dbService);
        void SaveWebserviceSource(IWebServiceSource resource, Guid serverWorkspaceId);
        void SaveRedisServiceSource(IRedisServiceSource redisServiceSource, Guid serverWorkspaceId);
        void SaveElasticsearchServiceSource(IElasticsearchSourceDefinition elasticsearchServiceSource, Guid serverWorkspaceId);
        void SaveSharePointServiceSource(ISharepointServerSource resource, Guid serverWorkspaceID);
        void SaveWebservice(IWebService model, Guid serverWorkspaceID);
        void SavePluginSource(IPluginSource source, Guid serverWorkspaceID);
        void SaveComPluginSource(IComPluginSource source, Guid serverWorkspaceID);
        void SaveOAuthSource(IOAuthSource source, Guid serverWorkspaceID);
        void SaveEmailServiceSource(IEmailServiceSource emailServiceSource, Guid serverWorkspaceID);
        void SaveExchangeSource(IExchangeSource exchangeSource, Guid serverWorkspaceID);

        void SaveRabbitMQServiceSource(IRabbitMQServiceSourceDefinition rabbitMqServiceSource, Guid serverWorkspaceID);
        void SaveWcfSource(IWcfServerSource wcfSource, Guid serverWorkspaceID);
    }

    public interface IUpdateManagerTest
    {
        void TestConnection(IServerSource resource);
        IList<string> TestDbConnection(IDbSource resource);
		IList<string> TestSqliteConnection(ISqliteDBSource resource);
		DataTable TestDbService(IDatabaseService inputValues);
        void TestConnection(IWebServiceSource resource);
        void TestConnection(IRedisServiceSource redisServiceSource);
        string TestConnection(IElasticsearchSourceDefinition elasticServiceSource);
        void TestConnection(ISharepointServerSource resource);
        string TestWebService(IWebService inputValues);
        string TestPluginService(IPluginService inputValues);
        string TestComPluginService(IComPluginService inputValues);
        string TestEmailServiceSource(IEmailServiceSource emailServiceSource);
        string TestExchangeServiceSource(IExchangeSource emailServiceSource);

        string TestRabbitMQServiceSource(IRabbitMQServiceSourceDefinition rabbitMqServiceSource);
        string TestWcfServiceSource(IWcfServerSource wcfServerSource);
        string TestWcfService(IWcfService service);
    }

    /// <summary>
    /// Updates resources on a warewolf servers. Order of execution is gaurenteed
    /// </summary>
    public interface IUpdateManager : IUpdateManagerSave, IUpdateManagerTest
    {
        List<IDeployResult> Deploy(List<Guid> resourceIDsToDeploy, bool deployTests, IConnection destinationEnvironmentId);
    }
}
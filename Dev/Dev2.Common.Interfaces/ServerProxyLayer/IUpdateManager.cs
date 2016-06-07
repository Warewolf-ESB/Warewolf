using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.WebServices;
using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;

namespace Dev2.Common.Interfaces.ServerProxyLayer
{
    /// <summary>
    /// Updates resources on a warewolf servers. Order of execution is gaurenteed
    /// </summary>
    public interface IUpdateManager
    {

        /// <summary>
        /// Save a resource to the server
        /// </summary>
        /// <param name="resource">resource to save</param>
        /// <param name="workspaceId">the workspace to save to</param>
        void SaveServerSource(IServerSource resource, Guid workspaceId);

        /// <summary>
        /// Tests if a valid connection to a server can be made returns 'Success' on a successful connection
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        void TestConnection(IServerSource resource);

        ///// <summary>
        ///// Tests if a valid connection to a server can be made returns 'Success' on a successful connection
        ///// </summary>
        ///// <param name="resource"></param>
        ///// <returns></returns>
        IList<string> TestDbConnection(IDbSource resource);

        void SaveDbSource(IDbSource toDbSource, Guid serverWorkspaceID);

        void SaveDbService(IDatabaseService dbService);

        DataTable TestDbService(IDatabaseService inputValues);

        void SaveWebserviceSource(IWebServiceSource resource, Guid serverWorkspaceId);

        void TestConnection(IWebServiceSource resource);

        void SaveSharePointServiceSource(ISharepointServerSource resource, Guid serverWorkspaceID);

        void TestConnection(ISharepointServerSource resource);

        string TestWebService(IWebService inputValues);

        void SaveWebservice(IWebService model, Guid serverWorkspaceID);

        void SavePluginSource(IPluginSource source, Guid serverWorkspaceID);

        void SaveOAuthSource(IOAuthSource source, Guid serverWorkspaceID);

        string TestPluginService(IPluginService inputValues);

        void SavePluginService(IPluginService toDbSource);

        string TestEmailServiceSource(IEmailServiceSource emailServiceSource);
        string TestExchangeServiceSource(IExchangeSource emailServiceSource);

        void SaveEmailServiceSource(IEmailServiceSource emailServiceSource, Guid serverWorkspaceID);

        void SaveExchangeSource(IExchangeSource exchangeSource, Guid serverWorkspaceID);
        // ReSharper disable InconsistentNaming
        void SaveRabbitMQServiceSource(IRabbitMQServiceSourceDefinition rabbitMQServiceSource, Guid serverWorkspaceID);

        string TestRabbitMQServiceSource(IRabbitMQServiceSourceDefinition rabbitMQServiceSource);

        // ReSharper enable InconsistentNaming

        void SaveWcfSource(IWcfServerSource wcfSource, Guid serverWorkspaceID);

        string TestWcfServiceSource(IWcfServerSource wcfServerSource);

        string TestWcfService(IWcfService service);

    }
}
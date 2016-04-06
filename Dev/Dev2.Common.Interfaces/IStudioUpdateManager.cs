using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Common.Interfaces.WebServices;

namespace Dev2.Common.Interfaces
{
    public interface IStudioUpdateManager
    {
        void Save(IServerSource serverSource);
        void Save(IDbSource toDbSource);
        void Save(IWebService model);
        void Save(IWebServiceSource model);
        void Save(IDatabaseService toDbSource);
        void Save(IPluginSource source);
        void Save(IEmailServiceSource emailServiceSource);
        void Save(IExchangeSource emailServiceSource);
        void Save(ISharepointServerSource sharePointServiceSource);
    
        void TestConnection(IServerSource serverSource);
        void TestConnection(IWebServiceSource serverSource);
        void TestConnection(ISharepointServerSource sharePointServiceSource);
        string TestConnection(IEmailServiceSource emailServiceSourceSource);
        string TestConnection(IExchangeSource emailServiceSourceSource);
        IList<string> TestDbConnection(IDbSource serverSource);
        DataTable TestDbService(IDatabaseService inputValues);
        string TestWebService(IWebService inputValues);

        event Action<IWebServiceSource> WebServiceSourceSaved;

        string TestPluginService(IPluginService inputValues);

        void Save(IPluginService toDbSource);

        event ItemSaved ItemSaved;
        event ServerSaved ServerSaved;

        void FireItemSaved();

        void FireServerSaved();

        event Action<IDbSource> DatabaseServiceSourceSaved;
        event Action<IPluginSource> PluginServiceSourceSaved;
        event Action<IEmailServiceSource> EmailServiceSourceSaved;
    }

    public delegate void ItemSaved();
    public delegate void ServerSaved();
}
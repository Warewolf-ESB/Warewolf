using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Common.Interfaces.WebServices;
using System;
using System.Collections.Generic;
using System.Data;

namespace Dev2.Common.Interfaces
{
    public interface IStudioUpdateManagerSave
    {
        void Save(IServerSource serverSource);
        void Save(IDbSource toDbSource);
        void Save(IWebService model);
        void Save(IWebServiceSource model);
        void Save(IDatabaseService toDbSource);
        void Save(IPluginSource source);
        void Save(IComPluginSource source);
        void Save(IEmailServiceSource emailServiceSource);
        void Save(IExchangeSource emailServiceSource);
        void Save(ISharepointServerSource sharePointServiceSource);
        void Save(IRabbitMQServiceSourceDefinition rabbitMqServiceSource);
        void Save(IWcfServerSource wcfSource);
        void Save(IPluginService toDbSource);
        void Save(IComPluginService toDbSource);
        void Save(IWcfService toSource);
        
        void Save(IOAuthSource sharePointServiceSource);
    }

    public interface IStudioUpdateManagerTest
    {
        void TestConnection(IServerSource serverSource);
        void TestConnection(IWebServiceSource serverSource);
        void TestConnection(ISharepointServerSource sharePointServiceSource);
        string TestConnection(IEmailServiceSource emailServiceSourceSource);
        string TestConnection(IExchangeSource emailServiceSourceSource);
        string TestConnection(IRabbitMQServiceSourceDefinition rabbitMqServiceSource);
        IList<string> TestDbConnection(IDbSource serverSource);
        DataTable TestDbService(IDatabaseService inputValues);
        string TestWebService(IWebService inputValues);
        string TestPluginService(IPluginService inputValues);
        string TestPluginService(IComPluginService inputValues);
        string TestWcfService(IWcfService inputValues);
    }

    public interface IStudioUpdateManager : IStudioUpdateManagerSave, IStudioUpdateManagerTest
    {
        string TestConnection(IWcfServerSource wcfServerSource);
        event Action<IWebServiceSource> WebServiceSourceSaved;
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
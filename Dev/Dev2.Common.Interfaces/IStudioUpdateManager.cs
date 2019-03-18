#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Common.Interfaces.WebServices;
using System.Collections.Generic;
using System.Data;
using Dev2.Common.Interfaces.Deploy;

namespace Dev2.Common.Interfaces
{
    public interface IStudioUpdateManagerSave
    {
        void Save(IServerSource serverSource);
        void Save(IDbSource toDbSource);
        void Save(IWebServiceSource model);
        void Save(IPluginSource source);
        void Save(IComPluginSource source);
        void Save(IEmailServiceSource emailServiceSource);
        void Save(IExchangeSource emailServiceSource);
        void Save(ISharepointServerSource sharePointServiceSource);
        void Save(IRabbitMQServiceSourceDefinition rabbitMqServiceSource);
        void Save(IWcfServerSource wcfSource);        
        void Save(IOAuthSource sharePointServiceSource);
    }

    public interface IStudioUpdateManagerTest
    {
        void TestConnection(IServerSource serverSource);
        void TestConnection(IWebServiceSource serverSource);
        void TestConnection(ISharepointServerSource sharePointServiceSource);
        string TestConnection(IEmailServiceSource emailServiceSource);
        string TestConnection(IExchangeSource emailServiceSource);
        string TestConnection(IRabbitMQServiceSourceDefinition rabbitMqServiceSource);
        IList<string> TestDbConnection(IDbSource serverSource);
		IList<string> TestSqliteConnection(ISqliteDBSource serverSource);
		DataTable TestDbService(IDatabaseService inputValues);
        string TestWebService(IWebService inputValues);
        string TestPluginService(IPluginService inputValues);
        string TestPluginService(IComPluginService inputValues);
        string TestWcfService(IWcfService inputValues);
    }

    public interface IStudioUpdateManager : IStudioUpdateManagerSave, IStudioUpdateManagerTest
    {
        string TestConnection(IWcfServerSource wcfServerSource);
        Action<Guid, bool> ServerSaved { get; set; }
        void FireServerSaved(Guid savedServerID);
        void FireServerSaved(Guid savedServerID, bool isDeleted);

        List<IDeployResult> Deploy(List<Guid> resourceIDsToDeploy, bool deployTests, IConnection destinationEnvironment);
    }

    public delegate void ItemSaved(bool refresh);

    public delegate void ServerSaved();
}
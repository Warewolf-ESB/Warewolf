using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerDialogue;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebServices;

namespace Dev2.Common.Interfaces.Explorer
{
    public interface IStudioUpdateManager
    {
        void Save(IServerSource serverSource);
        void Save(IDbSource toDbSource);
        void Save(IWebService model);
        void Save(IWebServiceSource model);
        void Save(IDatabaseService toDbSource);

        string TestConnection(IServerSource serverSource);
        void TestConnection(IWebServiceSource serverSource);
        IList<string> TestDbConnection(IDbSource serverSource);
        DataTable TestDbService(IDatabaseService inputValues);
        string TestWebService(IWebService inputValues);

        event Action<IWebServiceSource> WebServiceSourceSaved;
    }
}
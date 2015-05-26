using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerDialogue;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Common.Interfaces.WebServices;

namespace Dev2.Common.Interfaces.Explorer
{
    public interface IExplorerRepository
    {
        bool Rename(IExplorerItemViewModel vm, string newName);

        bool Move(IExplorerItemViewModel explorerItemViewModel, IExplorerItemViewModel destination);

        bool Delete(IExplorerItemViewModel explorerItemViewModel);

        ICollection<IVersionInfo> GetVersions(Guid id);

        IRollbackResult Rollback(Guid resourceId, string version);

        void CreateFolder(Guid parentGuid, string name, Guid id);
    }

    public interface IStudioUpdateManager
    {
        void Save(IServerSource serverSource);

        string TestConnection(IServerSource serverSource);
        void TestConnection(IWebServiceSource serverSource);
        
        IList<string> TestDbConnection(IDbSource serverSource);

        void Save(IDbSource toDbSource);
        void Save(IWebServiceSource toDbSource);

        void Save(IDatabaseService toDbSource);

        DataTable TestDbService(IDatabaseService inputValues);

        string TestWebService(IWebService inputValues);
    }
}

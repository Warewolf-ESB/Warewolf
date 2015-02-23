using System.Collections.Generic;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;

namespace Warewolf.Studio.ViewModels
{
    public class ManageDatabaseSourceModel : IManageDatabaseSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;

        public ManageDatabaseSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;
        }

        #region Implementation of IManageDatabaseSourceModel

        public IList<string> GetComputerNames()
        {
            return _queryProxy.GetComputerNames();
        }

        public IList<string> TestDbConnection(IDbSource resource)
        {
            return _updateRepository.TestDbConnection(resource);
        }

        public void Save(IDbSource toDbSource)
        {
            _updateRepository.Save(toDbSource);
        }

        #endregion
    }
}

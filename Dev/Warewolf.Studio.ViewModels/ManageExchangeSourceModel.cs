using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class ManageExchangeSourceModel : IManageExchangeSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;

        public ManageExchangeSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, string serverName)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;

            ServerName = serverName;
            if (ServerName.Contains("("))
            {
                ServerName = serverName.Substring(0, serverName.IndexOf("(", System.StringComparison.Ordinal));
            }
        }

        #region Implementation of IManageDatabaseSourceModel

        public string TestConnection(IExchangeServiceSource resource)
        {
            return _updateRepository.TestConnection(resource);
        }

        public void Save(IExchangeServiceSource toDbSource)
        {
            _updateRepository.Save(toDbSource);
        }

        public string ServerName { get; private set; }

        #endregion
    }
}

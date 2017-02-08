using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class ManageComPluginSourceModel : IManageComPluginSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;

        public ManageComPluginSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, string serverName)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;

            ServerName = serverName;
            if (ServerName.Contains("("))
            {
                ServerName = serverName.Substring(0, serverName.IndexOf("(", StringComparison.Ordinal));
            }

        }

        #region Implementation of IManageDatabaseSourceModel

        public string ServerName { get; private set; }

        public IList<IFileListing> GetComDllListings(IFileListing listing)
        {
            return _queryProxy.GetComDllListings(listing);
        }

        public void Save(IComPluginSource source)
        {
            _updateRepository.Save(source);
        }

        #endregion
    }
}
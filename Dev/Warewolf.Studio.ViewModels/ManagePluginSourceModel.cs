using System;
using System.Collections.Generic;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Runtime.ServiceModel.Data;

namespace Warewolf.Studio.ViewModels
{
    public class ManagePluginSourceModel : IManagePluginSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;

        public ManagePluginSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, string serverName)
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

        public IList<IFileListing> GetDllListings(IFileListing listing)
        {
            return _queryProxy.GetDllListings(listing);
        }

        public void Save(IPluginSource source)
        {
            _updateRepository.Save(source);
        }

        public IPluginSource FetchSource(Guid resourceID)
        {
            var xaml = _queryProxy.FetchResourceXaml(resourceID);
            var db = new PluginSource(xaml.ToXElement());

            var def = new PluginSourceDefinition(db);

            return def;
        }

        #endregion
    }
}

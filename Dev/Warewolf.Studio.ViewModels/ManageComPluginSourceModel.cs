using System;
using System.Collections.Generic;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Runtime.ServiceModel.Data;

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

        public IComPluginSource FetchSource(Guid pluginSourceId)
        {
            var xaml = _queryProxy.FetchResourceXaml(pluginSourceId);
            var db = new ComPluginSource(xaml.ToXElement());

            var def = new ComPluginSourceDefinition(db);
            return def;
        }

        #endregion
    }
}
#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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

        public IList<IFileListing> GetComDllListings(IFileListing listing) => _queryProxy.GetComDllListings(listing);

        public void Save(IComPluginSource toDbSource)
        {
            _updateRepository.Save(toDbSource);
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
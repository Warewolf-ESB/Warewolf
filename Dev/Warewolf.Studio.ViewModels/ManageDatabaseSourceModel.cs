/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Runtime.ServiceModel.Data;

namespace Warewolf.Studio.ViewModels
{
    public class ManageDatabaseSourceModel : IManageDatabaseSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;

        public ManageDatabaseSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, string serverName)
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

        public IDbSource FetchDbSource(Guid resourceID)
        {
            var xaml = _queryProxy.FetchResourceXaml(resourceID);
            var db = new DbSource(xaml.ToXElement());
            var def = new DbSourceDefinition(db);
            return def; 
        }

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

        public string ServerName { get; private set; }

        #endregion
    }
}

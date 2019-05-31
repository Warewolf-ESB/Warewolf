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
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.ServiceModel;

namespace Warewolf.Studio.ViewModels
{
    public class ManageNewServerSourceModel: IManageServerSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;

        public ManageNewServerSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, string serverName)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;

            ServerName = serverName;
            if (ServerName.Contains("("))
            {
                ServerName = serverName.Substring(0, serverName.IndexOf("(", StringComparison.Ordinal));
            }

        }

        #region Implementation of IManageServerSourceModel

        public IList<string> GetComputerNames() => _queryProxy.GetComputerNames();

        public void TestConnection(IServerSource resource) => _updateRepository.TestConnection(resource);

        public void Save(IServerSource toDbSource) => _updateRepository.Save(toDbSource);

        public string ServerName { get; set; }

        public IServerSource FetchSource(Guid resourceID)
        {
            var xaml = _queryProxy.FetchResourceXaml(resourceID);
            
            var connection = new Connection(xaml.ToXElement());
            string address = null;
            if (Uri.TryCreate(connection.Address, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                address = uri.Host;
            }

            var selectedServer = new ServerSource
            {
                Address = connection.Address,
                ID = connection.ResourceID,
                AuthenticationType = connection.AuthenticationType,
                UserName = connection.UserName,
                Password = connection.Password,
                ResourcePath = connection.GetSavePath(),
                ServerName = address,
                Name = connection.ResourceName
            };
            return selectedServer;
        }

        #endregion
    }
}

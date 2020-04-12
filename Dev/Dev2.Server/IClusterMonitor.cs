/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Net;
using Dev2.Common;
using Dev2.Runtime.Interfaces;
using Warewolf.Client;

namespace Dev2
{
    public interface IClusterMonitor
    {
        void Start(ClusterSettings clusterSettings, IResourceCatalog resourceCatalog, IWriter writer);
    }

    public class ClusterMonitor : IClusterMonitor
    {
        public void Start(ClusterSettings clusterSettings, IResourceCatalog resourceCatalog, IWriter writer)
        {
            var resource = resourceCatalog.GetResource<Data.ServiceModel.Connection>(GlobalConstants.ServerWorkspaceID,
                clusterSettings.LeaderServerResource.Value);

            var credentials = CredentialCache.DefaultNetworkCredentials;
            var havePassword = string.IsNullOrWhiteSpace(resource.UserName) && string.IsNullOrWhiteSpace(resource.Password);
            if (havePassword)
            {
                credentials = new NetworkCredential(resource.UserName, resource.Password);
            }

            var connection = new ProxyConnection(resource.Address, credentials);
            var req = new ClusterJoinRequest(Config.Cluster.LeaderServerKey);
            var response = req.Execute(connection.ConnectedProxy, 3);
            if (response.Result.Token == Guid.Empty)
            {
                writer.WriteLine("cluster join request failed");
            }
        }
    }
}
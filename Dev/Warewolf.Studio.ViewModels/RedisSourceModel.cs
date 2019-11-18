/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Data.ServiceModel;
using System;

namespace Warewolf.Studio.ViewModels
{
    public class RedisSourceModel : IRedisSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;

        public RedisSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, string serverName)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;

            ServerName = serverName;
            if (ServerName.Contains("("))
            {
                ServerName = serverName.Substring(0, serverName.IndexOf("(", StringComparison.Ordinal));
            }
        }

        public string ServerName { get; set; }

        public IRedisServiceSource FetchSource(Guid id)
        {
            var xaml = _queryProxy.FetchResourceXaml(id);
            var redisSource = new RedisSource(xaml.ToXElement());

            var def = new RedisSourceDefinition(redisSource);
            return def;
        }

        public void Save(IRedisServiceSource toRedisSource)
        {
            _updateRepository.Save(toRedisSource);
        }

        public void TestConnection(IRedisServiceSource resource)
        {
            _updateRepository.TestConnection(resource);
        }
    }
}

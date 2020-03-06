/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
    public class ElasticsearchSourceModel : IElasticsearchSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;

        public string ServerName { get; set; }
        
        public ElasticsearchSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, string serverName)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;

            ServerName = serverName;
            if (ServerName.Contains("("))
            {
                ServerName = serverName.Substring(0, serverName.IndexOf("(", StringComparison.Ordinal));
            }
        }
        
        public IElasticsearchServiceSource FetchSource(Guid id)
        {
            var xaml = _queryProxy.FetchResourceXaml(id);
            var elasticsearchSource = new ElasticsearchSource(xaml.ToXElement());

            var def = new ElasticsearchSourceDefinition(elasticsearchSource);
            return def;
        }
        
        public void Save(IElasticsearchServiceSource toElasticsearchSource)
        {
            _updateRepository.Save(toElasticsearchSource);
        }
        
        public void TestConnection(IElasticsearchServiceSource resource)
        {
            _updateRepository.TestConnection(resource);
        }
    }

  
}
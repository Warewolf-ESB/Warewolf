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
using System.Collections.Generic;
using Dev2.Studio.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class ElasticsearchSourceModel : IElasticsearchSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryManager;
        readonly IShellViewModel _shellViewModel;
        public string ServerName { get; set; }
        
        public ElasticsearchSourceModel(IStudioUpdateManager updateManager, IQueryManager queryManager, IShellViewModel shellViewModel)
        {
            _updateRepository = updateManager;
            _queryManager = queryManager;
            _shellViewModel = shellViewModel;
        }
        
        public ElasticsearchSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryManager, string serverName)
        {
            _updateRepository = updateRepository;
            _queryManager = queryManager;

            ServerName = serverName;
            if (ServerName.Contains("("))
            {
                ServerName = serverName.Substring(0, serverName.IndexOf("(", StringComparison.Ordinal));
            }
        }
        
        public void CreateNewSource() => _shellViewModel.NewElasticsearchSource(string.Empty);
        public ICollection<IElasticsearchSourceDefinition> RetrieveSources() => new List<IElasticsearchSourceDefinition>(_queryManager.FetchElasticsearchServiceSources());
        public void EditSource(IElasticsearchSourceDefinition source) => _shellViewModel.EditResource(source);
        
        public IElasticsearchSourceDefinition FetchSource(Guid id)
        {
            var xaml = _queryManager.FetchResourceXaml(id);
            var source = new ElasticsearchSource(xaml.ToXElement());

            var def = new ElasticsearchSourceDefinition(source);
            return def;
        }
        public void Save(IElasticsearchSourceDefinition source) => _updateRepository.Save(source);
        
        public string TestConnection(IElasticsearchSourceDefinition source) => _updateRepository.TestConnection(source);
    }
}
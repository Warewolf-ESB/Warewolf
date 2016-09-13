/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Warewolf.Studio.ViewModels
{
    public class ManageDbServiceModel : IDbServiceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;
        readonly IShellViewModel _shell;

        #region Implementation of IDbServiceModel

        public ManageDbServiceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, IShellViewModel shell, IServer server)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;
            _shell = shell;
            shell.SetActiveServer(server);
        }

        public IStudioUpdateManager UpdateRepository => _updateRepository;

        public ObservableCollection<IDbSource> RetrieveSources()
        {
            return new ObservableCollection<IDbSource>(_queryProxy.FetchDbSources());
        }

        public ICollection<IDbAction> GetActions(IDbSource source)
        {
            return _queryProxy.FetchDbActions(source);
        }

        public ICollection<IDbAction> RefreshActions(IDbSource source)
        {
            source.ReloadActions = true;
            return _queryProxy.FetchDbActions(source);
        }

        public void CreateNewSource()
        {
            _shell.NewDatabaseSource(string.Empty);
        }

        public void EditSource(IDbSource selectedSource)
        {
            _shell.EditResource(selectedSource);
        }

        public DataTable TestService(IDatabaseService inputValues)
        {
            return _updateRepository.TestDbService(inputValues);
        }

        public IEnumerable<IServiceOutputMapping> GetDbOutputMappings(IDbAction action)
        {
            return new List<IServiceOutputMapping>();
        }

        public void SaveService(IDatabaseService toModel)
        {
            _updateRepository.Save(toModel);
        }

        #endregion
    }
}

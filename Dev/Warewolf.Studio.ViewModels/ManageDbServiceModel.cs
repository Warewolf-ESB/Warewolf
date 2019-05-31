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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Studio.Interfaces;
using System;

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
            shell.SetActiveServer(server.EnvironmentID);
        }

        public IStudioUpdateManager UpdateRepository => _updateRepository;

        public ObservableCollection<IDbSource> RetrieveSources() => new ObservableCollection<IDbSource>(_queryProxy.FetchDbSources());

        public ICollection<IDbAction> GetActions(IDbSource source) => _queryProxy.FetchDbActions(source);

        public void CreateNewSource(enSourceType type)
        {
            switch (type)
            {
                case enSourceType.SqlDatabase:
                    _shell.NewSqlServerSource(string.Empty);
                    break;
                case enSourceType.MySqlDatabase:
                    _shell.NewMySqlSource(string.Empty);
                    break;
                case enSourceType.PostgreSQL:
                    _shell.NewPostgreSqlSource(string.Empty);
                    break;
                case enSourceType.Oracle:
                    _shell.NewOracleSource(string.Empty);
                    break;
                case enSourceType.ODBC:
                    _shell.NewOdbcSource(string.Empty);
                    break;
                default:
                    throw new ArgumentException("Unrecognized Source Type: " + type.ToString());
            }
        }

        public void EditSource(IDbSource selectedSource, enSourceType type)
        {
            switch (type)
            {
                case enSourceType.SqlDatabase:
                    _shell.EditSqlServerResource(selectedSource);
                    break;
                case enSourceType.MySqlDatabase:
                    _shell.EditMySqlResource(selectedSource);
                    break;
                case enSourceType.PostgreSQL:
                    _shell.EditPostgreSqlResource(selectedSource);
                    break;
                case enSourceType.Oracle:
                    _shell.EditOracleResource(selectedSource);
                    break;
                case enSourceType.ODBC:
                    _shell.EditOdbcResource(selectedSource);
                    break;
                default:
                    throw new ArgumentException("Unrecognized Source Type: " + type.ToString());
            }
        }

        public DataTable TestService(IDatabaseService inputValues) => _updateRepository.TestDbService(inputValues);

        #endregion
    }
}

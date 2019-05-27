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
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Studio.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class ManageWcfServiceModel : IWcfServiceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;
        readonly IShellViewModel _shell;

        public ManageWcfServiceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, IShellViewModel shell, IServer server)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>
            {
                { "updateRepository", updateRepository },
                { "queryProxy", queryProxy },
                { "shell", shell } ,
                {"server",server}
            });
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;
            _shell = shell;
            shell.SetActiveServer(server.EnvironmentID);
        }

        public IStudioUpdateManager UpdateRepository => _updateRepository;

        public ObservableCollection<IWcfServerSource> RetrieveSources() => new ObservableCollection<IWcfServerSource>(_queryProxy.FetchWcfSources());

        public ICollection<IWcfAction> GetActions(IWcfServerSource source) => _queryProxy.WcfActions(source).ToArray();

        public void CreateNewSource()
        {
            _shell.NewWcfSource(string.Empty);
        }


        public void EditSource(IWcfServerSource selectedSource)
        {
            _shell.EditResource(selectedSource);
        }

        public string TestService(IWcfService inputValues) => _updateRepository.TestWcfService(inputValues);
    }
}

/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Studio.Interfaces;

namespace Warewolf.Studio.Models
{
    public class ExchangeServiceModel : IExchangeServiceModel
    {
        readonly IServer _server;
        readonly IShellViewModel _shell;

        public ExchangeServiceModel(IServer server, IShellViewModel shell)
        {
            _server = server;
            _shell = shell;
            _shell.SetActiveServer(server.EnvironmentID);
        }
        public ObservableCollection<IExchangeSource> RetrieveSources() => new ObservableCollection<IExchangeSource>(_server.QueryProxy.FetchExchangeSources());

        public void CreateNewSource()
        {
            _shell.NewExchangeSource(string.Empty);
        }

        public void EditSource(IExchangeSource selectedSource)
        {
            _shell.EditResource(selectedSource);
        }

        public IStudioUpdateManager UpdateRepository => _server.UpdateRepository;
    }
}

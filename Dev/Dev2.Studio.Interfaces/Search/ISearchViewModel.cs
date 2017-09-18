/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Dev2.Studio.Interfaces.Search
{
    public delegate void ServerSate(object sender, IServer server);
    public interface ISearchViewModel : IExplorerViewModel
    {
        event ServerSate ServerStateChanged;
        bool IsAllSelected { get; set; }
        bool IsWorkflowNameSelected { get; set; }
        bool IsToolTitleSelected { get; set; }
        bool IsToolNameSelected { get; set; }
        bool IsInputFieldSelected { get; set; }
        bool IsScalarNameSelected { get; set; }
        bool IsObjectNameSelected { get; set; }
        bool IsRecSetNameSelected { get; set; }
        bool IsInputVariableSelected { get; set; }
        bool IsOutputVariableSelected { get; set; }
        bool IsTestNameSelected { get; set; }
        string SearchInput { get; set; }
        ICommand SearchInputCommand { get; set; }
        ObservableCollection<ISearchValue> SearchResults { get; set; }
    }
}

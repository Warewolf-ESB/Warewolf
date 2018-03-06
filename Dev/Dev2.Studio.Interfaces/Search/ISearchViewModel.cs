/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Search;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Dev2.Studio.Interfaces.Search
{
    public delegate void ServerSate(object sender, IServer server);
    public interface ISearchViewModel : IExplorerViewModel
    {
        event ServerSate ServerStateChanged;
        ISearchValue SearchValue { get; set; }
        ICommand SearchInputCommand { get; set; }
        ObservableCollection<ISearchResult> SearchResults { get; set; }
    }
}

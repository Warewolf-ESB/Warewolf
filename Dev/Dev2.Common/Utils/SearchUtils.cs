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
using System.Linq;

namespace Dev2.Common.Utils
{
    public static class SearchUtils
    {
        public static bool FilterText(string valueToFilter, ISearch searchValue)
        {
            var searchInput = searchValue.SearchInput;
            var filterValue = valueToFilter;
            if (!searchValue.SearchOptions.IsMatchCaseSelected)
            {
                searchInput = searchValue.SearchInput.ToLower();
                filterValue = valueToFilter.ToLower();
            }

            var words = filterValue.Split(' ');
            var isMatch = false;
            if (searchValue.SearchOptions.IsMatchWholeWordSelected)
            {
                if (words.Count() == 1)
                {
                    isMatch = words.Equals(searchInput);
                }
                else
                {
                    if (words.Count() > 1)
                    {
                        isMatch = filterValue.Contains(searchInput);
                    }
                }
            }
            else
            {
                isMatch = filterValue.Contains(searchInput);
            }
            return isMatch;
        }
    }
}

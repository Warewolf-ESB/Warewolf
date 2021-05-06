/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Linq;

namespace Warewolf.Data
{
    public static class CollectionExtentions
    {
        public static bool IsItemDuplicate(this ICollection<string> input, string item)
        {
            return input.Any(o => o.Contains(item));
        }

        public static void AddItem(this ICollection<string> input, string item) => AddItem(input, item, false);
        public static void AddItem(this ICollection<string> input, string item, bool checkForDuplicate)
        {
            var itemIsDuplicate = input.IsItemDuplicate(item) && checkForDuplicate;
            if (!itemIsDuplicate)
            {
                input.Add(item);
            }
        }
    }
}

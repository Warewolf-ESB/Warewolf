
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Linq;
using Dev2.Models;

namespace Dev2
{
    public static class TreeEx
    {
        public static IEnumerable<IExplorerItemModel> Descendants(this IExplorerItemModel root)
        {
            var nodes = new Stack<IExplorerItemModel>(new[] { root });
            while(nodes.Any())
            {
                IExplorerItemModel node = nodes.Pop();
                yield return node;
                foreach(var n in node.Children) nodes.Push(n);
            }
        }
    }
}

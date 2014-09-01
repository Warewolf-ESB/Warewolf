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
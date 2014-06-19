using System.Collections.Generic;
using System.Linq;
using Dev2.Models;

namespace Dev2
{
    public static class TreeEx
    {
        public static IEnumerable<ExplorerItemModel> Descendants(this ExplorerItemModel root)
        {
            var nodes = new Stack<ExplorerItemModel>(new[] { root });
            while(nodes.Any())
            {
                ExplorerItemModel node = nodes.Pop();
                yield return node;
                foreach(var n in node.Children) nodes.Push(n);
            }
        }
    }
}
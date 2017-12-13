using System.Collections.Generic;
using System.Linq;
using Infragistics.Controls.Menus;

namespace Warewolf.Studio.Views
{
    public static class TreeUtils
    {        
        public static IEnumerable<XamDataTreeNode> Descendants(XamDataTreeNode[] roots)
        {
            var nodes = new Stack<XamDataTreeNode>(roots);
            while (nodes.Any())
            {
                var node = nodes.Pop();
                yield return node;
            }
        }
    }
}
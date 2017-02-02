using System.Collections.Generic;
using System.Linq;
using Infragistics.Controls.Menus;

namespace Warewolf.Studio.Views
{
    public static class TreeUtils
    {        

        public static IEnumerable<XamDataTreeNode> Descendants(XamDataTreeNode root)
        {
            var nodes = new Stack<XamDataTreeNode>(new[] { root });
            while (nodes.Any())
            {
                XamDataTreeNode node = nodes.Pop();
                yield return node;
                if(node != null)
                {
                    foreach (var n in node.Nodes)
                    {
                        if (n != null)
                        {
                            nodes.Push(n);
                        }
                    }
                }
            }
        }
        
        public static IEnumerable<XamDataTreeNode> Descendants(XamDataTreeNode[] roots)
        {
            var nodes = new Stack<XamDataTreeNode>(roots);
            while (nodes.Any())
            {
                XamDataTreeNode node = nodes.Pop();
                yield return node;
                if(node != null)
                {
                    foreach (var n in node.Nodes)
                    {
                        if (n != null)
                        {
                            nodes.Push(n);
                        }
                    }
                }
            }
        }
    }
}
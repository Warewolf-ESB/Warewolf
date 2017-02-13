using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Common
{
    public class DebugStateTreeBuilder
    {
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static IList<IDebugState> BuildTree(IEnumerable<IDebugState> source)
        {
            var groups = source.GroupBy(i => i.ParentID);

            var roots = groups.First(g => g.Key == Guid.Empty).ToList();

            if (roots.Any())
            {
                var dict = groups.Where(g => g.Key != Guid.Empty).ToDictionary(g => g.Key.ToString(), g => g.ToList());
                for (int i = 0; i < roots.Count(); i++)
                    AddChildren(roots[i], dict);
            }

            return roots;
        }

        private static void AddChildren(IDebugState node, IDictionary<string, List<IDebugState>> source)
        {
            if (source.ContainsKey(node.ID.ToString()))
            {
                node.Children = source[node.ID.ToString()];
                for (int i = 0; i < node.Children.Count; i++)
                    AddChildren(node.Children[i], source);
            }
            else
            {
                node.Children = new List<IDebugState>();
            }
        }
    }
}

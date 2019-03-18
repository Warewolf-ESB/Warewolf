
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Common
{
    public static class DebugStateTreeBuilder
    {
        public static IEnumerable<IDebugState> BuildTree(IEnumerable<IDebugState> source)
        {
            var groups = source?.GroupBy(i => i.ParentID) ?? new List<IGrouping<Guid?, IDebugState>>();
            if (!groups.Any())
            {
                return new List<IDebugState>();
            }
            var roots = groups.First(g => !g.Key.HasValue).ToList();
            roots = roots.Where(state => state.StateType != StateType.Duration).ToList();
            if (roots.Any())
            {
                var dict = groups.Where(g => g.Key.HasValue).ToDictionary(g => g.Key.Value, g => g.ToList());
                for (var i = 0; i < roots.Count(); i++)
                {
                    AddChildren(roots[i], dict);
                }
            }
            var debugStates = roots?.DistinctBy(state => new { state.ID, state.StateType, state.Children }).ToList();

            return debugStates;
        }

        static void AddChildren(IDebugState node, IDictionary<Guid, List<IDebugState>> source)
        {
            var IsActualTypeContains = (node.ActualType?.Contains("DsfForEachActivity") ?? false);

            if (source.ContainsKey(node.ID)
                && (!node.IsAdded || IsActualTypeContains)
                && node.StateType != StateType.Duration)//Services have the same Id so, they dont work inside the foreach
            {
                List<IDebugState> debugStates;
                if (IsActualTypeContains)
                {
                    var states = source[node.ID].DistinctBy(state => new
                    {
                        state.DisconnectedID

                    }).ToList();
                    debugStates = states;
                }
                else
                {
                    debugStates = source[node.ID]?
                                            .Where(state => state.ID != node.ID)
                                            .ToList();
                }

                AddEachChild(node, source, debugStates);
            }
            else
            {
                node.Children = new List<IDebugState>();
            }
        }

        private static void AddEachChild(IDebugState node, IDictionary<Guid, List<IDebugState>> source, List<IDebugState> debugStates)
        {
            node.Children = debugStates ?? new List<IDebugState>();
            node.IsAdded = true;
            foreach (var state in node.Children)
            {
                AddChildren(state, source);
            }
        }
    }
}

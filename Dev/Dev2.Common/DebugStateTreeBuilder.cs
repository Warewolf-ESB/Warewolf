#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
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
            if (source.ContainsKey(node.ID)
                && (!node.IsAdded || (node.ActualType?.Contains("DsfForEachActivity") ?? false))
                && node.StateType != StateType.Duration)//Services have the same Id so, they dont work inside the foreach
            {
                List<IDebugState> debugStates;
                if (node.ActualType?.Contains("DsfForEachActivity") ?? false)
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

                node.Children = debugStates ?? new List<IDebugState>();
                node.IsAdded = true;
                foreach (var state in node.Children)
                {
                    AddChildren(state, source);
                }
            }
            else
            {
                node.Children = new List<IDebugState>();
            }
        }
    }
}

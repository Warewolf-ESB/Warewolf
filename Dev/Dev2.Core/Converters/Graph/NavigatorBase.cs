#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using Dev2.Common.Interfaces.Core.Graph;
using Unlimited.Framework.Converters.Graph;

namespace Dev2.Converters.Graph
{
    [Serializable]
    public abstract class NavigatorBase
    {
        #region Properties

        public object Data { get; internal set; }

        #endregion Properties

        protected void BuildResultsStructure(IList<IPath> paths, Dictionary<IPath, IList<object>> results)
        {
            foreach (IPath path in paths)
            {
                results.Add(path, new List<object>());
            }
        }

        protected void IndexPathSegments(IList<IPath> paths, Dictionary<IPath, List<IPathSegment>> indexedPathSegments)
        {
            indexedPathSegments.Clear();

            foreach (IPath path in paths)
            {
                indexedPathSegments.Add(path, new List<IPathSegment>(path.GetSegements()));
            }
        }

        protected virtual void BuildIndexedTree(IList<IPath> paths,
            Dictionary<IPath, List<IPathSegment>> indexedPathSegments,
            IndexedPathSegmentTreeNode<string> rootIndexedValueTreeNode)
        {
            foreach (IPath path in paths)
            {
                var IndexedPathSegmentTreeNode = rootIndexedValueTreeNode;
                var pathSegmentCount = 0;

                while (pathSegmentCount < indexedPathSegments[path].Count)
                {
                    var pathSegment = indexedPathSegments[path][pathSegmentCount];
                    if (
                        !IndexedPathSegmentTreeNode.TryGetValue(pathSegment.ActualSegment,
                            out IndexedPathSegmentTreeNode<string> tmpIndexedPathSegmentTreeNode))
                    {
                        var newIndexedPathSegmentTreeNode =
                            CreatePathSegmentIndexedPathSegmentTreeNode(pathSegment, IndexedPathSegmentTreeNode);
                        IndexedPathSegmentTreeNode.Add(pathSegment.ActualSegment, newIndexedPathSegmentTreeNode);
                        IndexedPathSegmentTreeNode = newIndexedPathSegmentTreeNode;
                    }
                    else
                    {
                        IndexedPathSegmentTreeNode = tmpIndexedPathSegmentTreeNode;
                    }

                    pathSegmentCount++;
                }
            }
        }

        protected virtual IndexedPathSegmentTreeNode<string> CreatePathSegmentIndexedPathSegmentTreeNode(
            IPathSegment pathSegment, IndexedPathSegmentTreeNode<string> parentNode) => null;

        protected void CreateRootNode(IList<IPath> validPaths, Dictionary<IPath, IList<object>> results)
        {
            //
            // Create the root node
            //
            var rootIndexedValueTreeNode = new IndexedPathSegmentTreeNode<string>();
            rootIndexedValueTreeNode.CurrentValue = Data;

            //
            // Index the segments of all the paths, this is done so that they don't have to be
            // regenerated for every use.
            //
            var indexedPathSegments = new Dictionary<IPath, List<IPathSegment>>();
            IndexPathSegments(validPaths, indexedPathSegments);

            do
            {
                BuildIndexedTree(validPaths, indexedPathSegments, rootIndexedValueTreeNode);
                WriteToResults(validPaths, indexedPathSegments, rootIndexedValueTreeNode, results);
            } while (EnumerateIndexedTree(rootIndexedValueTreeNode) > 0);
        }

        protected long EnumerateIndexedTree(IndexedPathSegmentTreeNode<string> node)
        {
            long enumerationCount = 0;

            foreach (var childNode in node.Values)
            {
                enumerationCount += EnumerateIndexedTree(childNode);
            }

            if (node.Enumerator != null && enumerationCount == 0)
            {
                node.EnumerationComplete = !node.Enumerator.MoveNext();
                if (node.EnumerationComplete)
                {
                    node.CurrentValue = string.Empty;
                }
                else
                {
                    node.CurrentValue = node.Enumerator.Current;
                    enumerationCount++;
                }

                node.Clear();
            }

            return enumerationCount;
        }

        protected virtual void WriteToResults(IList<IPath> paths,
            Dictionary<IPath, List<IPathSegment>> indexedPathSegments,
            IndexedPathSegmentTreeNode<string> rootIndexedValueTreeNode, Dictionary<IPath, IList<object>> results)
        {
            foreach (IPath path in paths)
            {
                var IndexedPathSegmentTreeNode =
                    rootIndexedValueTreeNode[indexedPathSegments[path].Select(p => p.ActualSegment).ToList()];
                results[path].Add(IndexedPathSegmentTreeNode.CurrentValue);
            }
        }
    }
}
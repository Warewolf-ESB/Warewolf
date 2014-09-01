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
            foreach(IPath path in paths)
            {
                results.Add(path, new List<object>());
            }
        }

        protected void IndexPathSegments(IList<IPath> paths, Dictionary<IPath, List<IPathSegment>> indexedPathSegments)
        {
            indexedPathSegments.Clear();

            foreach(IPath path in paths)
            {
                indexedPathSegments.Add(path, new List<IPathSegment>(path.GetSegements()));
            }
        }

        protected virtual void BuildIndexedTree(IList<IPath> paths, Dictionary<IPath, List<IPathSegment>> indexedPathSegments, IndexedPathSegmentTreeNode<string> rootIndexedValueTreeNode)
        {
            foreach(IPath path in paths)
            {
                IndexedPathSegmentTreeNode<string> IndexedPathSegmentTreeNode = rootIndexedValueTreeNode;
                int pathSegmentCount = 0;

                while(pathSegmentCount < indexedPathSegments[path].Count)
                {
                    IndexedPathSegmentTreeNode<string> tmpIndexedPathSegmentTreeNode;
                    IPathSegment pathSegment = indexedPathSegments[path][pathSegmentCount];
                    if(!IndexedPathSegmentTreeNode.TryGetValue(pathSegment.ActualSegment, out tmpIndexedPathSegmentTreeNode))
                    {
                        IndexedPathSegmentTreeNode<string> newIndexedPathSegmentTreeNode = CreatePathSegmentIndexedPathSegmentTreeNode(pathSegment, IndexedPathSegmentTreeNode);
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

        protected virtual IndexedPathSegmentTreeNode<string> CreatePathSegmentIndexedPathSegmentTreeNode(IPathSegment pathSegment, IndexedPathSegmentTreeNode<string> parentNode)
        {
            return null;
        }

        protected void CreateRootNode(IList<IPath> validPaths, Dictionary<IPath, IList<object>> results)
        {
            //
            // Create the root node
            //
            IndexedPathSegmentTreeNode<string> rootIndexedValueTreeNode = new IndexedPathSegmentTreeNode<string>();
            rootIndexedValueTreeNode.CurrentValue = Data;

            //
            // Index the segments of all the paths, this is done so that they don't have to be
            // regenerated for every use.
            //
            Dictionary<IPath, List<IPathSegment>> indexedPathSegments = new Dictionary<IPath, List<IPathSegment>>();
            IndexPathSegments(validPaths, indexedPathSegments);

            do
            {
                BuildIndexedTree(validPaths, indexedPathSegments, rootIndexedValueTreeNode);
                WriteToResults(validPaths, indexedPathSegments, rootIndexedValueTreeNode, results);
            }
            while(EnumerateIndexedTree(rootIndexedValueTreeNode) > 0);
        }

        protected long EnumerateIndexedTree(IndexedPathSegmentTreeNode<string> node)
        {
            long enumerationCount = 0;

            foreach(IndexedPathSegmentTreeNode<string> childNode in node.Values)
            {
                enumerationCount += EnumerateIndexedTree(childNode);
            }

            if(node.Enumerator != null && enumerationCount == 0)
            {
                node.EnumerationComplete = !node.Enumerator.MoveNext();
                if(node.EnumerationComplete)
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

        protected virtual void WriteToResults(IList<IPath> paths, Dictionary<IPath, List<IPathSegment>> indexedPathSegments, IndexedPathSegmentTreeNode<string> rootIndexedValueTreeNode, Dictionary<IPath, IList<object>> results)
        {
            foreach(IPath path in paths)
            {
                IndexedPathSegmentTreeNode<string> IndexedPathSegmentTreeNode = rootIndexedValueTreeNode[indexedPathSegments[path].Select(p => p.ActualSegment).ToList()];
                results[path].Add(IndexedPathSegmentTreeNode.CurrentValue);
            }
        }
    }
}

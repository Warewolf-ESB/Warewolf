#pragma warning disable
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dev2;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Converters.Graph;
using Warewolf.Resource.Errors;

namespace Unlimited.Framework.Converters.Graph.Poco
{
    [Serializable]
    public class PocoNavigator : NavigatorBase, INavigator
    {
        #region Constructor

        public PocoNavigator(object data)
        {
            Data = data;
        }

        #endregion Constructor

        #region Methods

        public object SelectScalar(IPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            var pocoPath = path as PocoPath;

            if (pocoPath == null)
            {
                throw new Exception(string.Format(ErrorResource.PathMismatch,
                    typeof (PocoPath), path.GetType()));
            }

            var currentData = Data;

            if (path.ActualPath == PocoPath.SeperatorSymbol)
            {
                currentData = Data.ToString();
            }
            else if (path.ActualPath == PocoPath.EnumerableSymbol + PocoPath.SeperatorSymbol)
            {
                var enumerableData = currentData as IEnumerable;

                if (enumerableData == null)
                {
                    currentData = null;
                }
                else
                {
                    var enumerator = enumerableData.GetEnumerator();
                    enumerator.Reset();
                    while (enumerator.MoveNext())
                    {
                        currentData = enumerator.Current;
                    }
                }
            }
            else
            {
                var pathSegments = pocoPath.GetSegements().ToList();
                var segmentIndex = 0;

                while (currentData != null && segmentIndex < pathSegments.Count)
                {
                    currentData = pathSegments[segmentIndex].IsEnumarable ? GetScalarValueForEnumarablePathSegment(pathSegments, currentData, segmentIndex) : GetScalarValueForPathSegement(pathSegments[segmentIndex], currentData);

                    segmentIndex++;
                }
            }

            return currentData;
        }

        private object GetScalarValueForEnumarablePathSegment(List<IPathSegment> pathSegments, object currentData, int segmentIndex)
        {
            var enumerableData = GetEnumerableValueForPathSegment(pathSegments[segmentIndex],
                                        currentData);

            if (enumerableData == null)
            {
                currentData = null;
            }
            else
            {
                var enumerator = enumerableData.GetEnumerator();
                enumerator.Reset();
                while (enumerator.MoveNext())
                {
                    currentData = enumerator.Current;
                }
            }

            return currentData;
        }

        public IEnumerable<object> SelectEnumerable(IPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            var pocoPath = path as PocoPath;

            if (pocoPath == null)
            {
                throw new Exception(string.Format(ErrorResource.PathMismatch,
                    typeof (PocoPath), path.GetType()));
            }

            List<object> returnData;

            if (path.ActualPath == PocoPath.SeperatorSymbol)
            {
                returnData = new List<object> {Data};
            }
            else if (path.ActualPath == PocoPath.EnumerableSymbol + PocoPath.SeperatorSymbol)
            {
                var enumerableData = Data as IEnumerable;
                returnData = new List<object>();

                if (enumerableData != null)
                {
                    var enumerator = enumerableData.GetEnumerator();
                    enumerator.Reset();
                    while (enumerator.MoveNext())
                    {
                        returnData.Add(enumerator.Current);
                    }
                }
            }
            else
            {
                returnData = SelectEnumarable(pocoPath.GetSegements().ToList(), Data).ToList();
            }

            return returnData;
        }

        public Dictionary<IPath, IList<object>> SelectEnumerablesAsRelated(IList<IPath> paths)
        {
            //
            // Get valid paths
            //
            IList<IPath> validPaths = new List<IPath>(paths.OfType<PocoPath>().ToList());

            //
            // Setup results structure
            //
            var results = new Dictionary<IPath, IList<object>>();
            BuildResultsStructure(validPaths, results);

            if (validPaths.Count == 1 && validPaths[0].ActualPath == PocoPath.SeperatorSymbol)
            {
                results[validPaths[0]].Add(Data);
            }
            else if (validPaths.Count == 1 &&
                     validPaths[0].ActualPath == PocoPath.EnumerableSymbol + PocoPath.SeperatorSymbol)
            {

                if (Data is IEnumerable enumerableData)
                {
                    var enumerator = enumerableData.GetEnumerator();
                    enumerator.Reset();
                    while (enumerator.MoveNext())
                    {
                        results[validPaths[0]].Add(enumerator.Current);
                    }
                }
            }
            else
            {
                //
                // Create the root node
                //
                var rootIndexedValueTreeNode = new IndexedPathSegmentTreeNode<string> {CurrentValue = Data};

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
            return results;
        }

        public void Dispose()
        {
            Data = null;
        }

        #endregion Methods

        #region Private Methods

        IEnumerable<object> SelectEnumarable(IList<IPathSegment> pathSegments, object data)
        {
            var returnData = new List<object>();
            var currentData = data;

            for (int i = 0; i < pathSegments.Count; i++)
            {
                var pathSegment = pathSegments[i];
                var lastSegment = i == pathSegments.Count - 1;

                if (pathSegment.IsEnumarable)
                {
                    return SelectEnumarable(pathSegments, currentData, returnData, i, pathSegment);
                }

                currentData = GetScalarValueForPathSegement(pathSegment, currentData);

                if (lastSegment)
                {
                    returnData.Add(currentData);
                }
            }

            return returnData;
        }

        List<object> SelectEnumarable(IList<IPathSegment> pathSegments, object data, List<object> returnData, int i, IPathSegment pathSegment)
        {
            var enumerableData = GetEnumerableValueForPathSegment(pathSegment, data);

            if (enumerableData != null)
            {
                var enumerator = enumerableData.GetEnumerator();
                enumerator.Reset();

                while (enumerator.MoveNext())
                {
                    returnData.AddRange(SelectEnumarable(pathSegments.Skip(i + 1).ToList(), enumerator.Current));
                }
            }
            return returnData;
        }

        protected override IndexedPathSegmentTreeNode<string> CreatePathSegmentIndexedPathSegmentTreeNode(
            IPathSegment pathSegment, IndexedPathSegmentTreeNode<string> parentNode)
        {
            var newIndexedValueTreeNode = new IndexedPathSegmentTreeNode<string>();

            if (parentNode.EnumerationComplete)
            {
                newIndexedValueTreeNode.CurrentValue = string.Empty;
                newIndexedValueTreeNode.EnumerationComplete = true;
            }
            else
            {
                if (pathSegment.IsEnumarable)
                {
                    newIndexedValueTreeNode = IndexedEnumarablePathSegmentTreeNode(newIndexedValueTreeNode, pathSegment, parentNode);
                }
                else
                {
                    newIndexedValueTreeNode.CurrentValue = GetScalarValueForPathSegement(pathSegment,
                        parentNode.CurrentValue);

                    if (newIndexedValueTreeNode.CurrentValue == null)
                    {
                        newIndexedValueTreeNode.CurrentValue = string.Empty;
                        newIndexedValueTreeNode.EnumerationComplete = true;
                    }
                }
            }

            return newIndexedValueTreeNode;
        }

        IndexedPathSegmentTreeNode<string> IndexedEnumarablePathSegmentTreeNode(IndexedPathSegmentTreeNode<string> newIndexedValueTreeNode, IPathSegment pathSegment, IndexedPathSegmentTreeNode<string> parentNode)
        {
            newIndexedValueTreeNode.EnumerableValue = GetEnumerableValueForPathSegment(pathSegment,
                                    parentNode.CurrentValue);

            if (newIndexedValueTreeNode.EnumerableValue == null)
            {
                newIndexedValueTreeNode.CurrentValue = string.Empty;
                newIndexedValueTreeNode.EnumerationComplete = true;
            }
            else
            {
                newIndexedValueTreeNode.Enumerator = newIndexedValueTreeNode.EnumerableValue.GetEnumerator();

                newIndexedValueTreeNode.Enumerator.Reset();

                if (!newIndexedValueTreeNode.Enumerator.MoveNext())
                {
                    newIndexedValueTreeNode.CurrentValue = string.Empty;
                    newIndexedValueTreeNode.EnumerationComplete = true;
                }
                else
                {
                    newIndexedValueTreeNode.CurrentValue = newIndexedValueTreeNode.Enumerator.Current;
                }
            }
            return newIndexedValueTreeNode;
        }

        object GetScalarValueForPathSegement(IPathSegment pathSegment, object data)
        {
            var propertyInfo = data.GetType().GetProperty(pathSegment.ActualSegment);

            object returnVal = null;
            if (propertyInfo != null)
            {
                returnVal = propertyInfo.GetValue(data, null);
            }

            return returnVal;
        }

        IEnumerable GetEnumerableValueForPathSegment(IPathSegment pathSegment, object data)
        {
            var propertyInfo = data.GetType().GetProperty(pathSegment.ActualSegment);
            IEnumerable returnVal = null;

            if (propertyInfo == null)
            {
                returnVal = data as IEnumerable;
            }
            if (propertyInfo != null && propertyInfo.PropertyType.IsEnumerable())
            {
                returnVal = propertyInfo.GetValue(data, null) as IEnumerable;
            }

            return returnVal;
        }

        #endregion Private Methods
    }
}
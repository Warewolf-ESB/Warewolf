/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
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
                throw new Exception(string.Format("Path of type '{0}' expected, path of type '{1}' received.",
                    typeof (PocoPath), path.GetType()));
            }

            object currentData = Data;

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
                    IEnumerator enumerator = enumerableData.GetEnumerator();
                    enumerator.Reset();
                    while (enumerator.MoveNext())
                    {
                        currentData = enumerator.Current;
                    }
                }
            }
            else
            {
                List<IPathSegment> pathSegments = pocoPath.GetSegements().ToList();
                int segmentIndex = 0;

                while (currentData != null && segmentIndex < pathSegments.Count)
                {
                    if (pathSegments[segmentIndex].IsEnumarable)
                    {
                        IEnumerable enumerableData = GetEnumerableValueForPathSegment(pathSegments[segmentIndex],
                            currentData);

                        if (enumerableData == null)
                        {
                            currentData = null;
                        }
                        else
                        {
                            IEnumerator enumerator = enumerableData.GetEnumerator();
                            enumerator.Reset();
                            while (enumerator.MoveNext())
                            {
                                currentData = enumerator.Current;
                            }
                        }
                    }
                    else
                    {
                        currentData = GetScalarValueForPathSegement(pathSegments[segmentIndex], currentData);
                    }

                    segmentIndex++;
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
                throw new Exception(string.Format("Path of type '{0}' expected, path of type '{1}' received.",
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
                    IEnumerator enumerator = enumerableData.GetEnumerator();
                    enumerator.Reset();
                    while (enumerator.MoveNext())
                    {
                        returnData.Add(enumerator.Current);
                    }
                }
            }
            else
            {
                returnData = SelectEnumberable(pocoPath.GetSegements().ToList(), Data).ToList();
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
                var enumerableData = Data as IEnumerable;

                if (enumerableData != null)
                {
                    IEnumerator enumerator = enumerableData.GetEnumerator();
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

        private IEnumerable<object> SelectEnumberable(IList<IPathSegment> pathSegments, object data)
        {
            var returnData = new List<object>();
            object currentData = data;

            for (int i = 0; i < pathSegments.Count; i++)
            {
                IPathSegment pathSegment = pathSegments[i];
                bool lastSegment = (i == pathSegments.Count - 1);

                if (pathSegment.IsEnumarable)
                {
                    IEnumerable enumerableData = GetEnumerableValueForPathSegment(pathSegment, currentData);

                    if (enumerableData != null)
                    {
                        IEnumerator enumerator = enumerableData.GetEnumerator();
                        enumerator.Reset();

                        while (enumerator.MoveNext())
                        {
                            returnData.AddRange(SelectEnumberable(pathSegments.Skip(i + 1).ToList(), enumerator.Current));
                        }
                    }
                    else
                    {
                        // ReSharper disable RedundantAssignment
                        currentData = null;
                        // ReSharper restore RedundantAssignment
                    }

                    return returnData;
                }

                currentData = GetScalarValueForPathSegement(pathSegment, currentData);

                if (lastSegment)
                {
                    returnData.Add(currentData);
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

        private object GetScalarValueForPathSegement(IPathSegment pathSegment, object data)
        {
            PropertyInfo propertyInfo = data.GetType().GetProperty(pathSegment.ActualSegment);

            object returnVal = null;
            if (propertyInfo != null)
            {
                returnVal = propertyInfo.GetValue(data, null);
            }

            return returnVal;
        }

        private IEnumerable GetEnumerableValueForPathSegment(IPathSegment pathSegment, object data)
        {
            PropertyInfo propertyInfo = data.GetType().GetProperty(pathSegment.ActualSegment);

            IEnumerable returnVal = null;
            if (propertyInfo != null && propertyInfo.PropertyType.IsEnumerable())
            {
                returnVal = propertyInfo.GetValue(data, null) as IEnumerable;
            }

            return returnVal;
        }

        #endregion Private Methods
    }
}
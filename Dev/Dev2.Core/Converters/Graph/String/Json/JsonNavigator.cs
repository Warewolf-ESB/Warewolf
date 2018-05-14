/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Text;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Converters.Graph;
using Newtonsoft.Json.Linq;
using Warewolf.Resource.Errors;

namespace Unlimited.Framework.Converters.Graph.String.Json
{
    [Serializable]
    public class JsonNavigator : NavigatorBase, INavigator
    {
        public JsonNavigator(object data) => Data = JToken.Parse(data.ToString());

        #region Methods

        public object SelectScalar(IPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            var jsonPath = path as JsonPath;

            if (jsonPath == null)
            {
                throw new Exception(string.Format(ErrorResource.PathMismatch,
                    typeof (JsonPath), path.GetType()));
            }

            var currentData = Data as JToken;

            if (path.ActualPath == JsonPath.SeperatorSymbol)
            {
                //nothing to do here yet
            }
            else if (path.ActualPath == JsonPath.EnumerableSymbol + JsonPath.SeperatorSymbol)
            {
                var enumerableData = currentData as IEnumerable;
                var enumerator = enumerableData.GetEnumerator();
                enumerator.Reset();
                while (enumerator.MoveNext())
                {
                    currentData = enumerator.Current as JToken;
                }
            }
            else
            {
                var pathSegments = jsonPath.GetSegements().ToList();
                var segmentIndex = 0;

                while (currentData != null && segmentIndex < pathSegments.Count)
                {
                    if (pathSegments[segmentIndex].IsEnumarable)
                    {
                        currentData = GetEnumuratedValueForPathSegment(currentData, pathSegments, segmentIndex);
                    }
                    else
                    {
                        currentData = GetScalarValueForPathSegment(pathSegments[segmentIndex], currentData);
                    }

                    segmentIndex++;
                }
            }

            var returnVal = "";

            if (currentData != null)
            {
                returnVal = currentData.ToString();
            }

            return returnVal;
        }

        private JToken GetEnumuratedValueForPathSegment(JToken currentData, List<IPathSegment> pathSegments, int segmentIndex)
        {
            var enumerableData = GetEnumerableValueForPathSegment(pathSegments[segmentIndex],
                                        currentData);

            if (enumerableData != null)
            {
                var enumerator = enumerableData.GetEnumerator();
                enumerator.Reset();
                while (enumerator.MoveNext())
                {
                    currentData = enumerator.Current as JToken;
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

            var jsonPath = path as JsonPath;

            if (jsonPath == null)
            {
                throw new Exception(string.Format(ErrorResource.DataTypeMismatch,
                    typeof (JsonPath), path.GetType()));
            }

            List<object> returnData;

            if (path.ActualPath == JsonPath.SeperatorSymbol)
            {
                returnData = new List<object> {Data};
            }
            else if (path.ActualPath == JsonPath.EnumerableSymbol + JsonPath.SeperatorSymbol)
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
                returnData =
                    new List<object>(
                        SelectEnumberable(jsonPath.GetSegements().ToList(), Data as JToken).Select(o => o.ToString()));
            }

            return returnData;
        }

        public Dictionary<IPath, IList<object>> SelectEnumerablesAsRelated(IList<IPath> paths)
        {
            //
            // Get valid paths
            //
            IList<IPath> validPaths = new List<IPath>(paths.OfType<JsonPath>().ToList());

            //
            // Setup results structure
            //
            var results = new Dictionary<IPath, IList<object>>();
            BuildResultsStructure(validPaths, results);

            if (validPaths.Count == 1 && validPaths[0].ActualPath == JsonPath.SeperatorSymbol)
            {
                results[validPaths[0]].Add(Data);
            }
            else if (validPaths.Count == 1 &&
                     validPaths[0].ActualPath == JsonPath.EnumerableSymbol + JsonPath.SeperatorSymbol)
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
                CreateRootNode(validPaths, results);
            }
            return results;
        }

        public void Dispose()
        {
            Data = null;
        }

        #endregion Methods

        #region Private Methods

        IEnumerable<object> SelectEnumberable(IList<IPathSegment> pathSegments, JToken data)
        {
            var returnData = new List<object>();
            var currentData = data;

            for (int i = 0; i < pathSegments.Count; i++)
            {
                var pathSegment = pathSegments[i];
                var lastSegment = i == pathSegments.Count - 1;

                if (pathSegment.IsEnumarable)
                {
                    var enumerableData = GetEnumerableValueForPathSegment(pathSegment, currentData);

                    if (enumerableData != null)
                    {
                        GetEnumerableValueForSegment(pathSegments, returnData, currentData, i, enumerableData);
                    }

                    return returnData;
                }

                currentData = GetScalarValueForPathSegment(pathSegment, currentData);

                if (currentData != null && lastSegment)
                {
                    returnData.Add(currentData.ToString());
                }
            }

            return returnData;
        }

        void GetEnumerableValueForSegment(IList<IPathSegment> pathSegments, List<object> returnData, JToken currentData, int i, IEnumerable enumerableData)
        {
            var enumerator = enumerableData.GetEnumerator();
            enumerator.Reset();

            var testToken = enumerableData as JToken;

            if (testToken.IsEnumerableOfPrimitives())
            {
                while (enumerator.MoveNext())
                {
                    var currentToken = enumerator.Current as JToken;
                    if (currentData != null && currentToken != null)
                    {
                        returnData.Add(currentToken.ToString());
                    }

                }
            }
            else
            {
                while (enumerator.MoveNext())
                {
                    returnData.AddRange(SelectEnumberable(pathSegments.Skip(i + 1).ToList(),
                        enumerator.Current as JToken));
                }
            }
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
                    GetEnumerableValueForPathSegment(pathSegment, parentNode, newIndexedValueTreeNode);
                }
                else
                {
                    newIndexedValueTreeNode.CurrentValue = GetScalarValueForPathSegment(pathSegment,
                        parentNode.CurrentValue as JToken);

                    if (newIndexedValueTreeNode.CurrentValue == null)
                    {
                        newIndexedValueTreeNode.CurrentValue = string.Empty;
                        newIndexedValueTreeNode.EnumerationComplete = true;
                    }
                }
            }

            return newIndexedValueTreeNode;
        }

        private void GetEnumerableValueForPathSegment(IPathSegment pathSegment, IndexedPathSegmentTreeNode<string> parentNode, IndexedPathSegmentTreeNode<string> newIndexedValueTreeNode)
        {
            var data = parentNode.CurrentValue as JToken;
            newIndexedValueTreeNode.EnumerableValue = GetEnumerableValueForPathSegment(pathSegment, data);

            if (newIndexedValueTreeNode.EnumerableValue == null)
            {
                newIndexedValueTreeNode.CurrentValue = string.Empty;
                newIndexedValueTreeNode.EnumerationComplete = true;
            }
            else
            {
                var isPrimitiveArray = false;
                if (data is JObject jObject)
                {
                    var property = jObject.Property(pathSegment.ActualSegment);
                    isPrimitiveArray = property.IsEnumerableOfPrimitives();
                }

                newIndexedValueTreeNode.Enumerator = newIndexedValueTreeNode.EnumerableValue.GetEnumerator();
                newIndexedValueTreeNode.Enumerator.Reset();

                if (isPrimitiveArray)
                {
                    var valueBuilder = new StringBuilder();
                    while (newIndexedValueTreeNode.Enumerator.MoveNext())
                    {
                        valueBuilder.Append(newIndexedValueTreeNode.Enumerator.Current);
                        valueBuilder.Append(",");
                    }
                    newIndexedValueTreeNode.EnumerationComplete = true;
                    newIndexedValueTreeNode.CurrentValue = valueBuilder.ToString().TrimEnd(',');
                }
                else
                {
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
        }

        JToken GetScalarValueForPathSegment(IPathSegment pathSegment, IEnumerable<JToken> data)
        {
            var jObject = data as JObject;

            JToken returnVal = null;
            var property = jObject?.Property(pathSegment.ActualSegment);

            if (property != null)
            {
                returnVal = property.Value;
            }

            return returnVal;
        }

        IEnumerable GetEnumerableValueForPathSegment(IPathSegment pathSegment, IEnumerable<JToken> data)
        {
            var jObject = data as JObject;

            IEnumerable returnVal = null;
            var property = jObject?.Property(pathSegment.ActualSegment);

            if (property != null && property.IsEnumerable())
            {
                returnVal = property.Value as JArray;
            }

            if (data is JArray jArray)
            {
                returnVal = jArray;
            }

            return returnVal;
        }

        protected override void WriteToResults(IList<IPath> paths,
            Dictionary<IPath, List<IPathSegment>> indexedPathSegments,
            IndexedPathSegmentTreeNode<string> rootIndexedValueTreeNode, Dictionary<IPath, IList<object>> results)
        {
            foreach (IPath path in paths)
            {
                var indexedPathSegment = indexedPathSegments[path];
                var complexKey = indexedPathSegment.Select(p => p.ActualSegment).ToList();
                var IndexedPathSegmentTreeNode = rootIndexedValueTreeNode[complexKey];
                results[path].Add(IndexedPathSegmentTreeNode.CurrentValue.ToString());
            }
        }

        #endregion Private Methods
    }
}
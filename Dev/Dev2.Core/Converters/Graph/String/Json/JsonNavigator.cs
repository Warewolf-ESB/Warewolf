
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Converters.Graph;
using Newtonsoft.Json.Linq;

namespace Unlimited.Framework.Converters.Graph.String.Json
{
    [Serializable]
    public class JsonNavigator : NavigatorBase, INavigator
    {
        #region Constructor

        public JsonNavigator(object data)
        {
            Data = JToken.Parse(data.ToString());
        }

        #endregion Constructor

        #region Methods

        public object SelectScalar(IPath path)
        {
            if(path == null)
            {
                throw new ArgumentNullException("path");
            }

            JsonPath jsonPath = path as JsonPath;

            if(jsonPath == null)
            {
                throw new Exception(string.Format("Path of type '{0}' expected, path of type '{1}' received.", typeof(JsonPath), path.GetType()));
            }

            JToken currentData = Data as JToken;

            if(currentData == null)
            {
                throw new Exception(string.Format("Type of {0} was expected for data, type of {1} was found instead.", typeof(JToken), Data.GetType()));
            }

            if(path.ActualPath == JsonPath.SeperatorSymbol)
            {
                //nothing to do here yet
            }
            else if(path.ActualPath == JsonPath.EnumerableSymbol + JsonPath.SeperatorSymbol)
            {
                var enumerableData = currentData as IEnumerable;


                IEnumerator enumerator = enumerableData.GetEnumerator();
                enumerator.Reset();
                while(enumerator.MoveNext())
                {
                    currentData = enumerator.Current as JToken;
                }

            }
            else
            {
                List<IPathSegment> pathSegments = jsonPath.GetSegements().ToList();
                int segmentIndex = 0;

                while(currentData != null && segmentIndex < pathSegments.Count)
                {
                    if(pathSegments[segmentIndex].IsEnumarable)
                    {
                        IEnumerable enumerableData = GetEnumerableValueForPathSegment(pathSegments[segmentIndex], currentData);

                        if(enumerableData == null)
                        {
                            currentData = null;
                        }
                        else
                        {
                            IEnumerator enumerator = enumerableData.GetEnumerator();
                            enumerator.Reset();
                            while(enumerator.MoveNext())
                            {
                                currentData = enumerator.Current as JToken;
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

            string returnVal = "";

            if(currentData != null)
            {
                returnVal = currentData.ToString();
            }

            return returnVal;
        }

        public IEnumerable<object> SelectEnumerable(IPath path)
        {
            if(path == null)
            {
                throw new ArgumentNullException("path");
            }

            JsonPath jsonPath = path as JsonPath;

            if(jsonPath == null)
            {
                throw new Exception(string.Format("Path of type '{0}' expected, path of type '{1}' received.", typeof(JsonPath), path.GetType()));
            }

            List<object> returnData;

            if(path.ActualPath == JsonPath.SeperatorSymbol)
            {
                returnData = new List<object> { Data };
            }
            else if(path.ActualPath == JsonPath.EnumerableSymbol + JsonPath.SeperatorSymbol)
            {
                IEnumerable enumerableData = Data as IEnumerable;
                returnData = new List<object>();

                if(enumerableData != null)
                {
                    IEnumerator enumerator = enumerableData.GetEnumerator();
                    enumerator.Reset();
                    while(enumerator.MoveNext())
                    {
                        returnData.Add(enumerator.Current);
                    }
                }
            }
            else
            {
                returnData = new List<object>(SelectEnumberable(jsonPath.GetSegements().ToList(), Data as JToken).Select(o => o.ToString()));
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
            Dictionary<IPath, IList<object>> results = new Dictionary<IPath, IList<object>>();
            BuildResultsStructure(validPaths, results);

            if(validPaths.Count == 1 && validPaths[0].ActualPath == JsonPath.SeperatorSymbol)
            {
                results[validPaths[0]].Add(Data);
            }
            else if(validPaths.Count == 1 && validPaths[0].ActualPath == JsonPath.EnumerableSymbol + JsonPath.SeperatorSymbol)
            {
                IEnumerable enumerableData = Data as IEnumerable;

                if(enumerableData != null)
                {
                    IEnumerator enumerator = enumerableData.GetEnumerator();
                    enumerator.Reset();
                    while(enumerator.MoveNext())
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

        private IEnumerable<object> SelectEnumberable(IList<IPathSegment> pathSegments, JToken data)
        {
            List<object> returnData = new List<object>();
            JToken currentData = data;

            for(int i = 0; i < pathSegments.Count; i++)
            {
                IPathSegment pathSegment = pathSegments[i];
                bool lastSegment = (i == pathSegments.Count - 1);

                if(pathSegment.IsEnumarable)
                {
                    IEnumerable enumerableData = GetEnumerableValueForPathSegment(pathSegment, currentData);

                    if(enumerableData != null)
                    {
                        IEnumerator enumerator = enumerableData.GetEnumerator();
                        enumerator.Reset();

                        JToken testToken = enumerableData as JToken;

                        if(testToken.IsEnumerableOfPrimitives())
                        {
                            while(enumerator.MoveNext())
                            {
                                JToken currentToken = enumerator.Current as JToken;
                                if(currentData != null)
                                {
                                    if(currentToken != null)
                                    {
                                        returnData.Add(currentToken.ToString());
                                    }
                                }
                            }
                        }
                        else
                        {
                            while(enumerator.MoveNext())
                            {
                                returnData.AddRange(SelectEnumberable(pathSegments.Skip(i + 1).ToList(), enumerator.Current as JToken));
                            }
                        }
                    }

                    return returnData;
                }

                currentData = GetScalarValueForPathSegement(pathSegment, currentData);

                if(currentData != null && lastSegment)
                {
                    returnData.Add(currentData.ToString());
                }
            }

            return returnData;
        }

        protected override IndexedPathSegmentTreeNode<string> CreatePathSegmentIndexedPathSegmentTreeNode(IPathSegment pathSegment, IndexedPathSegmentTreeNode<string> parentNode)
        {
            IndexedPathSegmentTreeNode<string> newIndexedValueTreeNode = new IndexedPathSegmentTreeNode<string>();

            if(parentNode.EnumerationComplete)
            {
                newIndexedValueTreeNode.CurrentValue = string.Empty;
                newIndexedValueTreeNode.EnumerationComplete = true;
            }
            else
            {
                if(pathSegment.IsEnumarable)
                {
                    var data = parentNode.CurrentValue as JToken;
                    newIndexedValueTreeNode.EnumerableValue = GetEnumerableValueForPathSegment(pathSegment, data);

                    if(newIndexedValueTreeNode.EnumerableValue == null)
                    {
                        newIndexedValueTreeNode.CurrentValue = string.Empty;
                        newIndexedValueTreeNode.EnumerationComplete = true;
                    }
                    else
                    {
                        bool isPrimitiveArray = false;
                        JObject jObject = data as JObject;
                        if(jObject != null)
                        {
                            JProperty property = jObject.Property(pathSegment.ActualSegment);
                            isPrimitiveArray = property.IsEnumerableOfPrimitives();
                        }

                        newIndexedValueTreeNode.Enumerator = newIndexedValueTreeNode.EnumerableValue.GetEnumerator();
                        newIndexedValueTreeNode.Enumerator.Reset();

                        if(isPrimitiveArray)
                        {
                            var valueBuilder = new StringBuilder();
                            while(newIndexedValueTreeNode.Enumerator.MoveNext())
                            {
                                valueBuilder.Append(newIndexedValueTreeNode.Enumerator.Current);
                                valueBuilder.Append(",");
                            }
                            newIndexedValueTreeNode.EnumerationComplete = true;
                            newIndexedValueTreeNode.CurrentValue = valueBuilder.ToString().TrimEnd(',');
                        }
                        else
                        {

                            if(!newIndexedValueTreeNode.Enumerator.MoveNext())
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
                else
                {
                    newIndexedValueTreeNode.CurrentValue = GetScalarValueForPathSegement(pathSegment, parentNode.CurrentValue as JToken);

                    if(newIndexedValueTreeNode.CurrentValue == null)
                    {
                        newIndexedValueTreeNode.CurrentValue = string.Empty;
                        newIndexedValueTreeNode.EnumerationComplete = true;
                    }
                }
            }

            return newIndexedValueTreeNode;
        }

        private JToken GetScalarValueForPathSegement(IPathSegment pathSegment, IEnumerable<JToken> data)
        {
            JObject jObject = data as JObject;

            JToken returnVal = null;
            if(jObject != null)
            {
                JProperty property = jObject.Property(pathSegment.ActualSegment);

                if(property != null)
                {
                    returnVal = property.Value;
                }
            }

            return returnVal;
        }

        private IEnumerable GetEnumerableValueForPathSegment(IPathSegment pathSegment, IEnumerable<JToken> data)
        {
            JObject jObject = data as JObject;

            IEnumerable returnVal = null;
            if(jObject != null)
            {
                JProperty property = jObject.Property(pathSegment.ActualSegment);

                if(property != null && property.IsEnumerable())
                {
                    returnVal = property.Value as JArray;
                }
            }

            return returnVal;
        }

        protected override void WriteToResults(IList<IPath> paths, Dictionary<IPath, List<IPathSegment>> indexedPathSegments, IndexedPathSegmentTreeNode<string> rootIndexedValueTreeNode, Dictionary<IPath, IList<object>> results)
        {
            foreach(IPath path in paths)
            {
                List<IPathSegment> indexedPathSegment = indexedPathSegments[path];
                List<string> complexKey = indexedPathSegment.Select(p => p.ActualSegment).ToList();
                IndexedPathSegmentTreeNode<string> IndexedPathSegmentTreeNode = rootIndexedValueTreeNode[complexKey];
                results[path].Add(IndexedPathSegmentTreeNode.CurrentValue.ToString());
            }
        }

        #endregion Private Methods
    }
}

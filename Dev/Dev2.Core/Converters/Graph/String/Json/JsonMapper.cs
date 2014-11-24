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
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Graph;
using Newtonsoft.Json.Linq;

// ReSharper disable CheckNamespace

namespace Unlimited.Framework.Converters.Graph.String.Json
// ReSharper restore CheckNamespace
{
    [Serializable]
    public class JsonMapper : IMapper
    {
        #region Methods

        public IEnumerable<IPath> Map(object data)
        {
            JToken jToken = JToken.Parse(data.ToString());
            var propertyStack = new Stack<Tuple<JProperty, bool>>();

            return BuildPaths(jToken, propertyStack, jToken);
        }

        #endregion Methods

        #region Private Methods

        private IEnumerable<IPath> BuildPaths(JToken data, Stack<Tuple<JProperty, bool>> propertyStack, JToken root)
        {
            var paths = new List<IPath>();

            if (propertyStack.Count == 0 && data.IsEnumerable())
            {
                //
                // Handle raw array of values
                //
                paths.Add(new JsonPath(JsonPath.EnumerableSymbol + JsonPath.SeperatorSymbol,
                    JsonPath.EnumerableSymbol + JsonPath.SeperatorSymbol, data.ToString()));
            }

            if (propertyStack.Count == 0 && data.IsPrimitive())
            {
                //
                // Handle if the poco mapper is used to map to a raw primitive
                //
                paths.Add(new JsonPath(JsonPath.SeperatorSymbol, JsonPath.SeperatorSymbol, data.ToString()));
            }
            else if (data.IsObject())
            {
                var dataAsJObject = data as JObject;

                if (dataAsJObject == null)
                {
                    throw new Exception(string.Format("Data of type '{0}' expected, data of type '{1}' received.",
                        typeof (JObject), data.GetType()));
                }

                IList<JProperty> dataProperties = dataAsJObject.Properties().ToList();

                foreach (
                    JProperty property in dataProperties.Where(p => p.IsPrimitive() || p.IsEnumerableOfPrimitives()))
                {
                    JToken propertyData;

                    try
                    {
                        propertyData = property.Value;
                    }
                    catch (Exception ex)
                    {
                        Dev2Logger.Log.Error(ex);
                        propertyData = null;
                    }

                    if (propertyData != null)
                    {
                        paths.Add(BuildPath(propertyStack, property, root));
                    }
                }

                foreach (
                    JProperty property in dataProperties.Where(p => !p.IsPrimitive() && !p.IsEnumerableOfPrimitives()))
                {
                    JContainer propertyData;

                    try
                    {
                        propertyData = property.Value as JContainer;
                    }
                    catch (Exception ex)
                    {
                        Dev2Logger.Log.Error(ex);
                        propertyData = null;
                        //TODO When an exception is encountered stop discovery for this path and write to log
                    }

                    if (propertyData != null)
                    {
                        if (property.IsEnumerable())
                        {
                            // ReSharper disable RedundantCast
                            var enumerableData = propertyData as IEnumerable;
                            // ReSharper restore RedundantCast

                            // ReSharper disable ConditionIsAlwaysTrueOrFalse
                            if (enumerableData != null)
                                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                            {
                                IEnumerator enumerator = enumerableData.GetEnumerator();
                                enumerator.Reset();
                                if (enumerator.MoveNext())
                                {
                                    propertyData = enumerator.Current as JContainer;

                                    if (propertyData != null)
                                    {
                                        propertyStack.Push(new Tuple<JProperty, bool>(property, true));
                                        paths.AddRange(BuildPaths(propertyData, propertyStack, root));
                                        propertyStack.Pop();
                                    }
                                }
                            }
                        }
                        else
                        {
                            propertyStack.Push(new Tuple<JProperty, bool>(property, true));
                            paths.AddRange(BuildPaths(propertyData, propertyStack, root));
                            propertyStack.Pop();
                        }
                    }
                }
            }

            return paths;
        }

        private IPath BuildPath(Stack<Tuple<JProperty, bool>> propertyStack, JProperty jProperty, JToken root)
        {
            var path = new JsonPath();

            path.ActualPath = string.Join(JsonPath.SeperatorSymbol,
                propertyStack.Reverse().Select(p => path.CreatePathSegment(p.Item1).ToString(p.Item2)));

            List<Tuple<IPathSegment, bool>> displayPathSegments =
                propertyStack.Reverse()
                    .Select(p => new Tuple<IPathSegment, bool>(path.CreatePathSegment(p.Item1), p.Item2))
                    .ToList();
            bool recordsetEncountered = false;

            for (int i = displayPathSegments.Count - 1; i >= 0; i--)
            {
                Tuple<IPathSegment, bool> pathSegment = displayPathSegments[i];
                if (recordsetEncountered)
                {
                    pathSegment.Item1.IsEnumarable = false;
                }

                if (pathSegment.Item1.IsEnumarable && pathSegment.Item2) recordsetEncountered = true;
            }

            path.DisplayPath = string.Join(JsonPath.SeperatorSymbol,
                displayPathSegments.Select(p => p.Item1.ToString(p.Item2)));

            if (path.ActualPath != string.Empty)
            {
                path.ActualPath += JsonPath.SeperatorSymbol;
            }

            if (path.DisplayPath != string.Empty)
            {
                path.DisplayPath += JsonPath.SeperatorSymbol;
            }

            path.ActualPath += path.CreatePathSegment(jProperty).ToString();
            path.DisplayPath += path.CreatePathSegment(jProperty).ToString();
            path.SampleData += GetSampleData(root, path);

            return path;
        }

        private string GetSampleData(JToken root, IPath path)
        {
            var navigator = new JsonNavigator(root.ToString());
            return string.Join(GlobalConstants.AnythingToXmlPathSeperator,
                navigator.SelectEnumerable(path)
                    .Select(
                        o =>
                            o.ToString()
                                .Replace(GlobalConstants.AnythingToXmlPathSeperator,
                                    GlobalConstants.AnytingToXmlCommaToken))
                    .Take(10));
        }

        #endregion Private Methods
    }
}
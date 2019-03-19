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
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Graph;
using Newtonsoft.Json.Linq;
using Warewolf.Resource.Errors;



namespace Unlimited.Framework.Converters.Graph.String.Json
{
    [Serializable]
    public class JsonMapper : IMapper
    {
        #region Methods

        public IEnumerable<IPath> Map(object data)
        {
            var jToken = JToken.Parse(data.ToString());
            var propertyStack = new Stack<Tuple<JProperty, bool>>();

            return BuildPaths(jToken, propertyStack, jToken);
        }

        #endregion Methods

        #region Private Methods

        IEnumerable<IPath> BuildPaths(JToken data, Stack<Tuple<JProperty, bool>> propertyStack, JToken root)
        {
            var paths = new List<IPath>();

            if (propertyStack.Count == 0 && data.IsEnumerable())
            {
                paths.Add(new JsonPath(JsonPath.EnumerableSymbol + JsonPath.SeperatorSymbol,
                    JsonPath.EnumerableSymbol + JsonPath.SeperatorSymbol, data.ToString()));
            }

            if (propertyStack.Count == 0 && data.IsPrimitive())
            {
                var value = data as JValue;
                var type = value.Value.GetType().Name;
                paths.Add(new JsonPath(type, type, data.ToString()));
            }
            else
            {
                if (data.IsObject())
                {
                    AddObjectPaths(data, propertyStack, root, paths);
                }
            }

            return paths;
        }

        void AddObjectPaths(JToken data, Stack<Tuple<JProperty, bool>> propertyStack, JToken root, List<IPath> paths)
        {
            var dataAsJObject = data as JObject;
            if (dataAsJObject == null)
            {
                throw new Exception(string.Format(ErrorResource.DataTypeMismatch, typeof(JObject), data.GetType()));
            }
            IList<JProperty> dataProperties = dataAsJObject.Properties().ToList();
            foreach (JProperty property in dataProperties.Where(p => p.IsPrimitive() || p.IsEnumerableOfPrimitives()))
            {
                JToken propertyData;
                try
                {
                    propertyData = property.Value;
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                    propertyData = null;
                }
                if (propertyData != null)
                {
                    paths.Add(BuildPath(propertyStack, property, root));
                }
            }

            foreach (JProperty property in dataProperties.Where(p => !p.IsPrimitive() && !p.IsEnumerableOfPrimitives()))
            {
                TryAddPropertyAsJContainer(propertyStack, root, paths, property);
            }
        }

        private void TryAddPropertyAsJContainer(Stack<Tuple<JProperty, bool>> propertyStack, JToken root, List<IPath> paths, JProperty property)
        {
            JContainer propertyData;
            try
            {
                propertyData = property.Value as JContainer;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                propertyData = null;
                //TODO When an exception is encountered stop discovery for this path and write to log
            }

            if (propertyData != null)
            {
                if (property.IsEnumerable())
                {
                    AddPropertyStack(propertyStack, root, paths, property, propertyData);
                }
                else
                {
                    propertyStack.Push(new Tuple<JProperty, bool>(property, true));
                    paths.AddRange(BuildPaths(propertyData, propertyStack, root));
                    propertyStack.Pop();
                }
            }
        }

        void AddPropertyStack(Stack<Tuple<JProperty, bool>> propertyStack, JToken root, List<IPath> paths, JProperty property, JContainer propertyData)
        {
            if (propertyData is IEnumerable enumerableData)
            {
                var enumerator = enumerableData.GetEnumerator();
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

        IPath BuildPath(Stack<Tuple<JProperty, bool>> propertyStack, JProperty jProperty, JToken root)
        {
            var path = new JsonPath();

            path.ActualPath = string.Join(JsonPath.SeperatorSymbol,
                propertyStack.Reverse().Select(p => path.CreatePathSegment(p.Item1).ToString(p.Item2)));

            var displayPathSegments =
                propertyStack.Reverse()
                    .Select(p => new Tuple<IPathSegment, bool>(path.CreatePathSegment(p.Item1), p.Item2))
                    .ToList();
            var recordsetEncountered = false;

            for (int i = displayPathSegments.Count - 1; i >= 0; i--)
            {
                var pathSegment = displayPathSegments[i];
                if (recordsetEncountered)
                {
                    pathSegment.Item1.IsEnumarable = false;
                }

                if (pathSegment.Item1.IsEnumarable && pathSegment.Item2)
                {
                    recordsetEncountered = true;
                }
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

            path.ActualPath += path.CreatePathSegment(jProperty);
            path.DisplayPath += path.CreatePathSegment(jProperty);
            path.SampleData += GetSampleData(root, path);

            return path;
        }

        string GetSampleData(JToken root, IPath path)
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
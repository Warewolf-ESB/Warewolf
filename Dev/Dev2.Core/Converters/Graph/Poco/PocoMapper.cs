/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Graph;

// ReSharper disable CheckNamespace

namespace Unlimited.Framework.Converters.Graph.Poco
// ReSharper restore CheckNamespace
{
    [Serializable]
    public class PocoMapper : IMapper
    {
        #region Constructors

        #endregion Constructors

        #region Methods

        public IEnumerable<IPath> Map(object data)
        {
            var propertyStack = new Stack<Tuple<string, bool, bool, object>>();
            return BuildPaths(data, propertyStack, data);
        }

        #endregion Methods

        #region Private Methods

        private IEnumerable<IPath> BuildPaths(object data, Stack<Tuple<string,bool, bool, object>> propertyStack,
            object root)
        {
            var paths = new List<IPath>();

            if (propertyStack.Count == 0 && data.GetType().IsEnumerable())
            {
                //
                // Handle if the poco mapper is used to map to an raw enumerable
                //
//                paths.Add(new PocoPath("UnnamedArray"+PocoPath.EnumerableSymbol + PocoPath.SeperatorSymbol,
//                    "UnnamedArray" + PocoPath.EnumerableSymbol + PocoPath.SeperatorSymbol));

                var enumerableData = data as IEnumerable;

                if (enumerableData != null)
                {
                    IEnumerator enumerator = enumerableData.GetEnumerator();
                    enumerator.Reset();
                    if (enumerator.MoveNext())
                    {
                        propertyStack.Push(new Tuple<string, bool, bool, object>("UnnamedArray", true, true, enumerableData));
                        MapData(enumerator.Current, propertyStack, root, paths);
                        propertyStack.Pop();

                        
                    }
                }
            }

            if (propertyStack.Count == 0 && data.GetType().IsPrimitive())
            {
                //
                // Handle if the poco mapper is used to map to a raw primitive
                //
                paths.Add(new PocoPath(PocoPath.SeperatorSymbol, PocoPath.SeperatorSymbol, PocoPath.SeperatorSymbol,
                    data.ToString()));
            }
            else
            {
                MapData(data, propertyStack, root, paths);
            }

            return paths;
        }

        private void MapData(object data, Stack<Tuple<string,bool, bool, object>> propertyStack, object root, List<IPath> paths)
        {
            PropertyInfo[] dataProperties = data.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach(PropertyInfo propertyInfo in dataProperties.Where(p => p.PropertyType.IsPrimitive()))
            {
                object propertyData;

                try
                {
                    propertyData = propertyInfo.GetValue(data, null);
                }
                catch(Exception ex)
                {
                    Dev2Logger.Error(ex);
                    propertyData = null;
                }

                if(propertyData != null)
                {
                    paths.Add(BuildPath(propertyStack, propertyInfo.Name,propertyInfo.PropertyType.IsEnumerable(), root));
                }
            }

            foreach(PropertyInfo propertyInfo in dataProperties.Where(p => !p.PropertyType.IsPrimitive()))
            {
                object propertyData;

                try
                {
                    propertyData = propertyInfo.GetValue(data, null);
                }
                catch(Exception ex)
                {
                    Dev2Logger.Error(ex);
                    propertyData = null;
                    //TODO When an exception is encountered stop discovery for this path and write to log
                }

                if(propertyData != null)
                {
                    if(propertyInfo.PropertyType.IsEnumerable())
                    {
                        var enumerableData = propertyData as IEnumerable;

                        if(enumerableData != null)
                        {
                            propertyStack.Push(new Tuple<string,bool, bool, object>(propertyInfo.Name,propertyInfo.PropertyType.IsEnumerable(), false, data));
                            paths.AddRange(BuildPaths(propertyData, propertyStack, root));
                            propertyStack.Pop();

                            IEnumerator enumerator = enumerableData.GetEnumerator();
                            enumerator.Reset();
                            if(enumerator.MoveNext())
                            {
                                propertyData = enumerator.Current;

                                propertyStack.Push(new Tuple<string, bool, bool, object>(propertyInfo.Name, propertyInfo.PropertyType.IsEnumerable(), true, data));
                                paths.AddRange(BuildPaths(propertyData, propertyStack, root));
                                propertyStack.Pop();
                            }
                        }
                    }
                    else
                    {
                        propertyStack.Push(new Tuple<string, bool, bool, object>(propertyInfo.Name, propertyInfo.PropertyType.IsEnumerable(), true, data));
                        paths.AddRange(BuildPaths(propertyData, propertyStack, root));
                        propertyStack.Pop();
                    }
                }
            }
        }

        private IPath BuildPath(Stack<Tuple<string,bool, bool, object>> propertyStack, string name,bool isEnumerable,
            object root)
        {
            var path = new PocoPath();

            path.ActualPath = string.Join(PocoPath.SeperatorSymbol,
                propertyStack.Reverse().Select(p => path.CreatePathSegment(p.Item1,p.Item2).ToString(p.Item3)));

            List<Tuple<IPathSegment, bool>> displayPathSegments =
                propertyStack.Reverse()
                    .Select(p => new Tuple<IPathSegment, bool>(path.CreatePathSegment(p.Item1, p.Item2), p.Item3))
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

            path.DisplayPath = string.Join(PocoPath.SeperatorSymbol,
                displayPathSegments.Select(p => p.Item1.ToString(p.Item2)));

            if (path.ActualPath != string.Empty)
            {
                path.ActualPath += PocoPath.SeperatorSymbol;
            }

            if (path.DisplayPath != string.Empty)
            {
                path.DisplayPath += PocoPath.SeperatorSymbol;
            }

            path.ActualPath += path.CreatePathSegment(name,isEnumerable).ToString();
            path.DisplayPath += path.CreatePathSegment(name, isEnumerable).ToString();

            return path;
        }

        #endregion Private Methods
    }
}
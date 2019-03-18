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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Graph;



namespace Unlimited.Framework.Converters.Graph.Poco

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

        IEnumerable<IPath> BuildPaths(object data, Stack<Tuple<string, bool, bool, object>> propertyStack,
            object root)
        {
            var paths = new List<IPath>();

            if (propertyStack.Count == 0 && data.GetType().IsEnumerable() && data is IEnumerable enumerableData)
            {
                var enumerator = enumerableData.GetEnumerator();
                enumerator.Reset();
                if (enumerator.MoveNext())
                {
                    propertyStack.Push(new Tuple<string, bool, bool, object>("UnnamedArray", true, true, enumerableData));
                    MapData(enumerator.Current, propertyStack, root, paths);
                    propertyStack.Pop();


                }
            }


            if (propertyStack.Count == 0 && data.GetType().IsPrimitive())
            {
                paths.Add(new PocoPath(PocoPath.SeperatorSymbol, PocoPath.SeperatorSymbol, PocoPath.SeperatorSymbol,
                    data.ToString()));
            }
            else
            {
                MapData(data, propertyStack, root, paths);
            }

            return paths;
        }

        void MapData(object data, Stack<Tuple<string, bool, bool, object>> propertyStack, object root, List<IPath> paths)
        {
            var dataProperties = data.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (PropertyInfo propertyInfo in dataProperties.Where(p => p.PropertyType.IsPrimitive()))
            {
                object propertyData;

                try
                {
                    propertyData = propertyInfo.GetValue(data, null);
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                    propertyData = null;
                }

                if (propertyData != null)
                {
                    paths.Add(BuildPath(propertyStack, propertyInfo.Name, propertyInfo.PropertyType.IsEnumerable()));
                }
            }

            foreach (PropertyInfo propertyInfo in dataProperties.Where(p => !p.PropertyType.IsPrimitive()))
            {
                object propertyData;

                try
                {
                    propertyData = propertyInfo.GetValue(data, null);
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                    propertyData = null;
                }

                if (propertyData != null)
                {
                    if (propertyInfo.PropertyType.IsEnumerable())
                    {
                        MapEnumarableData(data, propertyStack, root, paths, propertyInfo, propertyData);
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

        void MapEnumarableData(object data, Stack<Tuple<string, bool, bool, object>> propertyStack, object root, List<IPath> paths, PropertyInfo propertyInfo, object propertyData)
        {
            if (propertyData is IEnumerable enumerableData)
            {
                propertyStack.Push(new Tuple<string, bool, bool, object>(propertyInfo.Name, propertyInfo.PropertyType.IsEnumerable(), false, data));
                paths.AddRange(BuildPaths(propertyData, propertyStack, root));
                propertyStack.Pop();

                var enumerator = enumerableData.GetEnumerator();
                enumerator.Reset();
                if (enumerator.MoveNext())
                {
                    propertyData = enumerator.Current;

                    propertyStack.Push(new Tuple<string, bool, bool, object>(propertyInfo.Name, propertyInfo.PropertyType.IsEnumerable(), true, data));
                    paths.AddRange(BuildPaths(propertyData, propertyStack, root));
                    propertyStack.Pop();
                }
            }
        }

        IPath BuildPath(Stack<Tuple<string, bool, bool, object>> propertyStack, string name, bool isEnumerable)
        {
            var path = new PocoPath();

            path.ActualPath = string.Join(PocoPath.SeperatorSymbol,
                propertyStack.Reverse().Select(p => path.CreatePathSegment(p.Item1, p.Item2).ToString(p.Item3)));

            var displayPathSegments =
                propertyStack.Reverse()
                    .Select(p => new Tuple<IPathSegment, bool>(path.CreatePathSegment(p.Item1, p.Item2), p.Item3))
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

            path.ActualPath += path.CreatePathSegment(name, isEnumerable);
            path.DisplayPath += path.CreatePathSegment(name, isEnumerable);

            return path;
        }

        #endregion Private Methods
    }
}
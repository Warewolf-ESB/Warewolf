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
            Stack<Tuple<PropertyInfo, bool, object>> propertyStack = new Stack<Tuple<PropertyInfo, bool, object>>();
            return BuildPaths(data, propertyStack, data);
        }

        #endregion Methods

        #region Private Methods

        private IEnumerable<IPath> BuildPaths(object data, Stack<Tuple<PropertyInfo, bool, object>> propertyStack, object root)
        {
            List<IPath> paths = new List<IPath>();

            if(propertyStack.Count == 0 && data.GetType().IsEnumerable())
            {
                //
                // Handle if the poco mapper is used to map to an raw enumerable
                //
                paths.Add(new PocoPath(PocoPath.EnumerableSymbol + PocoPath.SeperatorSymbol, PocoPath.EnumerableSymbol + PocoPath.SeperatorSymbol));
            }

            if(propertyStack.Count == 0 && data.GetType().IsPrimitive())
            {
                //
                // Handle if the poco mapper is used to map to a raw primitive
                //
                paths.Add(new PocoPath(PocoPath.SeperatorSymbol, PocoPath.SeperatorSymbol, PocoPath.SeperatorSymbol, data.ToString()));
            }
            else
            {
                PropertyInfo[] dataProperties = data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

                foreach(PropertyInfo propertyInfo in dataProperties.Where(p => p.PropertyType.IsPrimitive()))
                {
                    object propertyData;

                    try
                    {
                        propertyData = propertyInfo.GetValue(data, null);
                    }
                    catch(Exception ex)
                    {
                        Dev2Logger.Log.Error(ex);
                        propertyData = null;
                        //TODO When an exception is encountered stop discovery for this path and write to log
                    }

                    if(propertyData != null)
                    {
                        paths.Add(BuildPath(propertyStack, propertyInfo, root));
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
                        Dev2Logger.Log.Error(ex);
                        propertyData = null;
                        //TODO When an exception is encountered stop discovery for this path and write to log
                    }

                    if(propertyData != null)
                    {
                        if(propertyInfo.PropertyType.IsEnumerable())
                        {
                            IEnumerable enumerableData = propertyData as IEnumerable;

                            if(enumerableData != null)
                            {
                                propertyStack.Push(new Tuple<PropertyInfo, bool, object>(propertyInfo, false, data));
                                paths.AddRange(BuildPaths(propertyData, propertyStack, root));
                                propertyStack.Pop();

                                IEnumerator enumerator = enumerableData.GetEnumerator();
                                enumerator.Reset();
                                if(enumerator.MoveNext())
                                {
                                    propertyData = enumerator.Current;

                                    propertyStack.Push(new Tuple<PropertyInfo, bool, object>(propertyInfo, true, data));
                                    paths.AddRange(BuildPaths(propertyData, propertyStack, root));
                                    propertyStack.Pop();
                                }
                            }
                        }
                        else
                        {
                            propertyStack.Push(new Tuple<PropertyInfo, bool, object>(propertyInfo, true, data));
                            paths.AddRange(BuildPaths(propertyData, propertyStack, root));
                            propertyStack.Pop();
                        }
                    }
                }
            }

            return paths;
        }

        private IPath BuildPath(Stack<Tuple<PropertyInfo, bool, object>> propertyStack, PropertyInfo property, object root)
        {
            PocoPath path = new PocoPath();

            path.ActualPath = string.Join(PocoPath.SeperatorSymbol, propertyStack.Reverse().Select(p => path.CreatePathSegment(p.Item1).ToString(p.Item2)));

            List<Tuple<IPathSegment, bool>> displayPathSegments = propertyStack.Reverse().Select(p => new Tuple<IPathSegment, bool>(path.CreatePathSegment(p.Item1), p.Item2)).ToList();
            bool recordsetEncountered = false;

            for(int i = displayPathSegments.Count - 1; i >= 0; i--)
            {
                Tuple<IPathSegment, bool> pathSegment = displayPathSegments[i];
                if(recordsetEncountered)
                {
                    pathSegment.Item1.IsEnumarable = false;
                }

                if(pathSegment.Item1.IsEnumarable && pathSegment.Item2) recordsetEncountered = true;
            }

            path.DisplayPath = string.Join(PocoPath.SeperatorSymbol, displayPathSegments.Select(p => p.Item1.ToString(p.Item2)));

            if(path.ActualPath != string.Empty)
            {
                path.ActualPath += PocoPath.SeperatorSymbol;
            }

            if(path.DisplayPath != string.Empty)
            {
                path.DisplayPath += PocoPath.SeperatorSymbol;
            }

            path.ActualPath += path.CreatePathSegment(property).ToString();
            path.DisplayPath += path.CreatePathSegment(property).ToString();
            path.SampleData = GetSampleData(root, path);

            return path;
        }

        private string GetSampleData(object root, IPath path)
        {
            PocoNavigator navigator = new PocoNavigator(root);
            return string.Join(GlobalConstants.AnythingToXmlPathSeperator, navigator.SelectEnumerable(path).Select(o => o.ToString().Replace(GlobalConstants.AnythingToXmlPathSeperator, GlobalConstants.AnytingToXmlCommaToken)).Take(10));
        }

        #endregion Private Methods
    }
}

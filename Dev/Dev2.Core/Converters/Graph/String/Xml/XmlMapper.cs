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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Graph;



namespace Unlimited.Framework.Converters.Graph.String.Xml

{
    [Serializable]
    public class XmlMapper : IMapper
    {
        #region Constructors

        #endregion Constructors

        #region Methods

        public IEnumerable<IPath> Map(object data)
        {
            var document = XDocument.Parse(data.ToString());
            var elementStack = new Stack<Tuple<XElement, bool>>();

            //
            // Get all paths
            //
            var allPaths = BuildPaths(document.Root, elementStack, document.Root);

            //
            // Find unique paths
            //
            var uniquePaths = new Dictionary<string, IPath>();
            foreach (IPath path in allPaths)
            {
                if (!uniquePaths.TryGetValue(path.ActualPath, out IPath tmpPath))
                {
                    uniquePaths.Add(path.ActualPath, path);
                }
            }

            return uniquePaths.Values;
        }

        #endregion Methods

        #region Private Methods

        IEnumerable<IPath> BuildPaths(XElement element, Stack<Tuple<XElement, bool>> elementStack, XElement root)
        {
            var paths = new List<IPath>();

            //
            // Build path for current element
            //
            if (!element.HasElements && !element.HasAttributes)
            {
                paths.Add(BuildPath(elementStack, element, root));
            }

            if (!element.HasElements)
            {
                paths.Add(BuildPath(elementStack, element, root));
            }
            //
            // Build paths for attributes of current element
            //
            elementStack.Push(new Tuple<XElement, bool>(element, false));

            foreach (XAttribute attribute in element.Attributes())
            {
                paths.Add(BuildPath(elementStack, attribute, root));
            }

            elementStack.Pop();

            //
            // Build paths for child elements of current element
            //
            foreach (XElement childElement in element.Elements())
            {
                var cake =
                    element.Elements().Select(e => e.Name.ToString()).GroupBy(s => s);
                var considerEnumerable = cake.First(g => g.Key == childElement.Name.ToString()).Count() > 1;

                elementStack.Push(new Tuple<XElement, bool>(element, considerEnumerable));
                paths.AddRange(BuildPaths(childElement, elementStack, root));
                elementStack.Pop();
            }

            return paths;
        }

        IPath BuildPath(Stack<Tuple<XElement, bool>> elementStack, XElement element, XElement root)
        {
            var path = new XmlPath();

            path.ActualPath = string.Join(XmlPath.NodeSeperatorSymbol,
                elementStack.Reverse().Select(e => path.CreatePathSegment(e.Item1).ToString(e.Item2)));

            var displayPathSegments =
                elementStack.Reverse()
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

            path.DisplayPath = string.Join(XmlPath.NodeSeperatorSymbol,
                displayPathSegments.Select(p => p.Item1.ToString(p.Item2)));

            if (path.ActualPath != string.Empty)
            {
                path.ActualPath += XmlPath.NodeSeperatorSymbol;
            }

            if (path.DisplayPath != string.Empty)
            {
                path.DisplayPath += XmlPath.NodeSeperatorSymbol;
            }

            path.ActualPath += path.CreatePathSegment(element).ToString();
            path.DisplayPath += path.CreatePathSegment(element).ToString();
            path.SampleData += GetSampleData(root, path);

            return path;
        }

        IPath BuildPath(Stack<Tuple<XElement, bool>> elementStack, XAttribute attribute, XElement root)
        {
            var path = new XmlPath();

            path.ActualPath = string.Join(XmlPath.NodeSeperatorSymbol,
                elementStack.Reverse().Select(e => path.CreatePathSegment(e.Item1).ToString(e.Item2)));

            var displayPathSegments =
                elementStack.Reverse()
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

            path.DisplayPath = string.Join(XmlPath.NodeSeperatorSymbol,
                displayPathSegments.Select(p => p.Item1.ToString(p.Item2)));

            if (path.ActualPath != string.Empty)
            {
                path.ActualPath += XmlPath.AttributeSeperatorSymbol;
            }

            if (path.DisplayPath != string.Empty)
            {
                path.DisplayPath += XmlPath.AttributeSeperatorSymbol;
            }

            path.ActualPath += path.CreatePathSegment(attribute).ToString();
            path.DisplayPath += path.CreatePathSegment(attribute).ToString();
            path.SampleData += GetSampleData(root, path);

            return path;
        }

        string GetSampleData(XElement root, IPath path)
        {
            var navigator = new XmlNavigator(root.ToString());
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
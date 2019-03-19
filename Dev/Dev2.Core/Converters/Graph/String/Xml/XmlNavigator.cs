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
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Converters.Graph;
using Warewolf.Resource.Errors;


namespace Unlimited.Framework.Converters.Graph.String.Xml
{
    [Serializable]
    public class XmlNavigator : NavigatorBase, INavigator
    {
        #region Constructor

        public XmlNavigator(object data)
        {
            Data = XDocument.Parse(data.ToString());
        }

        #endregion Constructor

        #region Methods

        public object SelectScalar(IPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }


            if (!(path is XmlPath xmlPath))
            {
                throw new Exception(string.Format(ErrorResource.PathMismatch,
                    typeof(XmlPath), path.GetType()));
            }

            var document = Data as XDocument;
            var returnData = string.Empty;
            var currentElement = document.Root;

            if (path.ActualPath == XmlPath.NodeSeperatorSymbol)
            {
                if (currentElement != null)
                {
                    returnData = currentElement.ToString();
                }
            }
            else
            {
                var pathSegments = new List<XmlPathSegment>(xmlPath.GetSegements().OfType<XmlPathSegment>());
                var segmentIndex = 0;

                while (currentElement != null && segmentIndex < pathSegments.Count)
                {
                    if (segmentIndex == 0 && currentElement.Name != pathSegments[segmentIndex].ActualSegment && currentElement.Name.LocalName != pathSegments[segmentIndex].ActualSegment)
                    {
                        currentElement = null;
                        returnData = null;
                    }
                    else if (segmentIndex == 0 && pathSegments.Count == 1 &&
                             currentElement.Name.LocalName == pathSegments[segmentIndex].ActualSegment)
                    {
                        returnData = currentElement.Value;
                    }
                    else
                    {
                        returnData = ScalarSegment(ref currentElement, pathSegments, segmentIndex);
                    }

                    segmentIndex++;
                }
            }

            return returnData;
        }

        static string ScalarSegment(ref XElement currentElement, List<XmlPathSegment> pathSegments, int segmentIndex)
        {
            var returnData = string.Empty;
            if (segmentIndex > 0)
            {
                if (pathSegments[segmentIndex].IsAttribute)
                {
                    var attribute = currentElement.Attribute(pathSegments[segmentIndex].ActualSegment);

                    if (attribute != null)
                    {
                        currentElement = null;
                        returnData = attribute.Value;
                    }
                }
                else
                {
                    returnData = ActualSegment(ref currentElement, pathSegments, segmentIndex);
                }
            }
            return returnData;
        }

        static string ActualSegment(ref XElement currentElement, List<XmlPathSegment> pathSegments, int segmentIndex)
        {
            string returnData;
            var actualSegment = pathSegments[segmentIndex].ActualSegment;
            var newCurrentElement = currentElement.Elements(actualSegment).LastOrDefault();
            if (newCurrentElement != null)
            {
                returnData = newCurrentElement.Value;
                currentElement = newCurrentElement;
            }
            else
            {
                newCurrentElement = currentElement.Elements().LastOrDefault(element => element.Name.LocalName.Equals(actualSegment, StringComparison.InvariantCultureIgnoreCase));
                if (newCurrentElement != null)
                {
                    returnData = newCurrentElement.Value;
                    currentElement = newCurrentElement;
                }
                else
                {
                    returnData = string.Empty;
                }
            }

            return returnData;
        }

        public IEnumerable<object> SelectEnumerable(IPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            var xmlPath = path as XmlPath;

            if (xmlPath == null)
            {
                throw new Exception(string.Format(ErrorResource.PathMismatch,
                    typeof (XmlPath), path.GetType()));
            }

            var document = Data as XDocument;

            List<object> returnData = null;
            var currentElement = document.Root;

            if (path.ActualPath == XmlPath.NodeSeperatorSymbol)
            {
                if (currentElement != null)
                {
                    returnData = new List<object> {currentElement.ToString()};
                }
            }
            else
            {
                var pathSegments = new List<IPathSegment>(xmlPath.GetSegements());
                returnData =
                    new List<object>(SelectEnumberable(pathSegments.Skip(1).ToList(), pathSegments.FirstOrDefault(),
                        currentElement));
            }

            return returnData;
        }

        public Dictionary<IPath, IList<object>> SelectEnumerablesAsRelated(IList<IPath> paths)
        {
            //
            // Get valid paths
            //
            IList<IPath> validPaths = new List<IPath>(paths.OfType<XmlPath>().ToList());

            //
            // Setup results structure
            //
            var results = new Dictionary<IPath, IList<object>>();
            BuildResultsStructure(validPaths, results);

            if (validPaths.Count == 1 && validPaths[0].ActualPath == XmlPath.NodeSeperatorSymbol)
            {
                results[validPaths[0]].Add(Data);
            }
            else
            {
                var document = Data as XDocument;

                //
                // Create the root node
                //
                var rootIndexedValueTreeNode = new IndexedPathSegmentTreeNode<string> {CurrentValue = document};

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

        protected override void BuildIndexedTree(IList<IPath> paths,
            Dictionary<IPath, List<IPathSegment>> indexedPathSegments,
            IndexedPathSegmentTreeNode<string> rootIndexedValueTreeNode)
        {
            foreach (IPath path in paths)
            {
                var indexedPathSegmentTreeNode = rootIndexedValueTreeNode;
                var pathSegmentCount = 0;

                while (pathSegmentCount < indexedPathSegments[path].Count)
                {
                    IndexedPathSegmentTreeNode<string> tmpIndexedPathSegmentTreeNode = null;
                    var pathSegment = indexedPathSegments[path][pathSegmentCount] as XmlPathSegment;
                    XmlPathSegment parentPathSegment;

                    parentPathSegment = pathSegmentCount > 0 ? indexedPathSegments[path][pathSegmentCount - 1] as XmlPathSegment : null;

                    if (indexedPathSegmentTreeNode != null && pathSegment != null && !indexedPathSegmentTreeNode.TryGetValue(pathSegment.ActualSegment,
                        out tmpIndexedPathSegmentTreeNode))
                    {
                        var newIndexedPathSegmentTreeNode =
                            CreatePathSegmentIndexedPathSegmentTreeNode(pathSegment, parentPathSegment,
                                indexedPathSegmentTreeNode);
                        indexedPathSegmentTreeNode.Add(pathSegment.ActualSegment, newIndexedPathSegmentTreeNode);
                        indexedPathSegmentTreeNode = newIndexedPathSegmentTreeNode;
                    }
                    else
                    {
                        indexedPathSegmentTreeNode = tmpIndexedPathSegmentTreeNode;
                    }

                    pathSegmentCount++;
                }
            }
        }

        protected override void WriteToResults(IList<IPath> paths,
            Dictionary<IPath, List<IPathSegment>> indexedPathSegments,
            IndexedPathSegmentTreeNode<string> rootIndexedValueTreeNode, Dictionary<IPath, IList<object>> results)
        {
            foreach (IPath path in paths)
            {
                var list = indexedPathSegments[path];

                var indexedPathSegmentTreeNode =
                    rootIndexedValueTreeNode[list.Select(p => p.ActualSegment).ToList()];

                if (indexedPathSegmentTreeNode.CurrentValue is XElement element)
                {
                    results[path].Add(element.Value);
                }
                else
                {
                    var value = indexedPathSegmentTreeNode.CurrentValue as XAttribute;
                    results[path].Add(value?.Value ?? indexedPathSegmentTreeNode.CurrentValue.ToString());
                }
            }
        }

        IndexedPathSegmentTreeNode<string> CreatePathSegmentIndexedPathSegmentTreeNode(
            XmlPathSegment pathSegment, IPathSegment parentPathSegment, IndexedPathSegmentTreeNode<string> parentNode)
        {
            var newIndexedValueTreeNode = new IndexedPathSegmentTreeNode<string>();

            if (parentNode.EnumerationComplete)
            {
                newIndexedValueTreeNode.CurrentValue = string.Empty;
                newIndexedValueTreeNode.EnumerationComplete = true;
            }
            else
            {
                if (parentNode.CurrentValue is XDocument)
                {
                    var document = parentNode.CurrentValue as XDocument;
                    newIndexedValueTreeNode.CurrentValue = document.Root;
                }
                else
                {
                    XElementSegment(pathSegment, parentPathSegment, parentNode, newIndexedValueTreeNode);
                }
            }

            return newIndexedValueTreeNode;
        }

        static void XElementSegment(XmlPathSegment pathSegment, IPathSegment parentPathSegment, IndexedPathSegmentTreeNode<string> parentNode, IndexedPathSegmentTreeNode<string> newIndexedValueTreeNode)
        {
            var parentCurentElement = parentNode.CurrentValue as XElement;

            if (parentPathSegment != null && parentPathSegment.IsEnumarable)
            {
                if (parentCurentElement != null)
                {
                    ActualXElementSegment(pathSegment, newIndexedValueTreeNode, parentCurentElement);
                }
            }
            else
            {
                if (pathSegment.IsAttribute)
                {
                    if (parentCurentElement != null)
                    {
                        newIndexedValueTreeNode.CurrentValue =
                            parentCurentElement.Attribute(pathSegment.ActualSegment);
                    }

                    if (newIndexedValueTreeNode.CurrentValue == null)
                    {
                        newIndexedValueTreeNode.CurrentValue = string.Empty;
                        newIndexedValueTreeNode.EnumerationComplete = true;
                    }
                }
                else
                {
                    if (parentCurentElement != null)
                    {
                        newIndexedValueTreeNode.CurrentValue =
                            parentCurentElement.Element(pathSegment.ActualSegment);
                    }

                    if (newIndexedValueTreeNode.CurrentValue == null)
                    {
                        newIndexedValueTreeNode.CurrentValue = string.Empty;
                        newIndexedValueTreeNode.EnumerationComplete = true;
                    }
                }
            }
        }

        static void ActualXElementSegment(XmlPathSegment pathSegment, IndexedPathSegmentTreeNode<string> newIndexedValueTreeNode, XElement parentCurentElement)
        {
            var childElements =
                                    parentCurentElement.Elements(pathSegment.ActualSegment).ToList();
            newIndexedValueTreeNode.EnumerableValue = childElements;

            if (childElements.Count == 0)
            {
                newIndexedValueTreeNode.CurrentValue = string.Empty;
                newIndexedValueTreeNode.EnumerationComplete = true;
            }
            else
            {
                newIndexedValueTreeNode.Enumerator =
                    newIndexedValueTreeNode.EnumerableValue.GetEnumerator();

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

        IEnumerable<string> SelectEnumberable(IList<IPathSegment> pathSegments, IPathSegment parentPathSegment, XElement element)
        {
            var returnData = new List<string>();
            var currentElement = element;

            if (pathSegments.Count > 0)
            {
                for (int i = 0; i < pathSegments.Count; i++)
                {
                    var pathSegment = pathSegments[i] as XmlPathSegment;
                    XmlPathSegment previousPathSegment;

                    previousPathSegment = i > 0 ? pathSegments[i - 1] as XmlPathSegment : parentPathSegment as XmlPathSegment;

                    var lastSegment = i == pathSegments.Count - 1;

                    if (previousPathSegment != null && previousPathSegment.IsEnumarable)
                    {
                        return SelectEnumerable(pathSegments, returnData, i, pathSegment, lastSegment, currentElement);
                    }
                    currentElement = SelectEnumerable(ref returnData, currentElement, pathSegment, lastSegment);
                }
            }
            else
            {
                if (currentElement.Name == parentPathSegment.ActualSegment)
                {
                    returnData.Add(currentElement.Value);
                }
            }

            return returnData;
        }

        private static XElement SelectEnumerable(ref List<string> returnData, XElement currentElement, XmlPathSegment pathSegment, bool lastSegment)
        {
            if (pathSegment != null && pathSegment.IsAttribute)
            {
                var attribute = currentElement?.Attribute(pathSegment.ActualSegment);

                if (attribute != null)
                {
                    currentElement = null;

                    if (lastSegment)
                    {
                        returnData.Add(attribute.Value);
                    }
                }
            }
            else
            {
                if (currentElement != null && pathSegment != null)
                {
                    currentElement = currentElement.Element(pathSegment.ActualSegment);
                }

                if (currentElement != null && lastSegment)
                {
                    returnData.Add(currentElement.Value);
                }
            }

            return currentElement;
        }

        List<string> SelectEnumerable(IList<IPathSegment> pathSegments, List<string> returnData, int i, XmlPathSegment pathSegment, bool lastSegment, XElement currentElement)
        {
            if (currentElement != null && pathSegment != null)
            {
                var childElements =
                    currentElement.Elements(pathSegment.ActualSegment).ToList();

                if (childElements.Count > 0)
                {
                    SelectEnumarable(pathSegments, returnData, i, pathSegment, lastSegment, childElements);
                }
            }
            return returnData;
        }

        void SelectEnumarable(IList<IPathSegment> pathSegments, List<string> returnData, int i, XmlPathSegment pathSegment, bool lastSegment, List<XElement> childElements)
        {
            if (lastSegment)
            {
                foreach (XElement childElement in childElements)
                {
                    if (pathSegment.IsAttribute)
                    {
                        returnData.AddRange(AddAttribute(returnData, pathSegment, childElement));
                    }
                    else
                    {
                        returnData.Add(childElement.Value);
                    }
                }
            }
            else
            {
                foreach (XElement childElement in childElements)
                {
                    returnData.AddRange(SelectEnumberable(pathSegments.Skip(i + 1).ToList(),
                        pathSegment, childElement));
                }
            }
        }

        static List<string> AddAttribute(List<string> returnData, XmlPathSegment pathSegment, XElement childElement)
        {
            var attribute = childElement.Attribute(pathSegment.ActualSegment);

            if (attribute != null)
            {
                returnData.Add(attribute.Value);
            }
            else
            {
                throw new Exception(string.Format("Attribute {0} not found.",
                    pathSegment.ActualSegment));
            }
            return returnData;
        }

        #endregion Private Methods
    }
}
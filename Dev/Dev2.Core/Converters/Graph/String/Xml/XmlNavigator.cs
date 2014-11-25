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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Converters.Graph;

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

            var xmlPath = path as XmlPath;

            if (xmlPath == null)
            {
                throw new Exception(string.Format("Path of type '{0}' expected, path of type '{1}' received.",
                    typeof (XmlPath), path.GetType()));
            }

            var document = Data as XDocument;

            if (document == null)
            {
                throw new Exception(string.Format("Type of {0} was expected for data, type of {1} was found instead.",
                    typeof (XDocument), Data.GetType()));
            }

            string returnData = string.Empty;
            XElement currentElement = document.Root;

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
                int segmentIndex = 0;

                while (currentElement != null && segmentIndex < pathSegments.Count)
                {
                    if (segmentIndex == 0 && currentElement.Name != pathSegments[segmentIndex].ActualSegment)
                    {
                        currentElement = null;
                        returnData = null;
                    }
                    else if (segmentIndex == 0 && pathSegments.Count == 1 &&
                             currentElement.Name == pathSegments[segmentIndex].ActualSegment)
                    {
                        returnData = currentElement.Value;
                    }
                    else if (segmentIndex > 0)
                    {
                        if (pathSegments[segmentIndex].IsAttribute)
                        {
                            XAttribute attribute = currentElement.Attribute(pathSegments[segmentIndex].ActualSegment);

                            if (attribute != null)
                            {
                                currentElement = null;
                                returnData = attribute.Value;
                            }
                        }
                        else
                        {
                            currentElement =
                                currentElement.Elements(pathSegments[segmentIndex].ActualSegment).LastOrDefault();
                            // Travis.Frisinger : 09/10/2012 - Fix for null element, naughty Brendan ;)
                            if (currentElement != null)
                            {
                                returnData = currentElement.Value;
                            }
                            else
                            {
                                returnData = string.Empty;
                            }
                        }
                    }

                    segmentIndex++;
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
                throw new Exception(string.Format("Path of type '{0}' expected, path of type '{1}' received.",
                    typeof (XmlPath), path.GetType()));
            }

            var document = Data as XDocument;

            if (document == null)
            {
                throw new Exception(string.Format("Type of {0} was expected for data, type of {1} was found instead.",
                    typeof (XDocument), Data.GetType()));
            }

            List<object> returnData = null;
            XElement currentElement = document.Root;

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

                if (document == null)
                {
                    throw new Exception(
                        string.Format("Type of {0} was expected for data, type of {1} was found instead.",
                            typeof (XDocument), Data.GetType()));
                }

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
                IndexedPathSegmentTreeNode<string> IndexedPathSegmentTreeNode = rootIndexedValueTreeNode;
                int pathSegmentCount = 0;

                while (pathSegmentCount < indexedPathSegments[path].Count)
                {
                    IndexedPathSegmentTreeNode<string> tmpIndexedPathSegmentTreeNode = null;
                    var pathSegment = indexedPathSegments[path][pathSegmentCount] as XmlPathSegment;
                    XmlPathSegment parentPathSegment;

                    if (pathSegmentCount > 0)
                    {
                        parentPathSegment = indexedPathSegments[path][pathSegmentCount - 1] as XmlPathSegment;
                    }
                    else
                    {
                        parentPathSegment = null;
                    }

                    if (IndexedPathSegmentTreeNode != null &&
                        (pathSegment != null &&
                         !IndexedPathSegmentTreeNode.TryGetValue(pathSegment.ActualSegment,
                             out tmpIndexedPathSegmentTreeNode)))
                    {
                        IndexedPathSegmentTreeNode<string> newIndexedPathSegmentTreeNode =
                            CreatePathSegmentIndexedPathSegmentTreeNode(pathSegment, parentPathSegment,
                                IndexedPathSegmentTreeNode);
                        IndexedPathSegmentTreeNode.Add(pathSegment.ActualSegment, newIndexedPathSegmentTreeNode);
                        IndexedPathSegmentTreeNode = newIndexedPathSegmentTreeNode;
                    }
                    else
                    {
                        IndexedPathSegmentTreeNode = tmpIndexedPathSegmentTreeNode;
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
                List<IPathSegment> list = indexedPathSegments[path];

                IndexedPathSegmentTreeNode<string> IndexedPathSegmentTreeNode =
                    rootIndexedValueTreeNode[list.Select(p => p.ActualSegment).ToList()];

                var element = IndexedPathSegmentTreeNode.CurrentValue as XElement;
                if (element != null)
                {
                    results[path].Add(element.Value);
                }
                else
                {
                    var value = IndexedPathSegmentTreeNode.CurrentValue as XAttribute;
                    if (value != null)
                    {
                        results[path].Add(value.Value);
                    }
                    else
                    {
                        results[path].Add(IndexedPathSegmentTreeNode.CurrentValue.ToString());
                    }
                }
            }
        }

        private IndexedPathSegmentTreeNode<string> CreatePathSegmentIndexedPathSegmentTreeNode(
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
                    var parentCurentElement = parentNode.CurrentValue as XElement;

                    if (parentPathSegment != null && parentPathSegment.IsEnumarable)
                    {
                        if (parentCurentElement != null)
                        {
                            List<XElement> childElements =
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
            }

            return newIndexedValueTreeNode;
        }

        private IEnumerable<string> SelectEnumberable(IList<IPathSegment> pathSegments, IPathSegment parentPathSegment,
            XElement element)
        {
            var returnData = new List<string>();
            XElement currentElement = element;

            if (pathSegments.Count > 0)
            {
                for (int i = 0; i < pathSegments.Count; i++)
                {
                    var pathSegment = pathSegments[i] as XmlPathSegment;
                    XmlPathSegment previousPathSegment;

                    if (i > 0)
                    {
                        previousPathSegment = pathSegments[i - 1] as XmlPathSegment;
                    }
                    else
                    {
                        previousPathSegment = parentPathSegment as XmlPathSegment;
                    }

                    bool lastSegment = (i == pathSegments.Count - 1);

                    if (previousPathSegment != null && previousPathSegment.IsEnumarable)
                    {
                        if (currentElement != null)
                        {
                            if (pathSegment != null)
                            {
                                List<XElement> childElements =
                                    currentElement.Elements(pathSegment.ActualSegment).ToList();

                                if (childElements.Count > 0)
                                {
                                    if (lastSegment)
                                    {
                                        foreach (XElement childElement in childElements)
                                        {
                                            if (pathSegment.IsAttribute)
                                            {
                                                XAttribute attribute = childElement.Attribute(pathSegment.ActualSegment);

                                                if (attribute != null)
                                                {
                                                    returnData.Add(attribute.Value);
                                                }
                                                else
                                                {
                                                    throw new Exception(string.Format("Attribute {0} not found.",
                                                        pathSegment.ActualSegment));
                                                }
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
                            }
                        }

                        return returnData;
                    }
                    if (pathSegment != null && pathSegment.IsAttribute)
                    {
                        if (currentElement != null)
                        {
                            XAttribute attribute = currentElement.Attribute(pathSegment.ActualSegment);

                            if (attribute != null)
                            {
                                currentElement = null;

                                if (lastSegment)
                                {
                                    returnData.Add(attribute.Value);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (currentElement != null)
                        {
                            if (pathSegment != null)
                            {
                                currentElement = currentElement.Element(pathSegment.ActualSegment);
                            }
                        }

                        if (currentElement != null && lastSegment)
                        {
                            returnData.Add(currentElement.Value);
                        }
                    }
                }
            }
            else if (currentElement.Name == parentPathSegment.ActualSegment)
            {
                returnData.Add(currentElement.Value);
            }

            return returnData;
        }

        #endregion Private Methods
    }
}
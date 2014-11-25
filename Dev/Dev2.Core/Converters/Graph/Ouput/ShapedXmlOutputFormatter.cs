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
using Dev2.DataList.Contract;

namespace Unlimited.Framework.Converters.Graph.Ouput
{
    /// <summary>
    ///     Fromats data from a source to shaped XML as defined by an output description
    /// </summary>
    [Serializable]
    public class ShapedXmlOutputFormatter : IOutputFormatter
    {
        #region Constructors

        public ShapedXmlOutputFormatter(IOutputDescription outputDescription, string rootNodeName)
        {
            RootNodeName = rootNodeName;
            OutputDescription = outputDescription;
            DataBrowser = DataBrowserFactory.CreateDataBrowser();
        }

        public ShapedXmlOutputFormatter(IOutputDescription outputDescription)
        {
            RootNodeName = "ADL";
            OutputDescription = outputDescription;
            DataBrowser = DataBrowserFactory.CreateDataBrowser();
        }

        #endregion Constructors

        #region Properties

        public string RootNodeName { get; set; }
        public IOutputDescription OutputDescription { get; private set; }

        #endregion Properties

        #region Private Properties

        private IDataBrowser DataBrowser { get; set; }

        #endregion Private Properties

        #region Methods

        /// <summary>
        ///     Formats the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public object Format(object data)
        {
            //
            // Group paths by their output expressions, each of these groups will be selected as a batch
            //
            Dictionary<string, IList<IPath>> groupedPaths = GroupPaths(OutputDescription.DataSourceShapes[0]);

            //
            // Go through each group, selecting values from the data and building XML from it
            //
            var rootNode = new XElement(RootNodeName);
            foreach (var paths in groupedPaths.Values)
            {
                //
                // Determine the type of select to perform
                //
                if (paths.Count == 1)
                {
                    //TODO Check if there is existing, more reliable, logic to detemine if an output expression is a recordset/scalar.
                    if (paths[0].OutputExpression.Contains("()"))
                    {
                        rootNode.Add(SelectEnumerable(paths[0], data));
                    }
                    else
                    {
                        rootNode.Add(SelectScalar(paths[0], data));
                    }
                }
                else if (paths.Count > 1)
                {
                    rootNode.Add(SelectEnumerableAsRelated(paths, data));
                }
            }

            return rootNode.ToString();
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        ///     Selects a single value
        /// </summary>
        private XElement SelectScalar(IPath path, object data)
        {
            object selectResult = DataBrowser.SelectScalar(path, data);
            return new XElement(GetNodeName(path.OutputExpression), selectResult);
        }

        /// <summary>
        ///     Selects values
        /// </summary>
        private IEnumerable<XElement> SelectEnumerable(IPath path, object data)
        {
            IList<XElement> returnNodes = new List<XElement>();
            IList<object> selectResults = DataBrowser.SelectEnumerable(path, data).ToList();
            string recordsetNodeName = GetRecordsetNodeName(path.OutputExpression);
            string nodeName = GetNodeName(path.OutputExpression);

            if (selectResults.Count > 0)
            {
                foreach (object result in selectResults)
                {
                    var recordsetNode = new XElement(recordsetNodeName);
                    recordsetNode.Add(new XElement(nodeName, result));

                    returnNodes.Add(recordsetNode);
                }
            }
            else
            {
                var recordsetNode = new XElement(recordsetNodeName);
                recordsetNode.Add(new XElement(nodeName, ""));

                returnNodes.Add(recordsetNode);
            }

            return returnNodes;
        }

        /// <summary>
        ///     Selectes values as though there were related, if values are nested they are related by hierarchy, if they are
        ///     parellel they are related
        ///     by position in the enumeration.
        /// </summary>
        private IEnumerable<XElement> SelectEnumerableAsRelated(IList<IPath> paths, object data)
        {
            IList<XElement> returnNodes = new List<XElement>();

            if (paths.Count > 0)
            {
                Dictionary<IPath, IList<object>> selectResults = DataBrowser.SelectEnumerablesAsRelated(paths, data);
                string recordsetNodeName = GetRecordsetNodeName(paths[0].OutputExpression);
                var nodeNames = new Dictionary<IPath, string>();
                long resultCount = selectResults[paths[0]].Count;

                foreach (IPath path in paths)
                {
                    //
                    // Check that there are the same number of results for every path
                    //
                    if (selectResults[path].Count != resultCount)
                    {
                        throw new Exception("The number of results for the paths representing the '" +
                                            path.OutputExpression + "' expression didn't match.");
                    }

                    //
                    // Index node name by path
                    //
                    nodeNames.Add(path, GetNodeName(path.OutputExpression));
                }

                for (int i = 0; i < resultCount; i++)
                {
                    var recordsetNode = new XElement(recordsetNodeName);

                    foreach (IPath path in paths)
                    {
                        recordsetNode.Add(new XElement(nodeNames[path], selectResults[path][i]));
                    }

                    returnNodes.Add(recordsetNode);
                }
            }

            return returnNodes;
        }

        /// <summary>
        ///     Groups paths by their output expressions
        /// </summary>
        private Dictionary<string, IList<IPath>> GroupPaths(IDataSourceShape dataSourceShape)
        {
            var groupedPaths = new Dictionary<string, IList<IPath>>();

            foreach (IPath path in dataSourceShape.Paths)
            {
                if (!string.IsNullOrWhiteSpace(path.OutputExpression))
                {
                    string key = GetOutputDescriptionKey(path.OutputExpression);
                    IList<IPath> dataSourceShapePaths;

                    if (groupedPaths.TryGetValue(key, out dataSourceShapePaths))
                    {
                        dataSourceShapePaths.Add(path);
                    }
                    else
                    {
                        dataSourceShapePaths = new List<IPath> {path};
                        groupedPaths.Add(key, dataSourceShapePaths);
                    }
                }
            }

            return groupedPaths;
        }

        /// <summary>
        ///     Gets the value to use as a key for an ouotput description
        /// </summary>
        private string GetOutputDescriptionKey(string outputDescription)
        {
            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();

            IList<IIntellisenseResult> parts = parser.ParseDataLanguageForIntellisense(outputDescription, "");

            if (parts.Count <= 0)
            {
                throw new Exception("Invalid output description '" + outputDescription + "'.");
            }

            string key;

            if (parts[0].Option.IsScalar)
            {
                key = parts[0].Option.Field;
            }
            else
            {
                key = parts[0].Option.Recordset + "()";
            }

            return key;
        }

        /// <summary>
        ///     Gets the name of a node from an output description
        /// </summary>
        private string GetNodeName(string outputDescription)
        {
            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();

            IList<IIntellisenseResult> parts = parser.ParseDataLanguageForIntellisense(outputDescription, "");

            if (parts.Count <= 0)
            {
                throw new Exception("Invalid output description '" + outputDescription + "'.");
            }

            return parts.Last().Option.Field;
        }

        /// <summary>
        ///     Gets the name of a recordset node from an output
        /// </summary>
        private string GetRecordsetNodeName(string outputDescription)
        {
            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();

            IList<IIntellisenseResult> parts = parser.ParseDataLanguageForIntellisense(outputDescription, "");

            if (parts.Count <= 0)
            {
                throw new Exception("Invalid output description '" + outputDescription + "'.");
            }

            return parts[0].Option.Recordset;
        }

        #endregion Private Methods
    }
}
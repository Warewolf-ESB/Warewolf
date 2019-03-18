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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.DataList.Contract;
using Warewolf.Resource.Errors;

namespace Unlimited.Framework.Converters.Graph.Ouput
{
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

        IDataBrowser DataBrowser { get; set; }

        #endregion Private Properties

        #region Methods

        public object Format(object data)
        {
            var groupedPaths = GroupPaths(OutputDescription.DataSourceShapes[0]);

            var rootNode = new XElement(RootNodeName);
            foreach (var paths in groupedPaths.Values)
            {
                if (paths.Count == 1)
                {
                    if (paths[0].OutputExpression.Contains("()"))
                    {
                        rootNode.Add(SelectEnumerable(paths[0], data));
                    }
                    else
                    {
                        rootNode.Add(SelectScalar(paths[0], data));
                    }
                }
                else
                {
                    if (paths.Count > 1)
                    {
                        rootNode.Add(SelectEnumerableAsRelated(paths, data));
                    }
                }
            }

            return rootNode.ToString();
        }

        #endregion Methods

        #region Private Methods
        
        XElement SelectScalar(IPath path, object data)
        {
            var selectResult = DataBrowser.SelectScalar(path, data);
            return new XElement(GetNodeName(path.OutputExpression), selectResult);
        }
        
        IEnumerable<XElement> SelectEnumerable(IPath path, object data)
        {
            IList<XElement> returnNodes = new List<XElement>();
            IList<object> selectResults = DataBrowser.SelectEnumerable(path, data).ToList();
            var recordsetNodeName = GetRecordsetNodeName(path.OutputExpression);
            var nodeName = GetNodeName(path.OutputExpression);

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
        
        IEnumerable<XElement> SelectEnumerableAsRelated(IList<IPath> paths, object data)
        {
            IList<XElement> returnNodes = new List<XElement>();

            if (paths.Count > 0)
            {
                var selectResults = DataBrowser.SelectEnumerablesAsRelated(paths, data);
                var recordsetNodeName = GetRecordsetNodeName(paths[0].OutputExpression);
                var nodeNames = new Dictionary<IPath, string>();
                long resultCount = selectResults[paths[0]].Count;

                foreach (IPath path in paths)
                {
                    //
                    // Check that there are the same number of results for every path
                    //
                    if (selectResults[path].Count != resultCount)
                    {
                        throw new Exception(string.Format(ErrorResource.NumberOfResultsMismatch, path.OutputExpression));
                    }

                    //
                    // Index node name by path
                    //
                    nodeNames.Add(path, GetNodeName(path.OutputExpression));
                }

                AddRecordsetNodes(paths, returnNodes, selectResults, recordsetNodeName, nodeNames, resultCount);
            }

            return returnNodes;
        }

        private static void AddRecordsetNodes(IList<IPath> paths, IList<XElement> returnNodes, Dictionary<IPath, IList<object>> selectResults, string recordsetNodeName, Dictionary<IPath, string> nodeNames, long resultCount)
        {
            for (int i = 0; i < resultCount; i++)
            {
                XElement recordsetNode;
                if (string.IsNullOrEmpty(recordsetNodeName))
                {
                    var path = paths[0];
                    recordsetNode = new XElement(nodeNames[path], selectResults[path][0]);
                }
                else
                {
                    recordsetNode = new XElement(recordsetNodeName);

                    foreach (IPath path in paths)
                    {
                        recordsetNode.Add(new XElement(nodeNames[path], selectResults[path][i]));
                    }
                }
                returnNodes.Add(recordsetNode);
            }
        }

        Dictionary<string, IList<IPath>> GroupPaths(IDataSourceShape dataSourceShape)
        {
            var groupedPaths = new Dictionary<string, IList<IPath>>();

            foreach (IPath path in dataSourceShape.Paths)
            {
                if (!string.IsNullOrWhiteSpace(path.OutputExpression))
                {
                    var key = GetOutputDescriptionKey(path.OutputExpression);

                    if (groupedPaths.TryGetValue(key, out IList<IPath> dataSourceShapePaths))
                    {
                        dataSourceShapePaths.Add(path);
                    }
                    else
                    {
                        dataSourceShapePaths = new List<IPath> { path };
                        groupedPaths.Add(key, dataSourceShapePaths);
                    }
                }
            }

            return groupedPaths;
        }
        
        string GetOutputDescriptionKey(string outputDescription)
        {
            var parser = DataListFactory.CreateLanguageParser();

            var parts = parser.ParseDataLanguageForIntellisense(outputDescription, "");

            if (parts.Count <= 0)
            {
                throw new Exception(string.Format(ErrorResource.OutputDecriptionInvalid, outputDescription));
            }

            string key;

            key = parts[0].Option.IsScalar ? parts[0].Option.Field : parts[0].Option.Recordset + "()";

            return key;
        }
        
        string GetNodeName(string outputDescription)
        {
            var parser = DataListFactory.CreateLanguageParser();

            var parts = parser.ParseDataLanguageForIntellisense(outputDescription, "");

            if (parts.Count <= 0)
            {
                throw new Exception(string.Format(ErrorResource.OutputDecriptionInvalid, outputDescription));
            }

            return parts.Last().Option.Field;
        }
        
        string GetRecordsetNodeName(string outputDescription)
        {
            var parser = DataListFactory.CreateLanguageParser();

            var parts = parser.ParseDataLanguageForIntellisense(outputDescription, "");

            if (parts.Count <= 0)
            {
                throw new Exception(string.Format(ErrorResource.OutputDecriptionInvalid, outputDescription));
            }

            return parts[0].Option.Recordset;
        }

        #endregion Private Methods
    }
}
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
using System.Xml;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.Interfaces;
using Dev2.Data.TO;


namespace Dev2.DataList.Contract
{
    class DataListIntellisenseBuilder
    {
        const string DescAttribute = "Description";

        public string DataList { set; private get; }

        public IIntellisenseFilterOpsTO FilterTO { get; set; }
        
        public IList<IDev2DataLanguageIntellisensePart> Generate()
        {
            IList<IDev2DataLanguageIntellisensePart> result = new List<IDev2DataLanguageIntellisensePart>();

            var xDoc = new XmlDocument();
            
            if (FilterTO == null)
            {
                FilterTO = new IntellisenseFilterOpsTO();
            }

            if (!string.IsNullOrEmpty(DataList))
            {
                XmlNodeList tmpRootNl = null;
                try
                {
                    xDoc.LoadXml(DataList);
                    tmpRootNl = xDoc.ChildNodes;
                }
                catch (Exception ex)
                {                    
                    Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                }

                if (tmpRootNl != null)
                {
                    var nl = tmpRootNl[0].ChildNodes;
                    AddValidNodes(result, nl);
                }
            }
            return result;
        }

        private void AddValidNodes(IList<IDev2DataLanguageIntellisensePart> result, XmlNodeList nl)
        {
            for (int i = 0; i < nl.Count; i++)
            {
                var tmpNode = nl[i];

                if (IsValidChildNode(tmpNode))
                {
                    var recordsetName = tmpNode.Name;
                    IList<IDev2DataLanguageIntellisensePart> children = new List<IDev2DataLanguageIntellisensePart>();
                    var childNl = tmpNode.ChildNodes;
                    for (int q = 0; q < childNl.Count; q++)
                    {
                        children.Add(DataListFactory.CreateIntellisensePart(childNl[q].Name, ExtractDescription(childNl[q])));
                    }
                    if (FilterTO.FilterType == enIntellisensePartType.None)
                    {
                        result.Add(DataListFactory.CreateIntellisensePart(recordsetName, ExtractDescription(tmpNode), children));
                    }
                    if (FilterTO.FilterType == enIntellisensePartType.RecordsetsOnly)
                    {
                        result.Add(DataListFactory.CreateIntellisensePart(string.Concat(recordsetName, "()"), ExtractDescription(tmpNode)));
                    }
                    if (FilterTO.FilterType == enIntellisensePartType.RecordsetFields)
                    {
                        result.Add(DataListFactory.CreateIntellisensePart(recordsetName, ExtractDescription(tmpNode), children));
                    }
                }
                else
                {
                    if (FilterTO.FilterType == enIntellisensePartType.None || FilterTO.FilterType == enIntellisensePartType.ScalarsOnly)
                    {
                        result.Add(DataListFactory.CreateIntellisensePart(tmpNode.Name, ExtractDescription(tmpNode)));
                    }
                }
            }
        }

        static bool IsValidChildNode(XmlNode tmpNode)
        {
            var result = false;

            if (tmpNode.HasChildNodes)
            {
                if (tmpNode.ChildNodes.Count == 1 && !tmpNode.ChildNodes[0].HasChildNodes)
                {
                    if (tmpNode.ChildNodes[0].Name != "#text")
                    {
                        result = true;
                    }
                }
                else
                {
                    if (tmpNode.ChildNodes.Count > 1)
                    {
                        result = true;
                    }
                }
            }

            return result;
        }

        static string ExtractDescription(XmlNode node)
        {
            var result = string.Empty;            
            var attribute = node.Attributes?[DescAttribute];
            if (attribute != null)
            {
                result = attribute.Value;
            }
            return result;
        }
    }
}

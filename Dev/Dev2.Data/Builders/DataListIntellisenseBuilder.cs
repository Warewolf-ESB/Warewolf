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
using System.Collections.Generic;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.Interfaces;
using Dev2.Data.TO;


namespace Dev2.DataList.Contract
{
    internal class DataListIntellisenseBuilder
    {
        const string DescAttribute = "Description";

        public string DataList { set; private get; }

        public IIntellisenseFilterOpsTO FilterTO { get; set; }
        
        public IList<IDev2DataLanguageIntellisensePart> Generate()
        {
            IList<IDev2DataLanguageIntellisensePart> result = new List<IDev2DataLanguageIntellisensePart>();

            XmlDocument xDoc = new XmlDocument();
            
            string rawRecsetName;
            
            if (FilterTO == null)
            {
                FilterTO = new IntellisenseFilterOpsTO();
            }
            if (FilterTO.FilterCondition != null)
            {
                rawRecsetName = FilterTO.FilterCondition;
                if (rawRecsetName.Contains("[["))
                {
                    rawRecsetName = rawRecsetName.Replace("[[", "");
                }
                if (rawRecsetName.Contains("]]"))
                {
                    rawRecsetName = rawRecsetName.Replace("]]", "");
                }
                if (rawRecsetName.Contains("()"))
                {
                    
                    rawRecsetName = rawRecsetName.Replace("()", "");
                    
                }
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
                    XmlNodeList nl = tmpRootNl[0].ChildNodes;
                    for (int i = 0; i < nl.Count; i++)
                    {
                        XmlNode tmpNode = nl[i];

                        if (IsValidChildNode(tmpNode))
                        {
                            string recordsetName = tmpNode.Name;
                            IList<IDev2DataLanguageIntellisensePart> children = new List<IDev2DataLanguageIntellisensePart>();
                            XmlNodeList childNl = tmpNode.ChildNodes;
                            for (int q = 0; q < childNl.Count; q++)
                            {
                                children.Add(DataListFactory.CreateIntellisensePart(childNl[q].Name, ExtractDescription(childNl[q])));
                            }
                            if (FilterTO.FilterType == enIntellisensePartType.All)
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
                            if (FilterTO.FilterType == enIntellisensePartType.All || FilterTO.FilterType == enIntellisensePartType.ScalarsOnly)
                            {
                                result.Add(DataListFactory.CreateIntellisensePart(tmpNode.Name, ExtractDescription(tmpNode)));
                            }
                        }
                    }
                }
            }


            return result;
        }

        private bool IsValidChildNode(XmlNode tmpNode)
        {
            bool result = false;

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
        
        private string ExtractDescription(XmlNode node)
        {
            string result = string.Empty;

            try
            {
                XmlAttribute attribute = node.Attributes?[DescAttribute];
                if (attribute != null)
                {
                    result = attribute.Value;
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
            }

            return result;
        }
    }
}

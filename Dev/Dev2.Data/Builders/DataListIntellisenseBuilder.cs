using System;
using System.Collections.Generic;
using System.Xml;
using Dev2.Common;

// ReSharper disable CheckNamespace
namespace Dev2.DataList.Contract
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Used studio side for funky stuff?!
    /// </summary>
    internal class DataListIntellisenseBuilder
    {
        const string _descAttribute = "Description";

        public string DataList { set; private get; }

        public IntellisenseFilterOpsTO FilterTO { get; set; }

        /// <summary>
        /// Generates this instance.
        /// </summary>
        /// <returns></returns>
        public IList<IDev2DataLanguageIntellisensePart> Generate()
        {
            IList<IDev2DataLanguageIntellisensePart> result = new List<IDev2DataLanguageIntellisensePart>();

            XmlDocument xDoc = new XmlDocument();
            // ReSharper disable TooWideLocalVariableScope
            string rawRecsetName;
            // ReSharper restore TooWideLocalVariableScope
            if(FilterTO == null)
            {
                FilterTO = new IntellisenseFilterOpsTO();
            }
            if(FilterTO.FilterCondition != null)
            {
                rawRecsetName = FilterTO.FilterCondition;
                if(rawRecsetName.Contains("[["))
                {
                    rawRecsetName = rawRecsetName.Replace("[[", "");
                }
                if(rawRecsetName.Contains("]]"))
                {
                    rawRecsetName = rawRecsetName.Replace("]]", "");
                }
                if(rawRecsetName.Contains("()"))
                {
                    // ReSharper disable RedundantAssignment
                    rawRecsetName = rawRecsetName.Replace("()", "");
                    // ReSharper restore RedundantAssignment
                }
            }


            if(!string.IsNullOrEmpty(DataList))
            {
                XmlNodeList tmpRootNl = null;

                try
                {
                    xDoc.LoadXml(DataList);
                    tmpRootNl = xDoc.ChildNodes;
                }
                catch(Exception ex)
                {
                    this.LogError(ex);
                }

                if(tmpRootNl != null)
                {
                    XmlNodeList nl = tmpRootNl[0].ChildNodes;
                    for(int i = 0; i < nl.Count; i++)
                    {
                        XmlNode tmpNode = nl[i];

                        if(IsValidChildNode(tmpNode))
                        {
                            // it is a record set, make it as such
                            string recordsetName = tmpNode.Name;
                            IList<IDev2DataLanguageIntellisensePart> children = new List<IDev2DataLanguageIntellisensePart>();
                            // now extract child node defs
                            XmlNodeList childNL = tmpNode.ChildNodes;
                            for(int q = 0; q < childNL.Count; q++)
                            {
                                children.Add(DataListFactory.CreateIntellisensePart(childNL[q].Name, ExtractDescription(childNL[q])));
                            }
                            if(FilterTO.FilterType == enIntellisensePartType.All)
                            {
                                result.Add(DataListFactory.CreateIntellisensePart(recordsetName, ExtractDescription(tmpNode), children));
                            }
                            if(FilterTO.FilterType == enIntellisensePartType.RecorsetsOnly)
                            {
                                result.Add(DataListFactory.CreateIntellisensePart(string.Concat(recordsetName, "()"), ExtractDescription(tmpNode)));
                            }
                            if(FilterTO.FilterType == enIntellisensePartType.RecordsetFields || FilterTO.FilterType == enIntellisensePartType.AllButRecordsets)
                            {
                                result.Add(DataListFactory.CreateIntellisensePart(recordsetName, ExtractDescription(tmpNode), children));
                            }
                        }
                        else
                        {
                            // scalar value, make it as such
                            if(FilterTO.FilterType == enIntellisensePartType.All || FilterTO.FilterType == enIntellisensePartType.ScalarsOnly || FilterTO.FilterType == enIntellisensePartType.AllButRecordsets)
                            {
                                result.Add(DataListFactory.CreateIntellisensePart(tmpNode.Name, ExtractDescription(tmpNode)));
                            }
                        }
                    }
                }
            }


            return result;
        }

        /// <summary>
        /// Determines whether [is valid child node] [the specified TMP node].
        /// </summary>
        /// <param name="tmpNode">The TMP node.</param>
        /// <returns>
        ///   <c>true</c> if [is valid child node] [the specified TMP node]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValidChildNode(XmlNode tmpNode)
        {
            bool result = false;

            if(tmpNode.HasChildNodes)
            {
                // has 1 child node that DOES NOT have child nodes
                if(tmpNode.ChildNodes.Count == 1 && !tmpNode.ChildNodes[0].HasChildNodes)
                {
                    if(tmpNode.ChildNodes[0].Name != "#text")
                    {
                        result = true;
                    }
                }
                else if(tmpNode.ChildNodes.Count > 1)
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Extracts the description.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private string ExtractDescription(XmlNode node)
        {
            string result = string.Empty;

            try
            {
                if(node.Attributes != null)
                {
                    XmlAttribute attribute = node.Attributes[_descAttribute];
                    if(attribute != null)
                    {
                        result = attribute.Value;
                    }
                }
            }
            catch(Exception ex)
            {
                this.LogError(ex);
            }

            return result;
        }
    }
}

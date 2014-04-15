using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Dev2.Common;
using Dev2.Common.StringTokenizer.Interfaces;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Value_Objects;

namespace Dev2.Data.Util
{
    /// <summary>
    /// General DataList utility methods
    /// </summary>
    public static class DataListUtil
    {
        #region Class Members

        private static readonly HashSet<string> _sysTags = new HashSet<string>();
        const string _emptyTag = "<Empty />";
        const string _cdataStart = "<![CDATA[";
        const string _cdataEnd = "]]>";
        const string _adlRoot = "ADL";
        private static readonly string[] stripTags = { "<XmlData>", "</XmlData>", "<Dev2ServiceInput>", "</Dev2ServiceInput>", "<sr>", "</sr>", "<DataList>", "</DataList>", "<ADL />" };
        private static readonly string[] naughtyTags = { "<Dev2ResumeData>", "</Dev2ResumeData>", 
                                                         "<Dev2XMLResult>", "</Dev2XMLResult>", 
                                                         "<WebXMLConfiguration>", "</WebXMLConfiguration>", 
                                                         "<Dev2WebpartBindingData>", "</Dev2WebpartBindingData>", 
                                                         "<ActivityInput>", "</ActivityInput>", 
                                                         "<WebPart>", "</WebPart>",
                                                         "<ADL>","</ADL>",
                                                         "<DL>","</DL>"
                                                       };

        private static readonly XmlReaderSettings _isXmlReaderSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Auto, DtdProcessing = DtdProcessing.Ignore };

        #endregion Class Members

        #region Constructor

        static DataListUtil()
        {
            // build system tags
            foreach(Enum e in (Enum.GetValues(typeof(enSystemTag))))
            {
                _sysTags.Add(e.ToString());
            }
        }

        #endregion Constructor

        /// <summary>
        /// Replaces the index of the star with fixed.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        public static string ReplaceStarWithFixedIndex(string exp, int idx)
        {
            return idx > 0 ? exp.Replace("(*)", "(" + idx + ")") : exp;
        }

        /// <summary>
        /// Replaces the index of a recordset with a blank index.
        /// </summary>
        /// <param name="expression">The expession.</param>
        /// <returns></returns>
        public static string ReplaceRecordsetIndexWithBlank(string expression)
        {
            string extractIndexRegionFromRecordset = ExtractIndexRegionFromRecordset(expression);
            return expression.Replace(extractIndexRegionFromRecordset, "()");
        }

        /// <summary>
        /// Determines whether [is calc evaluation] [the specified expression].
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="newExpression">The new expression.</param>
        /// <returns>
        ///   <c>true</c> if [is calc evaluation] [the specified expression]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCalcEvaluation(string expression, out string newExpression)
        {
            bool result = false;

            newExpression = string.Empty;

            if(expression.StartsWith(GlobalConstants.CalculateTextConvertPrefix))
            {
                if(expression.EndsWith(GlobalConstants.CalculateTextConvertSuffix))
                {
                    newExpression = expression.Substring(GlobalConstants.CalculateTextConvertPrefix.Length, expression.Length - (GlobalConstants.CalculateTextConvertSuffix.Length + GlobalConstants.CalculateTextConvertPrefix.Length));
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Binds the environment variables.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <param name="rootServiceName">Name of the root service.</param>
        /// <returns></returns>
        public static string BindEnvironmentVariables(string transform, string rootServiceName = "")
        {
            if(string.IsNullOrEmpty(transform))
            {
                return transform;
            }

            transform = transform.Replace("@AppPath", Environment.CurrentDirectory);
            transform = transform.Replace("@ServiceName", rootServiceName);
            transform = transform.Replace("@OSVersion", Environment.OSVersion.VersionString);

            return transform;
        }

        /// <summary>
        /// Composes the into user visible recordset.
        /// </summary>
        /// <param name="rs">The rs.</param>
        /// <param name="idx">The idx.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public static string ComposeIntoUserVisibleRecordset(string rs, string idx, string field)
        {
            return string.Format("{0}({1}).{2}", rs, idx, field);
        }

        /// <summary>
        /// Composes the into user visible recordset.
        /// </summary>
        /// <param name="rs">The rs.</param>
        /// <param name="idx">The idx.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public static string ComposeIntoUserVisibleRecordset(string rs, int idx, string field)
        {
            return string.Format("{0}({1}).{2}", rs, idx, field);
        }

        /// <summary>
        /// Extracts the fixed data list.
        /// </summary>
        /// <param name="dataList">The data list.</param>
        /// <returns>Only the fixed data list items as a string</returns>
        public static string ExtractFixedDataList(string dataList)
        {
            string result = "<ADL>";

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(dataList);
            if(xDoc.HasChildNodes)
            {
                XmlNodeList nodeList = xDoc.FirstChild.SelectNodes(@"./*[@IsEditable = ""False""]");

                if(nodeList != null)
                {
                    result = nodeList.Cast<XmlNode>().Aggregate(result, (current, node) => current + node.OuterXml);
                }

                result += "</ADL>";
            }

            return result;
        }

        /// <summary>
        /// Extracts the editable data list.
        /// </summary>
        /// <param name="dataList">The data list.</param>
        /// <returns>Only the editable data list items as a string</returns>
        public static string ExtractEditableDataList(string dataList)
        {
            string result = string.Empty;

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(dataList);
            if(xDoc.HasChildNodes)
            {
                XmlNodeList nodeList = xDoc.FirstChild.SelectNodes(@"./*[@IsEditable = ""True""]");

                if(nodeList != null)
                {
                    result = nodeList.Cast<XmlNode>().Aggregate(result, (current, node) => current + node.OuterXml);
                }
            }
            return result;
        }

        /// <summary>
        /// Adds the missing from right.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="errors">The errors.</param>
        /// <exception cref="System.ArgumentNullException">right</exception>
        public static void MergeDataList(IBinaryDataList left, IBinaryDataList right, out ErrorResultTO errors)
        {

            if(right == null)
            {
                throw new ArgumentNullException("right");
            }

            if(left == null)
            {
                throw new ArgumentException("left");
            }

            errors = new ErrorResultTO();
            ErrorResultTO invokeErrors;
            MergeOp(left, right, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            MergeOp(right, left, out invokeErrors);
            errors.MergeErrors(invokeErrors);

        }

        private static void MergeOp(IBinaryDataList left, IBinaryDataList right, out ErrorResultTO errors)
        {
            IList<string> itemKeys = right.FetchAllUserKeys();
            errors = new ErrorResultTO();

            foreach(string key in itemKeys)
            {
                IBinaryDataListEntry entry;

                string error;
                if(!left.TryGetEntry(key, out entry, out error))
                {
                    // NOTE : DO NOT ADD ERROR, IT IS A MISS AND WE ACCOUNT FOR THIS BELOW

                    // Left does not contain key, get it from the right and add ;)
                    if(right.TryGetEntry(key, out entry, out error))
                    {
                        errors.AddError(error);
                        // we found it add it to the left ;)
                        if(entry.IsRecordset)
                        {
                            left.TryCreateRecordsetTemplate(entry.Namespace, entry.Description, entry.Columns, false,
                                                            true, out error);
                            errors.AddError(error);
                        }
                        else
                        {
                            left.TryCreateScalarTemplate(string.Empty, entry.Namespace, entry.Description, false,
                                                         out error);
                            errors.AddError(error);
                        }
                    }
                    else
                    {
                        errors.AddError(error);
                    }
                }
            }
        }


        /// <summary>
        /// Remove XMLData and other nesting junk from the ADL
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public static string StripCrap(string payload)
        {
            string result = payload;
            string[] veryNaughtyTags = naughtyTags;

            if(!string.IsNullOrEmpty(payload))
            {

                if(stripTags != null)
                {
                    stripTags
                        .ToList()
                        .ForEach(tag =>
                        {
                            result = result.Replace(tag, "");
                        });
                }

                if(veryNaughtyTags != null)
                {
                    result = CleanupNaughtyTags(veryNaughtyTags, result);
                }

                // we now need to remove non-valid chars from the stream

                int start = result.IndexOf("<", StringComparison.Ordinal);
                if(start >= 0)
                {
                    result = result.Substring((start));
                }

                if(result.Contains("<") && result.Contains(">"))
                {
                    if(!IsXml(result))
                    {
                        result = result.Replace(string.Concat("<", _adlRoot, ">"), string.Empty).Replace(string.Concat("</", _adlRoot, ">"), "");
                        result = string.Concat("<", _adlRoot, ">", result, "</", _adlRoot, ">");
                    }
                }

                // Finally remove the Webpart Tag
                // Dev2WebpartConfig
                result = result.Replace("<Dev2WebpartConfig>", "").Replace("</Dev2WebpartConfig>", "");
            }

            return result;
        }

        /// <summary>
        /// Builds the system tag for data list.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="addBrackets">if set to <c>true</c> [add brackets].</param>
        /// <returns></returns>
        public static string BuildSystemTagForDataList(enSystemTag tag, bool addBrackets)
        {

            string result = (GlobalConstants.SystemTagNamespace + "." + tag);

            if(addBrackets)
            {
                result = "[[" + result + "]]";
            }

            return result;
        }

        /// <summary>
        /// Builds the system tag for data list.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="addBrackets">if set to <c>true</c> [add brackets].</param>
        /// <returns></returns>
        public static string BuildSystemTagForDataList(string tag, bool addBrackets)
        {

            string result = (GlobalConstants.SystemTagNamespace + "." + tag);

            if(addBrackets)
            {
                result = "[[" + result + "]]";
            }

            return result;
        }

        /// <summary>
        /// Extracts the attribute.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        public static string ExtractAttribute(string payload, string tagName, string attribute)
        {
            string result = string.Empty;

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(payload);
            XmlNodeList nl = xDoc.GetElementsByTagName(tagName);
            if(nl.Count > 0)
            {
                var xmlAttributeCollection = nl[0].Attributes;
                if(xmlAttributeCollection != null)
                {
                    result = xmlAttributeCollection[attribute].Value;
                }
            }

            return result;
        }

        public static string ExtractAttributeFromTagAndMakeRecordset(string payload, string tagName, string attribute)
        {
            return ExtractAttributeFromTagAndMakeRecordset(payload, tagName, new[] { attribute }, null);
        }

        /// <summary>
        /// Extracts the attribute from tag and make recordset.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="childTags">The child tags.</param>
        /// <returns></returns>
        public static string ExtractAttributeFromTagAndMakeRecordset(string payload, string tagName, string[] attribute, string[] childTags)
        {

            StringBuilder result = new StringBuilder(MakeOpenTag(_adlRoot));

            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(payload);
                XmlNodeList nl = xDoc.GetElementsByTagName(tagName);
                foreach(XmlNode n in nl)
                {
                    result.Append(MakeOpenTag(tagName));
                    foreach(string atr in attribute)
                    {
                        if(n.Attributes != null)
                        {
                            string attrValue = n.Attributes[atr].Value;
                            result.Append(string.Concat(MakeOpenTag(atr), attrValue, MakeCloseTag(atr)));
                        }
                    }
                    // now fetch the extra tag names ;)
                    if(childTags != null)
                    {
                        foreach(string tag in childTags)
                        {
                            string innerXML = n.InnerXml;
                            // we have a match!
                            int idx = innerXML.IndexOf(MakeOpenTag(tag), StringComparison.Ordinal);
                            if(idx >= 0)
                            {
                                // we have a match ;)
                                int end = innerXML.IndexOf(MakeCloseTag(tag), StringComparison.Ordinal);
                                int properStart = (idx + MakeOpenTag(tag).Length);
                                string val = innerXML.Substring(properStart, (end - properStart));
                                result.Append(string.Concat(MakeOpenTag(tag), val, MakeCloseTag(tag)));
                            }
                        }
                    }

                    result.Append(MakeCloseTag(tagName));
                }

            }
            catch(Exception ex)
            {
                ServerLogger.LogError("DataListUtil", ex);
                //TODO, EMPTY CATCH, Please add reasoning
            }

            result.Append(MakeCloseTag(_adlRoot));

            return result.ToString();
        }

        /// <summary>
        /// Removes a specified node for the ambient data list.
        /// </summary>
        /// <param name="adl">The ambient data list.</param>
        /// <param name="nodeToRemove">The node to remove.</param>
        /// <param name="evalNode">The eval node.</param>
        /// <returns></returns>
        public static string UpsertCleaning(string adl, string nodeToRemove, string evalNode)
        {
            string result = Regex.Replace(adl, @">\r\n*<", "><", RegexOptions.Singleline);
            // ReSharper disable RedundantAssignment
            result = Regex.Replace(result, @">\n*<", "><", RegexOptions.Singleline);
            // ReSharper restore RedundantAssignment
            result = Regex.Replace(adl, @">\r*<", "><", RegexOptions.Singleline);

            result = result.Replace(MakeOpenTag(nodeToRemove), "").Replace(MakeCloseTag(nodeToRemove), "").Replace(MakeSingleTag(nodeToRemove), "");

            bool isFragment;
            if(IsXml(result, out isFragment))
            {
                if(isFragment)
                {
                    result = string.Concat(MakeOpenTag(_adlRoot), result, MakeCloseTag(_adlRoot));
                }
                else
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(result);
                    if(xDoc.DocumentElement != null && xDoc.DocumentElement.Name == StripBracketsFromValue(evalNode))
                    {
                        result = string.Concat(MakeOpenTag(_adlRoot), result, MakeCloseTag(_adlRoot));
                    }
                }
            }
            else
            {
                result = string.Concat(MakeOpenTag(_adlRoot), result, MakeCloseTag(_adlRoot));
            }

            return result;
        }

        /// <summary>
        /// Makes the open tag.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public static string MakeOpenTag(string node)
        {
            return string.Concat("<", node, ">");
        }

        /// <summary>
        /// Makes the close tag.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public static string MakeCloseTag(string node)
        {
            return string.Concat("</", node, ">");
        }

        /// <summary>
        /// Makes a single tag.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public static string MakeSingleTag(string node)
        {
            return string.Concat("<", node, " />");
        }

        /// <summary>
        /// Used to detect the #text and #cdate-section 'nodes' returned by MS XML parser
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsMSXmlBugNode(string value)
        {
            bool result = value == "#text" || value == "#cdata-section";

            return result;
        }

        /// <summary>
        /// Used to strip newlines and white space be
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FlattenIntoSingleString(string value)
        {
            string tmp = value.Replace("\n", "").Replace("\r", "");
            tmp = Regex.Replace(tmp, @">\s*<", "><", RegexOptions.Singleline);
            return tmp;
        }

        /// <summary>
        /// Used to check and see if the ADL is empty
        /// </summary>
        /// <param name="dataList"></param>
        /// <returns></returns>
        public static bool isNullADL(string dataList)
        {
            bool result = false;

            if(dataList == null)
            {
                result = true;
            }
            else
            {
                if(dataList == _emptyTag)
                {
                    result = true;
                }
                else
                {
                    try
                    {
                        XmlDocument x = new XmlDocument();
                        x.LoadXml(dataList);

                        XmlNode xn = x.FirstChild;
                        XmlNodeList xnl = xn.ChildNodes;
                        if(xnl.Count == 0)
                        {
                            result = true;
                        }
                    }
                    catch(Exception ex)
                    {
                        ServerLogger.LogError("DataListUtil", ex);
                        result = true;
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///  Used to detect if input defs are null
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static bool isNullInput(string inputs)
        {
            bool result = false;

            if(inputs == null)
            {
                result = true;
            }
            else
            {

                try
                {
                    XmlDocument x = new XmlDocument();
                    x.LoadXml(inputs);

                    XmlNode xn = x.FirstChild;
                    XmlNodeList xnl = xn.ChildNodes;
                    if(xnl.Count == 0)
                    {
                        result = true;
                    }
                }
                catch(Exception ex)
                {
                    ServerLogger.LogError("DataListUtil", ex);
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Used to remove &lt; and &gt; from HTML data
        /// </summary>
        /// <param name="HTML"></param>
        /// <returns></returns>
        public static string RemoveHTMLEncoding(string HTML)
        {
            string resultHTML = HTML.Replace("&amp;lt;", "<").Replace("&amp;gt;", ">");
            return resultHTML;
        }

        /// <summary>
        /// Used to 
        /// </summary>
        /// <param name="region"></param>
        /// <param name="adl"></param>
        /// <returns></returns>
        public static string ExtractDataBetweenRegion(string region, string adl)
        {
            string result = string.Concat("Extraction Error For [ ", region, " ]");

            string startTag = string.Concat("<", region, ">");
            string endTag = string.Concat("</", region, ">");

            int start = adl.IndexOf(startTag, StringComparison.Ordinal);

            if(start >= 0)
            {
                int end = adl.IndexOf(endTag, StringComparison.Ordinal);

                if(end > start)
                {
                    int realStart = start + startTag.Length;
                    int len = (end - realStart);
                    result = adl.Substring(realStart, len);

                }
            }

            return result;
        }

        /// <summary>
        /// Removes the brackets.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public static string RemoveLanguageBrackets(string val)
        {
            return val.Replace("[", string.Empty).Replace("]", string.Empty);
        }

        /// <summary>
        /// Wrap up any text into a CDATA region
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string CDATAWrapText(string data)
        {
            return (string.Concat(_cdataStart, data, _cdataEnd));
        }

        /// <summary>
        /// Unwrap any text from a CDATA region
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string CDATAUnwrapText(string data)
        {
            data = data.Replace("&lt;", "<").Replace("&gt;", ">");
            return (data.Replace(_cdataStart, "").Replace(_cdataEnd, ""));
        }

        /// <summary>
        /// Wrap HTML in a CDATA region if fragment or FormView
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string CDATAWrapHTML(string field, string value)
        {
            string result = value;

            if(field.ToLower() == "fragment" || field.Equals("FormView"))
            {
                // value.Replace("<", "&lt;").Replace(">", "&gt;")
                if(!value.Contains(_cdataStart))
                {
                    result = string.Concat(_cdataStart, value, _cdataEnd);
                }
            }

            return result;
        }

        /// <summary>
        /// Remove CDATA regions wrapping with concerns for HTML
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string CDATAUnwrapHTML(string value)
        {
            string result = value;
            result = result.Replace("&lt;", "<").Replace("&gt;", ">");
            result = result.Replace("&amp;lt;", "<").Replace("&amp;gt;", ">");
            while(result.Contains("<![CDATA"))
            {
                result = result.Replace(_cdataStart, "");
                result = result.Replace(_cdataEnd, "");
            }
            return result;
        }

        /// <summary>
        /// Used to determin if a tag is a system tag or not
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool IsSystemTag(string tag)
        {

            // Nasty junk that has been carried!
            string[] nastyJunk = { "WebServerUrl", "Dev2WebServer", "PostData", "Service" };

            // Transfer System Tags
            bool result = _sysTags.Contains(tag) || nastyJunk.Contains(tag);

            if(!result && tag.StartsWith(GlobalConstants.SystemTagNamespaceSearch))
            {
                tag = tag.Replace(GlobalConstants.SystemTagNamespaceSearch, "");
                result = _sysTags.Contains(tag) || nastyJunk.Contains(tag);
            }

            return result;
        }

        /// <summary>
        /// Generates the data list from defs.
        /// </summary>
        /// <param name="defs">The defs.</param>
        /// <param name="withData">if set to <c>true</c> [with data].</param>
        /// <returns></returns>
        public static string GenerateDataListFromDefs(IList<IDev2Definition> defs, bool withData = false)
        {
            StringBuilder result = new StringBuilder("<" + _adlRoot + ">");

            Dictionary<string, string> rsMap = new Dictionary<string, string>();

            defs
                .ToList()
                .ForEach(d =>
                {

                    if(d.IsRecordSet)
                    {
                        string tmp = string.Empty;

                        if(rsMap.Keys.Contains(d.RecordSetName))
                        {
                            tmp = rsMap[d.RecordSetName];
                        }
                        string _name;
                        if(d.Name.Contains("."))
                        {
                            _name = d.Name.Split('.')[1];
                        }
                        else
                        {
                            _name = d.Name;
                        }
                        if(withData)
                        {
                            tmp = string.Concat(tmp, Environment.NewLine, "<", _name, ">", d.RawValue, "</", _name, ">");
                        }
                        else
                        {
                            tmp = string.Concat(tmp, Environment.NewLine, "<", _name, "/>");
                        }


                        rsMap[d.RecordSetName] = tmp;
                    }
                    else
                    {
                        if(withData)
                        {
                            result.Append(string.Concat("<", d.Name, ">", d.RawValue, "</", d.Name, ">"));
                        }
                        else
                        {
                            result.Append(string.Concat("<", d.Name, "/>"));
                        }
                    }

                });

            rsMap
                .ToList()
                .ForEach(rs =>
                {
                    result.Append(Environment.NewLine);
                    result.Append(string.Concat("<", rs.Key, ">"));
                    result.Append(Environment.NewLine);
                    result.Append(rs.Value);
                    result.Append(Environment.NewLine);
                    result.Append(string.Concat("</", rs.Key, ">"));

                });

            result.Append(Environment.NewLine);
            result.Append("</" + _adlRoot + ">");

            return result.ToString();
        }

        /// <summary>
        /// Shapes the definitions to data list.
        /// </summary>
        /// <param name="defs">The defs.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public static string ShapeDefinitionsToDataList(IList<IDev2Definition> defs, enDev2ArgumentType typeOf, out ErrorResultTO errors)
        {
            StringBuilder result = new StringBuilder();

            bool isInput = false;
            errors = new ErrorResultTO();

            if(typeOf == enDev2ArgumentType.Input)
            {
                isInput = true;
            }

            if(defs == null || defs.Count == 0)
            {
                errors.AddError(string.Concat("could not locate any data of type [ ", typeOf, " ]"));
            }
            else
            {

                IRecordSetCollection recCol = DataListFactory.CreateRecordSetCollection(defs, (isInput));
                IList<IDev2Definition> scalarList = DataListFactory.CreateScalarList(defs, (isInput));

                // open datashape
                result.Append(string.Concat("<", _adlRoot, ">"));
                result.Append(Environment.NewLine);

                // append scalar shape
                result.Append(BuildDev2ScalarShape(scalarList, isInput));
                // append record set shape
                result.Append(BuildDev2RecordSetShape(recCol, isInput));

                // close datashape
                result.Append(Environment.NewLine);
                result.Append(string.Concat("</", _adlRoot, ">"));
            }

            return result.ToString();
        }

        /// <summary>
        /// Shapes the definitions to data list.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="flipGeneration">if set to <c>true</c> [flip generation].</param>
        /// <returns></returns>
        public static string ShapeDefinitionsToDataList(string arguments, enDev2ArgumentType typeOf, out ErrorResultTO errors, bool flipGeneration = false)
        {
            StringBuilder result = new StringBuilder();
            IList<IDev2Definition> defs = null;
            bool isInput = false;
            errors = new ErrorResultTO();

            if(typeOf == enDev2ArgumentType.Output)
            {
                defs = DataListFactory.CreateOutputParser().Parse(arguments);
            }
            else if(typeOf == enDev2ArgumentType.Input)
            {
                defs = DataListFactory.CreateInputParser().Parse(arguments);
                isInput = true;
            }

            if(defs == null)
            {
                errors.AddError(string.Concat("could not locate any data of type [ ", typeOf, " ]"));
            }
            else
            {

                IRecordSetCollection recCol = DataListFactory.CreateRecordSetCollection(defs, !(isInput));
                IList<IDev2Definition> scalarList = DataListFactory.CreateScalarList(defs, !(isInput));

                // open datashape
                result.Append(string.Concat("<", _adlRoot, ">"));
                result.Append(Environment.NewLine);

                // do we want to do funky things ?!
                if(flipGeneration)
                {
                    isInput = true;
                }

                // append scalar shape
                result.Append(BuildDev2ScalarShape(scalarList, isInput));
                // append record set shape
                result.Append(BuildDev2RecordSetShape(recCol, isInput));

                // close datashape
                result.Append(Environment.NewLine);
                result.Append(string.Concat("</", _adlRoot, ">"));
            }

            return result.ToString();
        }


        /// <summary>
        /// Determines whether the value is a recordset.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [value is recordset] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValueRecordset(string value)
        {
            bool result = false;

            if(!string.IsNullOrEmpty(value))
            {
                if(value.Contains("(") && value.Contains(")"))
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Evaluates if an expression is a root level evaluated variable ie [[x]]
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool isRootVariable(string expression)
        {
            bool result = true;

            if(expression == null)
            {
                return false;
            }

            string[] openParts = Regex.Split(expression, @"\[\[");
            string[] closeParts = Regex.Split(expression, @"\]\]");

            //2013.05.31: Ashley lewis QA feedback on bug 9379 - count the number of opening and closing braces, they must both be more than one
            if(expression.Contains("[[") && expression.Contains("]]") && openParts.Count() == closeParts.Count() && openParts.Count() > 2 && closeParts.Count() > 2)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Used to extract a recordset name from a string as per the Dev2 data language spec
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns></returns>
        public static string ExtractRecordsetNameFromValue(string value)
        {
            if(value == null)
            {
                return string.Empty;
            }

            value = StripBracketsFromValue(value);
            string result = string.Empty;

            int openBracket = value.IndexOf("(", StringComparison.Ordinal);
            if(openBracket > 0)
            {
                result = value.Substring(0, openBracket);
            }

            return result;
        }

        /// <summary>
        /// Used to extract a field name from our recordset notation
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns></returns>
        public static string ExtractFieldNameFromValue(string value)
        {
            string result = string.Empty;
            value = StripBracketsFromValue(value);
            int dotIdx = value.IndexOf(".", StringComparison.Ordinal);
            if(dotIdx > 0)
            {
                result = value.Substring((dotIdx + 1));
            }

            return result;
        }

        /// <summary>
        /// Remove [[ ]] from a value if present
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns></returns>
        public static string StripBracketsFromValue(string value)
        {
            string result = string.Empty;
            if(value != null)
            {
                result = value.Replace("[[", "").Replace("]]", "");
            }

            return result;
        }


        /// <summary>
        /// Strips the leading and trailing brackets from value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string StripLeadingAndTrailingBracketsFromValue(string value)
        {
            string result = value;

            if(result.StartsWith("[["))
            {
                result = result.Substring(2, (result.Length - 2));
            }

            if(result.EndsWith("]]"))
            {
                result = result.Substring(0, (result.Length - 2));
            }

            return result;
        }

        /// <summary>
        /// Adds [[ ]] to a variable if they are not present already
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string AddBracketsToValueIfNotExist(string value)
        {
            string result;

            if(!value.Contains("]]"))
            {
                // missing both
                if(!value.Contains("[["))
                {
                    result = string.Concat("[[", value, "]]");
                }
                else
                {
                    // only ending brackets ;)
                    result = string.Concat(value, "]]");
                }
            }
            else
            {
                result = value;
            }

            return result;
        }

        /// <summary>
        /// Adds () to the end of the value
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="starNotation">if set to <c>true</c> [star notation].</param>
        /// <returns></returns>
        public static string MakeValueIntoHighLevelRecordset(string value, bool starNotation = false)
        {
            var inject = "()";

            if(starNotation)
            {
                inject = "(*)";
            }

            string result = StripBracketsFromValue(value);

            if(result.EndsWith("("))
            {
                result = string.Concat(result, ")");
            }
            else if(result.EndsWith(")"))
            {
                return result.Replace(")", inject);
            }
            else if(!result.EndsWith("()"))
            {
                result = string.Concat(result, inject);
            }
            return result;
        }

        /// <summary>
        /// Used to extract an index in the recordset notation
        /// </summary>
        /// <param name="rs">The rs.</param>
        /// <returns></returns>
        public static string ExtractIndexRegionFromRecordset(string rs)
        {
            string result = string.Empty;

            int start = rs.IndexOf("(", StringComparison.Ordinal);
            if(start > 0)
            {
                int end = rs.LastIndexOf(")", StringComparison.Ordinal);
                if(end < 0)
                {
                    end = rs.Length;
                }

                start += 1;
                result = rs.Substring(start, (end - start));
            }

            return result;
        }


        /// <summary>
        /// Is the expression evaluated
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        ///   <c>true</c> if the specified payload is evaluated; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEvaluated(string payload)
        {
            bool result = payload.IndexOf("[[", StringComparison.Ordinal) >= 0;

            return result;
        }

        /// <summary>
        /// Used to calculate design time support requirements, mainly webpart wizards
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="parentServiceName">Name of the parent service.</param>
        /// <returns></returns>
        public static bool RequiresDesignTimeBindingSupport(string serviceName, string parentServiceName)
        {
            bool result = serviceName.ToLower().EndsWith(".wiz") || parentServiceName.ToLower().EndsWith(".wiz");

            return result;
        }

        /// <summary>
        /// Strip the () from a recordset
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ConvertRecordsetValueToXmlElement(string value)
        {
            string result = string.Empty;

            int start = value.IndexOf("(", StringComparison.Ordinal);

            if(start > 0)
            {
                result = value.Substring(0, start);
            }

            return result;
        }

        /// <summary>
        /// Determines whether [has resume tags already] [the specified shaped data list].
        /// </summary>
        /// <param name="shapedDataList">The shaped data list.</param>
        /// <returns>
        ///   <c>true</c> if [has resume tags already] [the specified shaped data list]; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasResumeTagsAlready(XmlDocument shapedDataList)
        {
            bool result = true;
            // check for current resume region, if present, ignore addition
            var tmp = shapedDataList.GetElementsByTagName(enSystemTag.Dev2ResumeData.ToString());
            if(tmp.Count == 0)
            {
                tmp = shapedDataList.GetElementsByTagName(enSystemTag.Resumption.ToString());
                if(tmp.Count == 0)
                {
                    result = false;
                }
            }

            return result;
        }


        /// <summary>
        /// Gets the type of the recordset index.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        public static enRecordsetIndexType GetRecordsetIndexTypeRaw(string idx)
        {
            enRecordsetIndexType result = enRecordsetIndexType.Error;

            if(idx == "*")
            {
                result = enRecordsetIndexType.Star;
            }
            else if(string.IsNullOrEmpty(idx))
            {
                result = enRecordsetIndexType.Blank;
            }
            else
            {
                try
                {
                    // ReSharper disable ReturnValueOfPureMethodIsNotUsed
                   
                    Convert.ToInt32(idx);
                    // ReSharper restore ReturnValueOfPureMethodIsNotUsed
                    result = enRecordsetIndexType.Numeric;
                }
                catch(Exception ex)
                {
                    ServerLogger.LogError("DataListUtil", ex);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the index type of a recorset
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static enRecordsetIndexType GetRecordsetIndexType(string expression)
        {
            enRecordsetIndexType result = enRecordsetIndexType.Error;

            string idx = ExtractIndexRegionFromRecordset(expression);
            if(idx == "*")
            {
                result = enRecordsetIndexType.Star;
            }
            else if(string.IsNullOrEmpty(idx))
            {
                result = enRecordsetIndexType.Blank;
            }
            else
            {
                int convertIntTest;
                if(Int32.TryParse(idx, out convertIntTest))
                {
                    result = enRecordsetIndexType.Numeric;
                }
            }

            return result;
        }

        /// <summary>
        /// Replace a single node in a XML document
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns></returns>
        public static string ReplaceXmlNode(string payload, string tagName, string newValue)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(payload);
            XmlNode node = xDoc.SelectSingleNode(string.Concat("//", tagName));
            if(node != null && node.ParentNode != null)
            {
                node.ParentNode.RemoveChild(node);
            }
            if(xDoc.DocumentElement != null)
            {
                xDoc.DocumentElement.InnerXml = string.Concat(xDoc.DocumentElement.InnerXml, newValue);
            }

            return xDoc.OuterXml;
        }

        //used in the replace node method
        private static readonly HashSet<char> _base64Characters = new HashSet<char>
            { 
    'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 
    'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 
    'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 
    'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '/', 
    '='
};

        /// <summary>
        /// Check if a string is a base64 string
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool IsBase64String(string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                return false;
            }
            if(value.Any(c => !_base64Characters.Contains(c)))
            {
                return false;
            }

            try
            {
                // ReSharper disable ReturnValueOfPureMethodIsNotUsed
                Convert.FromBase64String(value);
                // ReSharper restore ReturnValueOfPureMethodIsNotUsed
                return true;
            }
            catch(FormatException fex)
            {
                ServerLogger.LogError("DataListUtil", fex);
                return false;
            }
        }

        /// <summary>
        /// Checks if the info contained in data is well formed XML
        /// </summary>
        public static bool IsXml(string data)
        {
            bool isFragment;
            bool isHtml;
            var isXml = IsXml(data, out isFragment, out isHtml);
            return isXml && !isFragment && !isHtml;
        }

        /// <summary>
        /// Checks if the info contained in data is well formed XML
        /// </summary>
        public static bool IsXml(string data, out bool isFragment)
        {
            bool isHtml;
            return IsXml(data, out isFragment, out isHtml) && !isFragment && !isHtml;
        }

        /// <summary>
        /// Checks if the info contained in data is well formed XML
        /// </summary>
        public static bool IsXml(string data, out bool isFragment, out bool isHtml)
        {
            string trimedData = data.Trim();
            bool result = (trimedData.StartsWith("<") && !trimedData.StartsWith("<![CDATA["));

            isFragment = false;
            isHtml = false;

            if(result)
            {
                using(TextReader tr = new StringReader(trimedData))
                {
                    using(XmlReader reader = XmlReader.Create(tr, _isXmlReaderSettings))
                    {

                        try
                        {
                            long nodeCount = 0;
                            while(reader.Read() && !isHtml && !isFragment && reader.NodeType != XmlNodeType.Document)
                            {
                                nodeCount++;

                                if(reader.NodeType != XmlNodeType.CDATA)
                                {
                                    if(reader.NodeType == XmlNodeType.Element && reader.Name.ToLower() == "html" && reader.Depth == 0)
                                    {
                                        isHtml = true;
                                        result = false;
                                    }

                                    if(reader.NodeType == XmlNodeType.Element && nodeCount > 1 && reader.Depth == 0)
                                    {
                                        isFragment = true;
                                    }
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            ServerLogger.LogError("DataListUtil", ex);
                            tr.Close();
                            reader.Close();
                            isFragment = false;
                            result = false;
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Returns all of the possible expression combinations of recordset(*) data
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="currentDataList">The current data list.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public static IList<string> GetAllPossibleExpressionsForFunctionOperations(string expression, Guid currentDataList, out ErrorResultTO errors)
        {
            IList<string> result = new List<string>();
            IDev2IteratorCollection colItr = Dev2ValueObjectFactory.CreateIteratorCollection();
            IBinaryDataListEntry Entry = DataListFactory.CreateDataListCompiler().Evaluate(currentDataList, enActionType.User, expression, false, out errors);
            IDev2DataListEvaluateIterator expressionIterator = Dev2ValueObjectFactory.CreateEvaluateIterator(Entry);
            colItr.AddIterator(expressionIterator);

            while(colItr.HasMoreData())
            {
                result.Add(colItr.FetchNextRow(expressionIterator).TheValue);
            }
            return result;
        }

        /// <summary>
        /// Gets the regions from expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static IList<string> GetRegionsFromExpression(string expression)
        {
            // Retrieve all the regions from an expression
            const string openRegion = "[[";
            const string closeRegion = "]]";
            StringBuilder expressionBuilder = new StringBuilder();
            expressionBuilder.Append(expression.Substring(expression.IndexOf(openRegion, StringComparison.Ordinal), expression.LastIndexOf(closeRegion, StringComparison.Ordinal) - expression.IndexOf(openRegion, StringComparison.Ordinal)));
            string expressionString = (expressionBuilder.ToString().Remove(0, 2));
            expressionString = expressionString.Remove(expressionString.Length - 2, 2);
            expressionBuilder.Clear().Append(expressionString);
            // find the text before the next open region
            List<string> regions = new List<string> { expressionBuilder.ToString().Substring(0, expressionBuilder.ToString().IndexOf(openRegion, StringComparison.Ordinal)) };
            // if there are still regions
            if(expressionBuilder.ToString().Contains(openRegion) && expressionBuilder.ToString().Contains(closeRegion))
            {
                regions.AddRange(GetRegionsFromExpression(expressionBuilder.ToString()));
            }
            else
            {
                return regions;
            }
            return regions;
        }

        /// <summary>
        /// Adjusts for encoding issues.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public static string AdjustForEncodingIssues(string payload)
        {
            string trimedData = payload.Trim();
            var isXML = (trimedData.StartsWith("<") && !trimedData.StartsWith("<![CDATA["));

            if(!isXML)
            {
                // we need to adjust. there might be a silly encoding issue with first char!
                if(trimedData.Length > 1 && trimedData[1] == '<' && trimedData[2] == '?')
                {
                    trimedData = trimedData.Substring(1);
                }
                else if(trimedData.Length > 2 && trimedData[2] == '<' && trimedData[3] == '?')
                {
                    trimedData = trimedData.Substring(2);
                }
                else if(trimedData.Length > 3 && trimedData[3] == '<' && trimedData[4] == '?')
                {
                    trimedData = trimedData.Substring(3);
                }
            }

            return trimedData;
        }

        /// <summary>
        /// Build a scalar shape
        /// </summary>
        /// <param name="scalarList">The scalar list.</param>
        /// <param name="isInput">if set to <c>true</c> [is input].</param>
        /// <returns></returns>
        private static string BuildDev2ScalarShape(IEnumerable<IDev2Definition> scalarList, bool isInput)
        {
            StringBuilder result = new StringBuilder();

            foreach(IDev2Definition def in scalarList)
            {
                if(!isInput)
                {
                    if(IsEvaluated(def.RawValue))
                    {
                        result.Append(string.Concat("<", def.Value, "></", def.Value, ">"));
                        result.Append(Environment.NewLine);
                    }

                    if(!string.IsNullOrEmpty(def.Name))
                    {
                        result.Append(string.Concat("<", def.Name, "></", def.Name, ">"));
                        result.Append(Environment.NewLine);
                    }
                }
                else
                {
                    if(!string.IsNullOrEmpty(def.Name))
                    {
                        result.Append(string.Concat("<", def.Name, "></", def.Name, ">"));
                        result.Append(Environment.NewLine);
                    }

                    // we need to process the RawValue field incase it is not in the recordsets ;)
                    var rsName = ExtractRecordsetNameFromValue(def.Value);
                    if(string.IsNullOrEmpty(rsName) && IsEvaluated(def.Value))
                    {
                        var tmpValue = RemoveLanguageBrackets(def.Value);
                        result.Append(string.Concat("<", tmpValue, "></", tmpValue, ">"));
                        result.Append(Environment.NewLine);
                    }
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Cleanups the naughty tags.
        /// </summary>
        /// <param name="toRemove">To remove.</param>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        private static string CleanupNaughtyTags(string[] toRemove, string payload)
        {
            bool foundOpen = false;
            string result = payload;

            for(int i = 0; i < toRemove.Length; i++)
            {
                string myTag = toRemove[i];
                if(myTag.IndexOf("<", StringComparison.Ordinal) >= 0 && myTag.IndexOf("</", StringComparison.Ordinal) < 0)
                {
                    foundOpen = true;
                }
                else if(myTag.IndexOf("</", StringComparison.Ordinal) >= 0)
                {
                    // close tag
                    if(foundOpen)
                    {
                        // remove data between
                        int loc = i - 1;
                        if(loc >= 0)
                        {
                            int start = result.IndexOf(toRemove[loc], StringComparison.Ordinal);
                            int end = result.IndexOf(myTag, StringComparison.Ordinal);
                            if(start < end && start >= 0)
                            {
                                string canidate = result.Substring(start, ((end - start) + myTag.Length));
                                string tmpResult = canidate.Replace(myTag, "").Replace(toRemove[loc], "");
                                if(tmpResult.IndexOf("</", StringComparison.Ordinal) >= 0 || tmpResult.IndexOf("/>", StringComparison.Ordinal) >= 0)
                                {
                                    // replace just the tags
                                    result = result.Replace(myTag, "").Replace(toRemove[loc], "");
                                }
                                else
                                {
                                    // replace any tag and it's contents as long as it is not XML in side
                                    result = result.Replace(canidate, "");
                                }
                            }
                        }
                    }
                    else
                    {
                        result = result.Replace(myTag, "");
                    }

                    foundOpen = false;
                }
            }

            return result.Trim();

        }

        /// <summary>
        /// Build a recordset shape
        /// </summary>
        /// <param name="recCol"></param>
        /// <param name="isInput"></param>
        /// <returns></returns>
        private static string BuildDev2RecordSetShape(IRecordSetCollection recCol, bool isInput)
        {
            StringBuilder result = new StringBuilder();

            IList<IRecordSetDefinition> defs = recCol.RecordSets;
            HashSet<string> processedSetNames = new HashSet<string>();
            foreach(IRecordSetDefinition tmp in defs)
            {
                IList<string> postProcessDefs = new List<string>();
                // get DL recordset Name
                if(tmp.Columns.Count > 0)
                {
                    string setName = tmp.SetName;
                    result.Append(string.Concat("<", setName, ">"));
                    result.Append(Environment.NewLine);

                    processedSetNames.Add(setName);

                    IList<IDev2Definition> cols = tmp.Columns;
                    foreach(IDev2Definition tmpDef in cols)
                    {
                        if(isInput)
                        {
                            var col = ExtractFieldNameFromValue(tmpDef.MapsTo);

                            if(!string.IsNullOrEmpty(col))
                            {
                                var toAppend = ("\t<" + col + "></" + col + ">");
                                result.Append(toAppend);
                            }

                        }
                        else
                        {
                            //Name
                            string tag = ExtractFieldNameFromValue(tmpDef.Name);

                            if(string.IsNullOrEmpty(tag))
                            {
                                //Name
                                tag = tmpDef.Name;
                            }

                            result.Append(string.Concat("\t<", tag, "></", tag, ">"));

                        }
                        result.Append(Environment.NewLine);
                    }
                    result.Append(string.Concat("</", setName, ">"));
                    result.Append(Environment.NewLine);

                    //  Process post append data ;)
                    foreach(var col in postProcessDefs)
                    {
                        result.Append(col);
                        result.Append(Environment.NewLine);
                    }
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Removes the recordset brackets from a value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string RemoveRecordsetBracketsFromValue(string value)
        {
            return value.Replace("()", "");
        }

        /// <summary>
        /// Gets the value at an index.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="index">The index.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public static string GetValueAtIndex(IBinaryDataListEntry entry, int index, out string error)
        {
            error = string.Empty;
            string result;
            if(entry.IsRecordset)
            {
                result = entry.TryFetchIndexedRecordsetUpsertPayload(index, out error).TheValue;
            }
            else
            {
                result = entry.FetchScalar().TheValue;
            }
            return result;
        }

        /// <summary>
        /// Makes the data list fixed.
        /// </summary>
        /// <param name="datalist">The datalist.</param>
        /// <returns>The datalist string with all variables fixed/not editable</returns>
        public static string MakeDataListFixed(string datalist)
        {
            string result = datalist;

            if(!string.IsNullOrEmpty(datalist))
            {

                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(datalist);
                foreach(XmlNode root in xDoc.ChildNodes)
                {
                    if(root.HasChildNodes)
                    {
                        foreach(XmlNode node in root.ChildNodes)
                        {
                            foreach(XmlNode childnode in node.ChildNodes)
                            {
                                XmlAttribute editableAtt3 = xDoc.CreateAttribute("IsEditable");
                                editableAtt3.Value = "False";
                                if(childnode.Attributes != null)
                                {
                                    childnode.Attributes.Append(editableAtt3);
                                }
                            }
                            XmlAttribute editableAtt = xDoc.CreateAttribute("IsEditable");
                            editableAtt.Value = "False";
                            if(node.Attributes != null)
                            {
                                node.Attributes.Append(editableAtt);
                            }
                        }
                    }
                }
                result = xDoc.OuterXml;
            }
            return result;
        }

        /// <summary>
        /// Extracts the input definitions from a service definition.
        /// </summary>
        /// <param name="serviceDefintion">The service definition.</param>
        /// <returns></returns>
        public static string ExtractInputDefinitionsFromServiceDefinition(string serviceDefintion)
        {
            string result = string.Empty;
            if(!string.IsNullOrEmpty(serviceDefintion))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(serviceDefintion);
                var selectSingleNode = xDoc.SelectSingleNode("//Inputs");
                if(selectSingleNode != null)
                {
                    result = selectSingleNode.OuterXml;
                }
            }
            return result;
        }


        /// <summary>
        /// Extracts the output definitions from a service definition.
        /// </summary>
        /// <param name="serviceDefintion">The service definition.</param>
        /// <returns></returns>
        public static string ExtractOutputDefinitionsFromServiceDefinition(string serviceDefintion)
        {
            string result = string.Empty;
            if(!string.IsNullOrEmpty(serviceDefintion))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(serviceDefintion);
                var selectSingleNode = xDoc.SelectSingleNode("//Outputs");
                if(selectSingleNode != null)
                {
                    result = selectSingleNode.OuterXml;
                }
            }
            return result;
        }

        /// <summary>
        /// Creates a recordset display value.
        /// </summary>
        /// <param name="recsetName">Name of the recordset.</param>
        /// <param name="colName">Name of the column.</param>
        /// <param name="indexNum">The index number.</param>
        /// <returns></returns>
        public static string CreateRecordsetDisplayValue(string recsetName, string colName, string indexNum)
        {
            return string.Concat(recsetName, "(", indexNum, ").", colName);
        }

        /// <summary>
        /// Ecodes the region brackets in Html.
        /// </summary>
        /// <param name="stringToEncode">The string to encode.</param>
        /// <returns></returns>
        public static string HtmlEncodeRegionBrackets(string stringToEncode)
        {
            return stringToEncode.Replace("[", "&#91;").Replace("]", "&#93;");
        }

        /// <summary>
        /// Upserts the tokens.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="tokenizer">The tokenizer.</param>
        /// <param name="tokenPrefix">The token prefix.</param>
        /// <param name="tokenSuffix">The token suffix.</param>
        /// <param name="removeEmptyEntries">if set to <c>true</c> [remove empty entries].</param>
        /// <exception cref="System.ArgumentNullException">target</exception>
        public static void UpsertTokens(Collection<ObservablePair<string, string>> target, IDev2Tokenizer tokenizer, string tokenPrefix = null, string tokenSuffix = null, bool removeEmptyEntries = true)
        {
            if(target == null)
            {
                throw new ArgumentNullException("target");
            }

            target.Clear();

            if(tokenizer == null)
            {
                return;
            }

            var newTokens = new List<string>();
            while(tokenizer.HasMoreOps())
            {
                var token = tokenizer.NextToken();
                if(string.IsNullOrEmpty(token))
                {
                    if(!removeEmptyEntries)
                    {
                        target.Add(new ObservablePair<string, string>(string.Empty, string.Empty));
                    }
                }
                else
                {
                    token = AddBracketsToValueIfNotExist(string.Format("{0}{1}{2}", tokenPrefix, StripLeadingAndTrailingBracketsFromValue(token), tokenSuffix));
                    newTokens.Add(token);
                    target.Add(new ObservablePair<string, string>(token, string.Empty));
                }
            }

            foreach(var observablePair in target)
            {
                observablePair.Key = observablePair.Key.Replace(" ", "");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;

namespace Warewolf.Storage
{
    /// <summary>
    /// General DataList utility methods
    /// </summary>
    public static class DataListUtils
    {
        #region Class Members

        public const string OpeningSquareBrackets = "[[";
        public const string ClosingSquareBrackets = "]]";
        public const string RecordsetIndexOpeningBracket = "(";
        public const string RecordsetIndexClosingBracket = ")";

        const string CdataStart = "<![CDATA[";
        const string CdataEnd = "]]>";
        const string AdlRoot = "ADL";
      

        #endregion Class Members



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

            if (expression.StartsWith(GlobalConstants.CalculateTextConvertPrefix))
            {
                if (expression.EndsWith(GlobalConstants.CalculateTextConvertSuffix))
                {
                    newExpression = expression.Substring(GlobalConstants.CalculateTextConvertPrefix.Length, expression.Length - (GlobalConstants.CalculateTextConvertSuffix.Length + GlobalConstants.CalculateTextConvertPrefix.Length));
                    result = true;
                }
            }

            return result;
        }


        /// <summary>
        /// Replaces the index of the star with fixed.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        public static string ReplaceStarWithFixedIndex(string exp, int idx)
        {
            return idx > 0 ? exp.Replace("(*)", RecordsetIndexOpeningBracket + idx + RecordsetIndexClosingBracket) : exp;
        }

        /// <summary>
        /// Replaces the index of a recordset with a blank index.
        /// </summary>
        /// <param name="expression">The expession.</param>
        /// <returns></returns>
        public static string ReplaceRecordsetIndexWithBlank(string expression)
        {
            var index = ExtractIndexRegionFromRecordset(expression);

            if (string.IsNullOrEmpty(index))
            {
                return expression;
            }

            string extractIndexRegionFromRecordset = string.Format("({0})", index);
            return string.IsNullOrEmpty(extractIndexRegionFromRecordset) ? expression :
                expression.Replace(extractIndexRegionFromRecordset, "()");
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
        public static bool IsMsXmlBugNode(string value)
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
        /// Used to remove &lt; and &gt; from HTML data
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string RemoveHtmlEncoding(string html)
        {
            string resultHtml = html.Replace("&amp;lt;", "<").Replace("&amp;gt;", ">");
            return resultHtml;
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
        public static string CdataWrapText(string data)
        {
            return (string.Concat(CdataStart, data, CdataEnd));
        }

        /// <summary>
        /// Unwrap any text from a CDATA region
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string CdataUnwrapText(string data)
        {
            data = data.Replace("&lt;", "<").Replace("&gt;", ">");
            return (data.Replace(CdataStart, "").Replace(CdataEnd, ""));
        }

        /// <summary>
        /// Wrap HTML in a CDATA region if fragment or FormView
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string CdataWrapHtml(string field, string value)
        {
            string result = value;

            if(field.ToLower() == "fragment" || field.Equals("FormView"))
            {
                // value.Replace("<", "&lt;").Replace(">", "&gt;")
                if(!value.Contains(CdataStart))
                {
                    result = string.Concat(CdataStart, value, CdataEnd);
                }
            }

            return result;
        }

        /// <summary>
        /// Remove CDATA regions wrapping with concerns for HTML
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string CdataUnwrapHtml(string value)
        {
            string result = value;
            result = result.Replace("&lt;", "<").Replace("&gt;", ">");
            result = result.Replace("&amp;lt;", "<").Replace("&amp;gt;", ">");
            while(result.Contains("<![CDATA"))
            {
                result = result.Replace(CdataStart, "");
                result = result.Replace(CdataEnd, "");
            }
            return result;
        }

        /// <summary>
        /// Generates the data list from defs.
        /// </summary>
        /// <param name="defs">The defs.</param>
        /// <param name="withData">if set to <c>true</c> [with data].</param>
        /// <returns></returns>
        public static StringBuilder GenerateDataListFromDefs(IList<IDev2Definition> defs, bool withData = false)
        {
            StringBuilder result = new StringBuilder("<" + AdlRoot + ">");

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
                        string name = d.Name.Contains(".") ? d.Name.Split('.')[1] : d.Name;
                        tmp = withData ? string.Concat(tmp, Environment.NewLine, "<", name, ">", d.RawValue, "</", name, ">") : string.Concat(tmp, Environment.NewLine, "<", name, "/>");


                        rsMap[d.RecordSetName] = tmp;
                    }
                    else
                    {
                        result.Append(withData ? string.Concat("<", d.Name, ">", d.RawValue, "</", d.Name, ">") : string.Concat("<", d.Name, "/>"));
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
            result.Append("</" + AdlRoot + ">");

            return result;
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
                if(value.Contains(RecordsetIndexOpeningBracket) && value.Contains(RecordsetIndexClosingBracket))
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the value is a recordset.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [value is recordset] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValueScalar(string value)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(value))
            {
                if ( value.StartsWith(OpeningSquareBrackets) && value.EndsWith(ClosingSquareBrackets))
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether is a recordset with fields
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsValueRecordsetWithFields(string value)
        {
            return !string.IsNullOrEmpty(value) && value.Contains(").");
        }

        /// <summary>
        /// Evaluates if an expression is a root level evaluated variable ie [[x]]
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool IsRootVariable(string expression)
        {
            bool result = true;

            if(expression == null)
            {
                return false;
            }

            string[] openParts = Regex.Split(expression, @"\[\[");
            string[] closeParts = Regex.Split(expression, @"\]\]");

            //2013.05.31: Ashley lewis QA feedback on bug 9379 - count the number of opening and closing braces, they must both be more than one
            if(expression.Contains(OpeningSquareBrackets) && expression.Contains(ClosingSquareBrackets) && openParts.Count() == closeParts.Count() && openParts.Count() > 2 && closeParts.Count() > 2)
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

            int openBracket = value.IndexOf(RecordsetIndexOpeningBracket, StringComparison.Ordinal);
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
            int dotIdx = value.LastIndexOf(".", StringComparison.Ordinal);
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
                result = value.Replace(OpeningSquareBrackets, "").Replace(ClosingSquareBrackets, "");
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

            if(result.StartsWith(OpeningSquareBrackets))
            {
                result = result.Substring(2, (result.Length - 2));
            }

            if(result.EndsWith(ClosingSquareBrackets))
            {
                result = result.Substring(0, (result.Length - 2));
            }

            return result;
        }

        /// <summary>
        /// Checks if a region is closed
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool EndsWithClosingTags(string value)
        {
            return !string.IsNullOrEmpty(value) && value.EndsWith(ClosingSquareBrackets);
        }

        /// <summary>
        /// Checks if a region is open
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool StartsWithOpeningTags(string value)
        {
            return !string.IsNullOrEmpty(value) && value.StartsWith(OpeningSquareBrackets);
        }

        /// <summary>
        /// Get the index of the closing tags in a variable
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int IndexOfClosingTags(string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                return -1;
            }

            return value.LastIndexOf(ClosingSquareBrackets, StringComparison.Ordinal);
        }

        /// <summary>
        /// Adds [[ ]] to a variable if they are not present already
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string AddBracketsToValueIfNotExist(string value)
        {
            string result;

            if(!value.Contains(ClosingSquareBrackets))
            {
                // missing both
                result = !value.Contains(OpeningSquareBrackets) ? string.Concat(OpeningSquareBrackets, value, ClosingSquareBrackets) : string.Concat(value, ClosingSquareBrackets);
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

            if(result.EndsWith(RecordsetIndexOpeningBracket))
            {
                result = string.Concat(result, RecordsetIndexClosingBracket);
            }
            else if(result.EndsWith(RecordsetIndexClosingBracket))
            {
                return result.Replace(RecordsetIndexClosingBracket, inject);
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

            int start = rs.IndexOf(RecordsetIndexOpeningBracket, StringComparison.Ordinal);
            if(start > 0)
            {
                int end = rs.LastIndexOf(RecordsetIndexClosingBracket, StringComparison.Ordinal);
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
        /// Determines if recordset has a star index
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool IsStarIndex(string rs)
        {
            if(string.IsNullOrEmpty(rs))
            {
                return false;
            }

            return ExtractIndexRegionFromRecordset(rs) == "*";
        }

        /// <summary>
        /// An opening brace for a recordset
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsRecordsetOpeningBrace(string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                return false;
            }

            return value.StartsWith(RecordsetIndexOpeningBracket);
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
            bool result = payload != null && payload.IndexOf(OpeningSquareBrackets, StringComparison.Ordinal) >= 0;

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

            int start = value.IndexOf(RecordsetIndexOpeningBracket, StringComparison.Ordinal);

            if(start > 0)
            {
                result = value.Substring(0, start);
            }

            return result;
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
        /// Creates a recordset display value.
        /// </summary>
        /// <param name="recsetName">Name of the recordset.</param>
        /// <param name="colName">Name of the column.</param>
        /// <param name="indexNum">The index number.</param>
        /// <returns></returns>
        public static string CreateRecordsetDisplayValue(string recsetName, string colName, string indexNum)
        {
            return string.Concat(recsetName, RecordsetIndexOpeningBracket, indexNum, ").", colName);
        }

    }
}

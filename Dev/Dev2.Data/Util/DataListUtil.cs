/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Newtonsoft.Json;
using Warewolf.Security.Encryption;
using Warewolf.Storage;
using WarewolfParserInterop;
// ReSharper disable UnusedMember.Global

namespace Dev2.Data.Util
{
    /// <summary>
    /// General DataList utility methods
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class DataListUtil
    {
        #region Class Members

        public const string OpeningSquareBrackets = "[[";
        public const string ClosingSquareBrackets = "]]";
        public const string RecordsetIndexOpeningBracket = "(";
        public const string RecordsetIndexClosingBracket = ")";

        private static readonly HashSet<string> SysTags = new HashSet<string>();
        const string AdlRoot = "ADL";

        private static readonly string[] StripTags = { "<XmlData>", "</XmlData>", "<Dev2ServiceInput>", "</Dev2ServiceInput>", "<sr>", "</sr>", "<ADL />" };
        private static readonly string[] NaughtyTags = { "<Dev2ResumeData>", "</Dev2ResumeData>", 
                                                         "<Dev2XMLResult>", "</Dev2XMLResult>", 
                                                         "<WebXMLConfiguration>", "</WebXMLConfiguration>", 
                                                         "<ActivityInput>", "</ActivityInput>", 
                                                         "<ADL>","</ADL>",
                                                         "<DL>","</DL>"
                                                       };

        private static readonly XmlReaderSettings IsXmlReaderSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Auto, DtdProcessing = DtdProcessing.Ignore };

        #endregion Class Members

        #region Constructor

        static DataListUtil()
        {
            // build system tags
            foreach (Enum e in Enum.GetValues(typeof(enSystemTag)))
            {
                SysTags.Add(e.ToString());
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

            string extractIndexRegionFromRecordset = $"({index})";
            return string.IsNullOrEmpty(extractIndexRegionFromRecordset) ? expression :
                                        expression.Replace(extractIndexRegionFromRecordset, "()");
        }


        /// <summary>
        /// Replaces the index of a recordset with a blank index.
        /// </summary>
        /// <param name="expression">The expession.</param>
        /// <returns></returns>
        public static string ReplaceRecordsetIndexWithStar(string expression)
        {
            var index = ExtractIndexRegionFromRecordset(expression);

            if (string.IsNullOrEmpty(index))
            {
                return expression;
            }

            string extractIndexRegionFromRecordset = $"({index})";
            return string.IsNullOrEmpty(extractIndexRegionFromRecordset) ? expression :
                                        expression.Replace(extractIndexRegionFromRecordset, "(*)");
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

            if (expression.StartsWith(GlobalConstants.CalculateTextConvertPrefix))
            {
                if (expression.EndsWith(GlobalConstants.CalculateTextConvertSuffix))
                {
                    newExpression = expression.Substring(GlobalConstants.CalculateTextConvertPrefix.Length, expression.Length - (GlobalConstants.CalculateTextConvertSuffix.Length + GlobalConstants.CalculateTextConvertPrefix.Length));
                    result = true;
                }
            }

            if (expression.StartsWith(GlobalConstants.AggregateCalculateTextConvertPrefix))
            {
                if (expression.EndsWith(GlobalConstants.AggregateCalculateTextConvertSuffix))
                {
                    newExpression = expression.Substring(GlobalConstants.AggregateCalculateTextConvertPrefix.Length, expression.Length - (GlobalConstants.AggregateCalculateTextConvertSuffix.Length + GlobalConstants.AggregateCalculateTextConvertPrefix.Length));
                    result = true;
                }
            }

            return result;
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
            return $"{rs}({idx}).{field}";
        }
       

        /// <summary>
        /// Remove XMLData and other nesting junk from the ADL
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public static string StripJunk(string payload)
        {
            string result = payload;
            string[] veryNaughtyTags = NaughtyTags;

            if (!string.IsNullOrEmpty(payload))
            {
                StripTags?.ToList()
                    .ForEach(tag =>
                    {
                        result = result.Replace(tag, "");
                    });

                if (veryNaughtyTags != null)
                {
                    result = CleanupNaughtyTags(veryNaughtyTags, result);
                }

                // we now need to remove non-valid chars from the stream

                int start = result.IndexOf("<", StringComparison.Ordinal);
                if (start >= 0)
                {
                    result = result.Substring(start);
                }

                if (result.Contains("<") && result.Contains(">"))
                {
                    if (!IsXml(result))
                    {
                        // We need to replace DataList if present ;)
                        result = result.Replace("<DataList>", "").Replace("</DataList>", "");
                        result = result.Replace(string.Concat("<", AdlRoot, ">"), string.Empty).Replace(string.Concat("</", AdlRoot, ">"), "");
                        result = string.Concat("<", AdlRoot, ">", result, "</", AdlRoot, ">");
                    }
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
        /// Used to determine if a tag is a system tag or not
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool IsSystemTag(string tag)
        {

            // Nasty junk that has been carried!
            string[] nastyJunk = { "WebServerUrl", "Dev2WebServer", "PostData", "Service" };

            // Transfer System Tags
            bool result = SysTags.Contains(tag) || nastyJunk.Contains(tag);

            if (!result && tag.StartsWith(GlobalConstants.SystemTagNamespaceSearch))
            {
                tag = tag.Replace(GlobalConstants.SystemTagNamespaceSearch, "");
                result = SysTags.Contains(tag) || nastyJunk.Contains(tag);
            }

            return result;
        }

       

     

        /// <summary>
        /// Shapes the definitions to data list.
        /// </summary>
        /// <returns></returns>
        public static IExecutionEnvironment InputsToEnvironment(IExecutionEnvironment outerEnvironment, string inputDefs, int update)
        {
            var env = new ExecutionEnvironment();

            try
            {
                var inputs = DataListFactory.CreateInputParser().Parse(inputDefs);
                IRecordSetCollection inputRecSets = DataListFactory.CreateRecordSetCollection(inputs, false);
                IList<IDev2Definition> inputScalarList = DataListFactory.CreateScalarList(inputs, false);
                IList<IDev2Definition> inputObjectList = DataListFactory.CreateObjectList(inputs);
                CreateRecordSetsInputs(outerEnvironment, inputRecSets, inputs, env, update);
                CreateScalarInputs(outerEnvironment, inputScalarList, env, update);
                CreateObjectInputs(outerEnvironment, inputObjectList, env,update);
            }
            finally
            {
                env.CommitAssign();
            }
            return env;
        }

        private static void CreateObjectInputs(IExecutionEnvironment outerEnvironment, IEnumerable<IDev2Definition> inputObjectList, ExecutionEnvironment env,int update)
        {
            foreach (var dev2Definition in inputObjectList)
            {
                if (!string.IsNullOrEmpty(dev2Definition.RawValue))
                {
                    if (RemoveLanguageBrackets(dev2Definition.RawValue).StartsWith("@"))
                    {
                        var jVal = outerEnvironment.EvalJContainer(dev2Definition.RawValue);
                        env.AddToJsonObjects(AddBracketsToValueIfNotExist(dev2Definition.Name),jVal);
                    }
                    else
                    {
                        var result = outerEnvironment.Eval(dev2Definition.RawValue, update);
                        if (result.IsWarewolfAtomListresult)
                        {
                            var data = result as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                            if (data != null && data.Item.Any())
                            {
                                env.AssignWithFrame(new AssignValue(AddBracketsToValueIfNotExist(dev2Definition.Name), ExecutionEnvironment.WarewolfAtomToString(data.Item.Last())), 0);
                            }
                        }
                        else
                        {
                            var data = result as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                            if (data != null)
                            {
                                env.AssignWithFrame(new AssignValue(AddBracketsToValueIfNotExist(dev2Definition.Name), ExecutionEnvironment.WarewolfAtomToString(data.Item)), 0);
                            }
                        }
                    }
                    
                }
            }
        }

        static void CreateRecordSetsInputs(IExecutionEnvironment outerEnvironment, IRecordSetCollection inputRecSets, IList<IDev2Definition> inputs, ExecutionEnvironment env, int update)
        {
            foreach (var recordSetDefinition in inputRecSets.RecordSets)
            {
                var outPutRecSet = inputs.FirstOrDefault(definition => definition.IsRecordSet && ExtractRecordsetNameFromValue(definition.MapsTo) == recordSetDefinition.SetName);
                if (outPutRecSet != null)
                {
                    var emptyList = new List<string>();
                    foreach (var dev2ColumnDefinition in recordSetDefinition.Columns)
                    {
                        if (dev2ColumnDefinition.IsRecordSet)
                        {
                            var defn = "[[" + dev2ColumnDefinition.RecordSetName + "()." + dev2ColumnDefinition.Name + "]]";


                            if (string.IsNullOrEmpty(dev2ColumnDefinition.RawValue))
                            {
                                if (!emptyList.Contains(defn))
                                {
                                    emptyList.Add(defn);
                                    continue;
                                }
                            }
                            var warewolfEvalResult = outerEnvironment.Eval(dev2ColumnDefinition.RawValue, update);

                            if (warewolfEvalResult.IsWarewolfAtomListresult)
                            {
                                AtomListInputs(warewolfEvalResult, dev2ColumnDefinition, env);
                            }
                            if (warewolfEvalResult.IsWarewolfAtomResult)
                            {
                                AtomInputs(warewolfEvalResult, dev2ColumnDefinition, env);
                            }
                        }
                    }
                    foreach (var defn in emptyList)
                    {
                        env.AssignDataShape(defn);
                    }
                }
            }
        }

        static void CreateScalarInputs(IExecutionEnvironment outerEnvironment, IEnumerable<IDev2Definition> inputScalarList, ExecutionEnvironment env, int update)
        {
            foreach (var dev2Definition in inputScalarList)
            {
                if (!string.IsNullOrEmpty(dev2Definition.Name))
                {
                    env.AssignDataShape("[[" + dev2Definition.Name + "]]");
                }
                if (!dev2Definition.IsRecordSet)
                {
                    if (!string.IsNullOrEmpty(dev2Definition.RawValue))
                    {
                        var warewolfEvalResult = outerEnvironment.Eval(dev2Definition.RawValue, update);
                        if (warewolfEvalResult.IsWarewolfAtomListresult)
                        {
                            ScalarAtomList(warewolfEvalResult, env, dev2Definition);
                        }
                        else
                        {
                            ScalarAtom(warewolfEvalResult, env, dev2Definition);
                        }
                    }
                }
            }
        }

        static void ScalarAtom(CommonFunctions.WarewolfEvalResult warewolfEvalResult, ExecutionEnvironment env, IDev2Definition dev2Definition)
        {
            var data = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            if (data != null)
            {
                env.AssignWithFrame(new AssignValue("[[" + dev2Definition.Name + "]]", ExecutionEnvironment.WarewolfAtomToString(data.Item)), 0);
            }
        }

        static void ScalarAtomList(CommonFunctions.WarewolfEvalResult warewolfEvalResult, ExecutionEnvironment env, IDev2Definition dev2Definition)
        {
            var data = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            if (data != null && data.Item.Any())
            {
                env.AssignWithFrame(new AssignValue("[[" + dev2Definition.Name + "]]", ExecutionEnvironment.WarewolfAtomToString(data.Item.Last())), 0);
            }
        }

        static void AtomInputs(CommonFunctions.WarewolfEvalResult warewolfEvalResult, IDev2Definition dev2ColumnDefinition, ExecutionEnvironment env)
        {
            var recsetResult = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;

            if (dev2ColumnDefinition.IsRecordSet)
            {


                if (recsetResult != null)
                {
                    var correctRecSet = "[[" + dev2ColumnDefinition.RecordSetName + "(*)." + dev2ColumnDefinition.Name + "]]";

                    env.AssignWithFrame(new AssignValue(correctRecSet, PublicFunctions.AtomtoString(recsetResult.Item)), 0);
                }
            }
        }

        static void AtomListInputs(CommonFunctions.WarewolfEvalResult warewolfEvalResult, IDev2Definition dev2ColumnDefinition, ExecutionEnvironment env)
        {
            var recsetResult = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;

            GetRecordsetIndexType(dev2ColumnDefinition.Value);

            if (recsetResult != null)
            {
                var correctRecSet = "[[" + dev2ColumnDefinition.RecordSetName + "(*)." + dev2ColumnDefinition.Name + "]]";

                env.EvalAssignFromNestedStar(correctRecSet, recsetResult, 0);
            }
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

            if (!string.IsNullOrEmpty(value))
            {
                if (value.Contains(RecordsetIndexOpeningBracket) && value.Contains(RecordsetIndexClosingBracket))
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
                if (value.StartsWith(OpeningSquareBrackets) && value.EndsWith(ClosingSquareBrackets) && !IsValueRecordset(value))
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
        /// Used to extract a recordset name from a string as per the Dev2 data language spec
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns></returns>
        public static string ExtractRecordsetNameFromValue(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            value = StripBracketsFromValue(value);
            string result = string.Empty;

            int openBracket = value.IndexOf(RecordsetIndexOpeningBracket, StringComparison.Ordinal);
            if (openBracket > 0)
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
            if (dotIdx > 0)
            {
                result = value.Substring(dotIdx + 1);
            }

            return result;
        }

        /// <summary>
        /// Used to extract a field name from our recordset notation
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns></returns>
        public static string ExtractFieldNameOnlyFromValue(string value)
        {
            string result = string.Empty;
            int dotIdx = value.LastIndexOf(".", StringComparison.Ordinal);
            int closeIdx = value.Contains("]]") ? value.LastIndexOf("]]", StringComparison.Ordinal) : value.Length;
            if (dotIdx > 0)
            {
                result = value.Substring(dotIdx + 1, closeIdx - dotIdx - 1);
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
            if (value != null)
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
        private static string StripLeadingAndTrailingBracketsFromValue(string value)
        {
            string result = value;

            if (result.StartsWith(OpeningSquareBrackets))
            {
                result = result.Substring(2, result.Length - 2);
            }

            if (result.EndsWith(ClosingSquareBrackets))
            {
                result = result.Substring(0, result.Length - 2);
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

            if (!value.Contains(ClosingSquareBrackets))
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

            if (starNotation)
            {
                inject = "(*)";
            }

            string result = StripBracketsFromValue(value);

            if (result.EndsWith(RecordsetIndexOpeningBracket))
            {
                result = string.Concat(result, RecordsetIndexClosingBracket);
            }
            else if (result.EndsWith(RecordsetIndexClosingBracket))
            {
                return result.Replace(RecordsetIndexClosingBracket, inject);
            }
            else if (!result.EndsWith("()"))
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
            if (start > 0)
            {
                int end = rs.LastIndexOf(RecordsetIndexClosingBracket, StringComparison.Ordinal);
                if (end < 0)
                {
                    end = rs.Length;
                }

                start += 1;
                result = rs.Substring(start, end - start);
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
            if (string.IsNullOrEmpty(rs))
            {
                return false;
            }

            return ExtractIndexRegionFromRecordset(rs) == "*";
        }
        
        /// <summary>
        /// Is the expression evaluated
        /// </summary>  
        /// <param name="payload">The payload.</param>
        /// <returns>
        ///   <c>true</c> if the specified payload is evaluated; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFullyEvaluated(string payload)
        {
            bool result = payload != null && payload.IndexOf(OpeningSquareBrackets, StringComparison.Ordinal) >= 0 
                && payload.IndexOf(ClosingSquareBrackets, StringComparison.Ordinal) >= 0;

            return result;
        }

        public static bool ShouldEncrypt(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            if (IsFullyEvaluated(value))
            {
                return false;
            }
            if (value.CanBeDecrypted())
            {
                return false;
            }
            return true;
        }

        public static bool NotEncrypted(string value)
        {
            return string.IsNullOrEmpty(value) || IsFullyEvaluated(value);
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
        /// Gets the index type of a recorset
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static enRecordsetIndexType GetRecordsetIndexType(string expression)
        {
            enRecordsetIndexType result = enRecordsetIndexType.Error;

            string idx = ExtractIndexRegionFromRecordset(expression);
            if (idx == "*")
            {
                result = enRecordsetIndexType.Star;
            }
            else if (string.IsNullOrEmpty(idx))
            {
                result = enRecordsetIndexType.Blank;
            }
            else
            {
                int convertIntTest;
                if (Int32.TryParse(idx, out convertIntTest))
                {
                    result = enRecordsetIndexType.Numeric;
                }
            }

            return result;
        }

        //used in the replace node method

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
        static bool IsXml(string data, out bool isFragment, out bool isHtml)
        {
            string trimedData = data.Trim();
            bool result = trimedData.StartsWith("<") && !trimedData.StartsWith("<![CDATA[");

            isFragment = false;
            isHtml = false;

            if (result)
            {
                using (TextReader tr = new StringReader(trimedData))
                {
                    using (XmlReader reader = XmlReader.Create(tr, IsXmlReaderSettings))
                    {

                        try
                        {
                            long nodeCount = 0;
                            while (reader.Read() && !isHtml && !isFragment && reader.NodeType != XmlNodeType.Document)
                            {
                                nodeCount++;

                                if (reader.NodeType != XmlNodeType.CDATA)
                                {
                                    if (reader.NodeType == XmlNodeType.Element && reader.Name.ToLower() == "html" && reader.Depth == 0)
                                    {
                                        isHtml = true;
                                        result = false;
                                    }

                                    if (reader.NodeType == XmlNodeType.Element && nodeCount > 1 && reader.Depth == 0)
                                    {
                                        isFragment = true;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Dev2Logger.Error("DataListUtil", ex);
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

        public static bool IsJson(string data)
        {
            var tmp = data.Trim();
            if (tmp.StartsWith("{") && tmp.EndsWith("}"))
            {
                return true;
            }

            return false;
        }

        public static IList<string> GetAllPossibleExpressionsForFunctionOperations(string expression, IExecutionEnvironment env, out ErrorResultTO errors, int update)
        {
            IList<string> result = new List<string>();
            errors = new ErrorResultTO();
            try
            {
                result = env.EvalAsListOfStrings(expression, update);

            }
            catch (Exception err)
            {
                errors.AddError(err.Message);

            }


            return result;
        }

        /// <summary>
        /// Adjusts for encoding issues.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public static string AdjustForEncodingIssues(string payload)
        {
            string trimedData = payload.Trim();
            var isXml = trimedData.StartsWith("<") && !trimedData.StartsWith("<![CDATA[");

            if (!isXml)
            {
                // we need to adjust. there might be a silly encoding issue with first char!
                if (trimedData.Length > 1 && trimedData[1] == '<' && trimedData[2] == '?')
                {
                    trimedData = trimedData.Substring(1);
                }
                else if (trimedData.Length > 2 && trimedData[2] == '<' && trimedData[3] == '?')
                {
                    trimedData = trimedData.Substring(2);
                }
                else if (trimedData.Length > 3 && trimedData[3] == '<' && trimedData[4] == '?')
                {
                    trimedData = trimedData.Substring(3);
                }
            }
            var bomMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (trimedData.StartsWith(bomMarkUtf8, StringComparison.OrdinalIgnoreCase))
                trimedData = trimedData.Remove(0, bomMarkUtf8.Length);
            trimedData = trimedData.Replace("\0", "");
            return trimedData;
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

            for (int i = 0; i < toRemove.Length; i++)
            {
                string myTag = toRemove[i];
                if (myTag.IndexOf("<", StringComparison.Ordinal) >= 0 && myTag.IndexOf("</", StringComparison.Ordinal) < 0)
                {
                    foundOpen = true;
                }
                else if (myTag.IndexOf("</", StringComparison.Ordinal) >= 0)
                {
                    // close tag
                    if (foundOpen)
                    {
                        // remove data between
                        int loc = i - 1;
                        if (loc >= 0)
                        {
                            int start = result.IndexOf(toRemove[loc], StringComparison.Ordinal);
                            int end = result.IndexOf(myTag, StringComparison.Ordinal);
                            if (start < end && start >= 0)
                            {
                                string canidate = result.Substring(start, end - start + myTag.Length);
                                string tmpResult = canidate.Replace(myTag, "").Replace(toRemove[loc], "");
                                if (tmpResult.IndexOf("</", StringComparison.Ordinal) >= 0 || tmpResult.IndexOf("/>", StringComparison.Ordinal) >= 0)
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
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            target.Clear();

            if (tokenizer == null)
            {
                return;
            }


            while (tokenizer.HasMoreOps())
            {
                var token = tokenizer.NextToken();
                if (string.IsNullOrEmpty(token))
                {
                    if (!removeEmptyEntries)
                    {
                        target.Add(new ObservablePair<string, string>(string.Empty, string.Empty));
                    }
                }
                else
                {
                    token = AddBracketsToValueIfNotExist($"{tokenPrefix}{StripLeadingAndTrailingBracketsFromValue(token)}{tokenSuffix}");

                    target.Add(new ObservablePair<string, string>(token, string.Empty));
                }
            }

            foreach (var observablePair in target)
            {
                observablePair.Key = observablePair.Key.Replace(" ", "");
            }
        }

        public static void OutputsToEnvironment(IExecutionEnvironment innerEnvironment, IExecutionEnvironment environment, string outputDefs, int update)
        {
            try
            {
                var outputs = DataListFactory.CreateOutputParser().Parse(outputDefs);
                var outputRecSets = DataListFactory.CreateRecordSetCollection(outputs, true);
                var outputScalarList = DataListFactory.CreateScalarList(outputs, true);
                var outputComplexObjectList = DataListFactory.CreateObjectList(outputs);
                foreach (var recordSetDefinition in outputRecSets.RecordSets)
                {
                    var outPutRecSet = outputs.FirstOrDefault(definition => definition.IsRecordSet && definition.RecordSetName == recordSetDefinition.SetName);
                    if (outPutRecSet != null)
                    {
                        foreach (var outputColumnDefinitions in recordSetDefinition.Columns)
                        {
                            var correctRecSet = "[[" + outputColumnDefinitions.RecordSetName + "(*)." + outputColumnDefinitions.Name + "]]";
                            var warewolfEvalResult = innerEnvironment.Eval(correctRecSet, 0);
                            if (warewolfEvalResult.IsWarewolfAtomListresult)
                            {
                                var recsetResult = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                                if (outPutRecSet.IsRecordSet)
                                {
                                    var enRecordsetIndexType = GetRecordsetIndexType(outputColumnDefinitions.RawValue);
                                    if (enRecordsetIndexType == enRecordsetIndexType.Star)
                                    {
                                        if (recsetResult != null)
                                        {
                                            environment.EvalAssignFromNestedStar(outputColumnDefinitions.RawValue, recsetResult, update);
                                        }
                                    }
                                    if (enRecordsetIndexType == enRecordsetIndexType.Blank)
                                    {
                                        if (recsetResult != null)
                                        {

                                            environment.EvalAssignFromNestedLast(outputColumnDefinitions.RawValue, recsetResult, 0);
                                        }
                                    }
                                    if (enRecordsetIndexType == enRecordsetIndexType.Numeric)
                                    {
                                        if (recsetResult != null)
                                        {

                                            environment.EvalAssignFromNestedNumeric(outputColumnDefinitions.RawValue, recsetResult, 0);
                                        }
                                    }

                                }
                            }

                        }
                    }
                }

                foreach (var dev2Definition in outputScalarList)
                {
                    if (!dev2Definition.IsRecordSet && !dev2Definition.IsObject)
                    {
                        var warewolfEvalResult = innerEnvironment.Eval(AddBracketsToValueIfNotExist(dev2Definition.Name), update);
                        if (warewolfEvalResult.IsWarewolfAtomListresult)
                        {
                            var data = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                            if (data != null && data.Item.Any())
                            {
                                environment.Assign("[[" + dev2Definition.Value + "]]", ExecutionEnvironment.WarewolfAtomToString(data.Item.Last()), update);
                            }
                        }
                        else
                        {
                            var data = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                            if (data != null)
                            {
                                environment.Assign(AddBracketsToValueIfNotExist(dev2Definition.Value), ExecutionEnvironment.WarewolfAtomToString(data.Item), update);
                            }
                        }
                    }
                }

                foreach (var dev2Definition in outputComplexObjectList)
                {
                    if (dev2Definition.IsObject)
                    {
                        var warewolfEvalResult = innerEnvironment.EvalJContainer(AddBracketsToValueIfNotExist(dev2Definition.Name));
                        if (warewolfEvalResult != null)
                        {
                            environment.AddToJsonObjects(AddBracketsToValueIfNotExist(dev2Definition.Value), warewolfEvalResult);
                        }
                        
                    }
                }
            }
            finally
            {
                environment.CommitAssign();
            }

        }

        public static string ReplaceRecordsetBlankWithIndex(string fullRecSetName, int length)
        {
            var blankIndex = fullRecSetName.IndexOf("().", StringComparison.Ordinal);
            if (blankIndex != -1)
            {
                return fullRecSetName.Replace("().", $"({length}).");
            }
            return fullRecSetName;
        }

        public static string ReplaceRecordsetBlankWithStar(string fullRecSetName)
        {
            var blankIndex = fullRecSetName.IndexOf("().", StringComparison.Ordinal);
            if (blankIndex != -1)
            {
                return fullRecSetName.Replace("().", $"({"*"}).");
            }
            return fullRecSetName;
        }

        public static string ReplaceRecordBlankWithStar(string fullRecSetName)
        {
            var blankIndex = fullRecSetName.IndexOf("()", StringComparison.Ordinal);
            if (blankIndex != -1)
            {
                return fullRecSetName.Replace("()", $"({"*"})");
            }
            return fullRecSetName;
        }

        public static bool HasNegativeIndex(string variable)
        {
            if (IsEvaluated(variable))
            {
                var enRecordsetIndexType = GetRecordsetIndexType(variable);
                if (enRecordsetIndexType != enRecordsetIndexType.Numeric)
                {
                    return false;
                }
                if (IsValueRecordset(variable))
                {
                    var index = ExtractIndexRegionFromRecordset(variable);
                    int val;
                    if (!int.TryParse(index, out val))
                    {
                        return true;
                    }
                    return val < 0;
                }
            }
            return false;
        }


        public static string GenerateSerializableDefsFromDataList(string datalist, enDev2ColumnArgumentDirection direction)
        {
            DefinitionBuilder db = new DefinitionBuilder();

            if (direction == enDev2ColumnArgumentDirection.Input)
            {
                db.ArgumentType = enDev2ArgumentType.Input;
            }
            else if (direction == enDev2ColumnArgumentDirection.Output)
            {
                db.ArgumentType = enDev2ArgumentType.Output;
            }

            db.Definitions = GenerateDefsFromDataList(datalist, direction);

            return db.Generate();
        }

     

        public static IList<IDev2Definition> GenerateDefsFromDataList(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection)
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();

            if (!string.IsNullOrEmpty(dataList))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(dataList);

                XmlNodeList tmpRootNl = xDoc.ChildNodes;
                XmlNodeList nl = tmpRootNl[0].ChildNodes;

                for (int i = 0; i < nl.Count; i++)
                {
                    XmlNode tmpNode = nl[i];

                    var ioDirection = GetDev2ColumnArgumentDirection(tmpNode);

                    if (CheckIODirection(dev2ColumnArgumentDirection, ioDirection))
                    {
                        var jsonAttribute = false;
                        var xmlAttribute = tmpNode.Attributes?["IsJson"];
                        if (xmlAttribute != null)
                        {
                            bool.TryParse(xmlAttribute.Value, out jsonAttribute);
                        }
                        if (tmpNode.HasChildNodes && !jsonAttribute)
                        {
                            // it is a record set, make it as such
                            string recordsetName = tmpNode.Name;
                            // now extract child node defs
                            XmlNodeList childNl = tmpNode.ChildNodes;
                            for (int q = 0; q < childNl.Count; q++)
                            {
                                var xmlNode = childNl[q];
                                var fieldIODirection = GetDev2ColumnArgumentDirection(xmlNode);
                                if (CheckIODirection(dev2ColumnArgumentDirection, fieldIODirection))
                                {
                                    result.Add(DataListFactory.CreateDefinition(xmlNode.Name, "", "", recordsetName, false, "",
                                                                                false, "", false));
                                }
                            }
                        }
                        else
                        {
                            // scalar value, make it as such
                            var name = jsonAttribute ? "@"+tmpNode.Name:tmpNode.Name;
                            var dev2Definition = DataListFactory.CreateDefinition(name, "", "", false, "", false, "");
                            dev2Definition.IsObject = jsonAttribute;
                            result.Add(dev2Definition);
                        }
                    }
                }
            }

            return result;
        }
        static bool CheckIODirection(enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, enDev2ColumnArgumentDirection ioDirection)
        {
            return ioDirection == dev2ColumnArgumentDirection ||
                   ioDirection == enDev2ColumnArgumentDirection.Both &&
                   (dev2ColumnArgumentDirection == enDev2ColumnArgumentDirection.Input || dev2ColumnArgumentDirection == enDev2ColumnArgumentDirection.Output);
        }

        static enDev2ColumnArgumentDirection GetDev2ColumnArgumentDirection(XmlNode tmpNode)
        {
            XmlAttribute ioDirectionAttribute = tmpNode.Attributes[GlobalConstants.DataListIoColDirection];

            enDev2ColumnArgumentDirection ioDirection;
            if (ioDirectionAttribute != null)
            {
                ioDirection = (enDev2ColumnArgumentDirection)Dev2EnumConverter.GetEnumFromStringDiscription(ioDirectionAttribute.Value, typeof(enDev2ColumnArgumentDirection));
            }
            else
            {
                ioDirection = enDev2ColumnArgumentDirection.Both;
            }
            return ioDirection;
        }

        static bool IsObject(XmlNode tmpNode)
        {
            XmlAttribute isObjectAttribute = tmpNode.Attributes?["IsJson"];

            if (isObjectAttribute != null)
            {
                bool isObject;
                if(bool.TryParse(isObjectAttribute.Value,out isObject))
                {
                    return isObject;
                }
            }
            return false;
        }
        public static IList<IDev2Definition> GenerateDefsFromDataListForDebug(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection)
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();

            if (!string.IsNullOrEmpty(dataList))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(dataList);

                XmlNodeList tmpRootNl = xDoc.ChildNodes;
                XmlNodeList nl = tmpRootNl[0].ChildNodes;

                for (int i = 0; i < nl.Count; i++)
                {
                    XmlNode tmpNode = nl[i];

                    var ioDirection = GetDev2ColumnArgumentDirection(tmpNode);
                    var isObject = IsObject(tmpNode);
                    if (CheckIODirection(dev2ColumnArgumentDirection, ioDirection) && tmpNode.HasChildNodes && !isObject)
                    {
                        result.Add(DataListFactory.CreateDefinition("", "", "", tmpNode.Name, false, "",
                                                                            false, "", false));
                    }
                    else if (tmpNode.HasChildNodes && !isObject)
                    {
                        // it is a record set, make it as such
                        string recordsetName = tmpNode.Name;
                        // now extract child node defs
                        XmlNodeList childNl = tmpNode.ChildNodes;
                        for (int q = 0; q < childNl.Count; q++)
                        {
                            var xmlNode = childNl[q];
                            var fieldIODirection = GetDev2ColumnArgumentDirection(xmlNode);
                            if (CheckIODirection(dev2ColumnArgumentDirection, fieldIODirection))
                            {
                                result.Add(DataListFactory.CreateDefinition(xmlNode.Name, "", "", recordsetName, false, "",
                                                                            false, "", false));
                            }
                        }
                    }
                    else if (CheckIODirection(dev2ColumnArgumentDirection, ioDirection))
                    {
                        // scalar value, make it as such
                        result.Add(isObject ? DataListFactory.CreateDefinition("@" + tmpNode.Name, "", "", false, "", false, "") : DataListFactory.CreateDefinition(tmpNode.Name, "", "", false, "", false, ""));
                    }

                }
            }

            return result;
        }

        /// <summary>
        /// Converts from to.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public static T ConvertFromJsonToModel<T>(StringBuilder payload)
        {

            T obj = JsonConvert.DeserializeObject<T>(payload.ToString());

            return obj;
        }

        /// <summary>
        /// Converts the model to json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public static StringBuilder ConvertModelToJson<T>(T payload)
        {
            var result = new StringBuilder(JsonConvert.SerializeObject(payload));

            return result;
        }

    }
}

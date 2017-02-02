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
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Interfaces;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Newtonsoft.Json;
using Warewolf.Security.Encryption;
using Warewolf.Storage;

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
        internal const string ClosingSquareBrackets = "]]";
        internal const string RecordsetIndexOpeningBracket = "(";
        internal const string RecordsetIndexClosingBracket = ")";

        private static readonly HashSet<string> SysTags = new HashSet<string>();
        private static readonly Lazy<ICommon> LazyCommon = new Lazy<ICommon>(()=> new CommonDataUtils(), LazyThreadSafetyMode.ExecutionAndPublication);
        private static ICommon Common => LazyCommon.Value;

        private static readonly Lazy<ICommonRecordSetUtil> LazyRecSetCommon = new Lazy<ICommonRecordSetUtil>(()=>new CommonRecordSetUtil(),LazyThreadSafetyMode.ExecutionAndPublication);
        private static ICommonRecordSetUtil RecSetCommon => LazyRecSetCommon.Value;

        private static readonly Lazy<ICommonScalarUtil> LazyScalarCommon = new Lazy<ICommonScalarUtil>(()=>new CommonScalarUtil(),LazyThreadSafetyMode.ExecutionAndPublication);
        private static ICommonScalarUtil ScalarCommon => LazyScalarCommon.Value;

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
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsArray(ref string path)
        {
            var isArray = false;

            if (path.Contains("()") || path.Contains("(*)"))
            {
                isArray = true;
                path = path.Replace("(*)", "()");
            }
            return isArray;
        }

        /// <summary>
        /// Replaces the index of a recordset with a blank index.
        /// </summary>
        /// <param name="expression">The expession.</param>
        /// <returns></returns>
        public static string ReplaceRecordsetIndexWithBlank(string expression) => RecSetCommon.ReplaceRecordsetIndexWithBlank(expression);

        /// <summary>
        /// Replaces the index of a recordset with a blank index.
        /// </summary>
        /// <param name="expression">The expession.</param>
        /// <returns></returns>
        public static string ReplaceRecordsetIndexWithStar(string expression) => RecSetCommon.ReplaceRecordsetIndexWithStar(expression);

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
                Common.CreateRecordSetsInputs(outerEnvironment, inputRecSets, inputs, env, update);
                Common.CreateScalarInputs(outerEnvironment, inputScalarList, env, update);
                Common.CreateObjectInputs(outerEnvironment, inputObjectList, env, update);
            }
            finally
            {
                env.CommitAssign();
            }
            return env;
        }

        /// <summary>
        /// Determines whether the value is a recordset.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [value is recordset] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValueRecordset(string value) => RecSetCommon.IsValueRecordset(value);

        /// <summary>
        /// Determines whether the value is a scalar.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [value is scalar] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValueScalar(string value) => ScalarCommon.IsValueScalar(value);

        /// <summary>
        /// Determines whether is a recordset with fields
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsValueRecordsetWithFields(string value) => RecSetCommon.IsValueRecordsetWithFields(value);

        /// <summary>
        /// Used to extract a recordset name from a string as per the Dev2 data language spec
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns></returns>
        public static string ExtractRecordsetNameFromValue(string value) => RecSetCommon.ExtractRecordsetNameFromValue(value);

        /// <summary>
        /// Used to extract a field name from our recordset notation
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns></returns>
        public static string ExtractFieldNameFromValue(string value) => RecSetCommon.ExtractFieldNameFromValue(value);

        /// <summary>
        /// Used to extract a field name from our recordset notation
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns></returns>
        public static string ExtractFieldNameOnlyFromValue(string value) => RecSetCommon.ExtractFieldNameOnlyFromValue(value);

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
        public static string MakeValueIntoHighLevelRecordset(string value, bool starNotation = false) => RecSetCommon.MakeValueIntoHighLevelRecordset(value, starNotation);

        /// <summary>
        /// Used to extract an index in the recordset notation
        /// </summary>
        /// <param name="rs">The rs.</param>
        /// <returns></returns>
        public static string ExtractIndexRegionFromRecordset(string rs) => RecSetCommon.ExtractIndexRegionFromRecordset(rs);

        /// <summary>
        /// Determines if recordset has a star index
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool IsStarIndex(string rs) => RecSetCommon.IsStarIndex(rs);

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
        public static enRecordsetIndexType GetRecordsetIndexType(string expression) => RecSetCommon.GetRecordsetIndexType(expression);

        //used in the replace node method

        /// <summary>
        /// Checks if the info contained in data is well formed XML
        /// </summary>
        public static bool IsXml(string data)
        {
            bool isFragment;
            bool isHtml;
            var isXml = XmlHelper.IsXml(data, out isFragment, out isHtml);
            return isXml && !isFragment && !isHtml;
        }

        /// <summary>
        /// Checks if the info contained in data is well formed XML
        /// </summary>
        public static bool IsXml(string data, out bool isFragment)
        {
            bool isHtml;

            return XmlHelper.IsXml(data, out isFragment, out isHtml) && !isFragment && !isHtml;
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
        /// Removes the recordset brackets from a value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string RemoveRecordsetBracketsFromValue(string value) => RecSetCommon.RemoveRecordsetBracketsFromValue(value);

        /// <summary>
        /// Creates a recordset display value.
        /// </summary>
        /// <param name="recsetName">Name of the recordset.</param>
        /// <param name="colName">Name of the column.</param>
        /// <param name="indexNum">The index number.</param>
        /// <returns></returns>
        public static string CreateRecordsetDisplayValue(string recsetName, string colName, string indexNum) => RecSetCommon.CreateRecordsetDisplayValue(recsetName, colName, indexNum);

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



        public static string ReplaceRecordsetBlankWithIndex(string fullRecSetName, int length) => RecSetCommon.ReplaceRecordsetBlankWithIndex(fullRecSetName, length);

        public static string ReplaceRecordsetBlankWithStar(string fullRecSetName) => RecSetCommon.ReplaceRecordsetBlankWithStar(fullRecSetName);

        public static string ReplaceRecordBlankWithStar(string fullRecSetName) => RecSetCommon.ReplaceRecordBlankWithStar(fullRecSetName);

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

        public static IList<IDev2Definition> GenerateDefsFromDataList(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection) => Common.GenerateDefsFromDataList(dataList, dev2ColumnArgumentDirection);

        internal static bool CheckIODirection(enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, enDev2ColumnArgumentDirection ioDirection)
        {
            return ioDirection == dev2ColumnArgumentDirection ||
                   ioDirection == enDev2ColumnArgumentDirection.Both &&
                   (dev2ColumnArgumentDirection == enDev2ColumnArgumentDirection.Input || dev2ColumnArgumentDirection == enDev2ColumnArgumentDirection.Output);
        }

        internal static enDev2ColumnArgumentDirection GetDev2ColumnArgumentDirection(XmlNode tmpNode)
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

       
        public static IList<IDev2Definition> GenerateDefsFromDataListForDebug(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection) => Common.GenerateDefsFromDataListForDebug(dataList, dev2ColumnArgumentDirection);

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

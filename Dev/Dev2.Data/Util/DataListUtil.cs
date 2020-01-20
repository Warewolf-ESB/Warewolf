#pragma warning disable
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Common.Interfaces.Search;
using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Newtonsoft.Json;
using Warewolf.Data;
using Warewolf.Security.Encryption;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Data.Util
{
    /// <summary>
    /// General DataList utility methods
    /// </summary>

    public static class DataListUtil
    {
        #region Class Members

        public const string OpeningSquareBrackets = "[[";
        internal const string ClosingSquareBrackets = "]]";
        internal const string RecordsetIndexOpeningBracket = "(";
        internal const string RecordsetIndexClosingBracket = ")";
        public const string ObjectStartMarker = "@";

        static readonly HashSet<string> SysTags = new HashSet<string>();
        static readonly Lazy<ICommon> LazyCommon = new Lazy<ICommon>(() => new CommonDataUtils(), LazyThreadSafetyMode.ExecutionAndPublication);
        static ICommon Common => LazyCommon.Value;

        static readonly Lazy<ICommonRecordSetUtil> LazyRecSetCommon = new Lazy<ICommonRecordSetUtil>(() => new CommonRecordSetUtil(), LazyThreadSafetyMode.ExecutionAndPublication);
        static ICommonRecordSetUtil RecSetCommon => LazyRecSetCommon.Value;

        static readonly Lazy<ICommonScalarUtil> LazyScalarCommon = new Lazy<ICommonScalarUtil>(() => new CommonScalarUtil(), LazyThreadSafetyMode.ExecutionAndPublication);
        static ICommonScalarUtil ScalarCommon => LazyScalarCommon.Value;

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
        public static string ReplaceStarWithFixedIndex(string exp, int idx) => idx > 0 ? exp.Replace("(*)", RecordsetIndexOpeningBracket + idx + RecordsetIndexClosingBracket) : exp;

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

        public static string GetVariableNameToMapOutputTo(string mappedTo)
        {
            if (IsValueRecordset(mappedTo))
            {
                return ExtractFieldNameFromValue(mappedTo);
            }
            return RemoveLanguageBrackets(mappedTo);
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
            var result = false;
            newExpression = string.Empty;

            if (expression.StartsWith(GlobalConstants.CalculateTextConvertPrefix, StringComparison.Ordinal) &&
                expression.EndsWith(GlobalConstants.CalculateTextConvertSuffix, StringComparison.Ordinal))
            {
                newExpression = expression.Substring(GlobalConstants.CalculateTextConvertPrefix.Length, expression.Length - (GlobalConstants.CalculateTextConvertSuffix.Length + GlobalConstants.CalculateTextConvertPrefix.Length));
                result = true;
            }

            if (expression.StartsWith(GlobalConstants.AggregateCalculateTextConvertPrefix, StringComparison.Ordinal) &&
                expression.EndsWith(GlobalConstants.AggregateCalculateTextConvertSuffix, StringComparison.Ordinal))
            {
                newExpression = expression.Substring(GlobalConstants.AggregateCalculateTextConvertPrefix.Length, expression.Length - (GlobalConstants.AggregateCalculateTextConvertSuffix.Length + GlobalConstants.AggregateCalculateTextConvertPrefix.Length));
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Removes the brackets.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public static string RemoveLanguageBrackets(string val) => val.Replace("[", string.Empty).Replace("]", string.Empty);

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
            var result = SysTags.Contains(tag) || nastyJunk.Contains(tag);

            if (!result && tag.StartsWith(GlobalConstants.SystemTagNamespaceSearch, StringComparison.Ordinal))
            {
                var replacedTag = tag.Replace(GlobalConstants.SystemTagNamespaceSearch, "");
                result = SysTags.Contains(replacedTag) || nastyJunk.Contains(replacedTag);
            }

            return result;
        }

        public static IExecutionEnvironment InputsToEnvironment(IExecutionEnvironment outerEnvironment, string inputDefs, int update)
        {
            var env = new ExecutionEnvironment();

            try
            {
                var inputs = DataListFactory.CreateInputParser().Parse(inputDefs);
                var inputRecSets = DataListFactory.Instance.CreateRecordSetCollection(inputs, false);
                var inputScalarList = DataListFactory.Instance.CreateScalarList(inputs, false);
                var inputObjectList = DataListFactory.Instance.CreateObjectList(inputs);
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
            var result = string.Empty;
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
        static string StripLeadingAndTrailingBracketsFromValue(string value)
        {
            var result = value;

            if (result.StartsWith(OpeningSquareBrackets, StringComparison.Ordinal))
            {
                result = result.Substring(2, result.Length - 2);
            }

            if (result.EndsWith(ClosingSquareBrackets, StringComparison.Ordinal))
            {
                result = result.Substring(0, result.Length - 2);
            }

            return result;
        }

        public static string AddBracketsToValueIfNotExist(string value)
        {
            string WrapInBrackets(string val)
            {
                var result = val;
                if (!value.Contains(OpeningSquareBrackets))
                {
                    result = string.Concat(OpeningSquareBrackets, result);
                }
                if (!value.Contains(ClosingSquareBrackets))
                {
                    result = string.Concat(result, ClosingSquareBrackets);
                }
                
                return result;
            }

            return WrapInBrackets(value); 
        }

        public static string MakeValueIntoHighLevelRecordset(string value) => RecSetCommon.MakeValueIntoHighLevelRecordset(value, false);
        public static string MakeValueIntoHighLevelRecordset(string value, bool starNotation) => RecSetCommon.MakeValueIntoHighLevelRecordset(value, starNotation);

        public static string ExtractIndexRegionFromRecordset(string rs) => RecSetCommon.ExtractIndexRegionFromRecordset(rs);

        public static bool IsStarIndex(string rs) => RecSetCommon.IsStarIndex(rs);

        public static bool IsFullyEvaluated(string payload)
        {
            var result = payload != null && payload.IndexOf(OpeningSquareBrackets, StringComparison.Ordinal) >= 0
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

        public static bool NotEncrypted(string value) => string.IsNullOrEmpty(value) || IsFullyEvaluated(value);

        /// <summary>
        /// Is the expression evaluated
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        ///   <c>true</c> if the specified payload is evaluated; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEvaluated(string payload)
        {
            var result = payload != null && payload.IndexOf(OpeningSquareBrackets, StringComparison.Ordinal) >= 0;

            return result;
        }

        /// <summary>
        /// Gets the index type of a recorset
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static enRecordsetIndexType GetRecordsetIndexType(string expression) => RecSetCommon.GetRecordsetIndexType(expression);

        //used in the replace node method

        public static bool IsXml(string data) => DataListUtilBase.IsXml(data);
        
        public static bool IsXml(string data, out bool isFragment) => DataListUtilBase.IsXml(data, out isFragment);

        public static bool IsJson(string data)
        {
            var tmp = data.Trim();
            if (tmp.StartsWith("{", StringComparison.Ordinal) && tmp.EndsWith("}", StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }

        public static bool IsXmlOrJson(string data) => IsJson(data) || IsXml(data);

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

        public static string AdjustForEncodingIssues(string payload) => DataListUtilBase.AdjustForEncodingIssues(payload);

        public static string RemoveRecordsetBracketsFromValue(string value) => RecSetCommon.RemoveRecordsetBracketsFromValue(value);

        public static string CreateRecordsetDisplayValue(string recsetName, string colName, string indexNum) => RecSetCommon.CreateRecordsetDisplayValue(recsetName, colName, indexNum);

        public static void UpsertTokens(Collection<ObservablePair<string, string>> target, IDev2Tokenizer tokenizer) => UpsertTokens(target, tokenizer, null, null, true);
        public static void UpsertTokens(Collection<ObservablePair<string, string>> target, IDev2Tokenizer tokenizer, string tokenPrefix, string tokenSuffix, bool removeEmptyEntries)
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
        public static string ReplaceObjectBlankWithIndex(string objectName, int length) => RecSetCommon.ReplaceObjectBlankWithIndex(objectName, length);

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
                    if (!int.TryParse(index, out int val))
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
            var db = new DefinitionBuilder();

            if (direction == enDev2ColumnArgumentDirection.Input)
            {
                db.ArgumentType = enDev2ArgumentType.Input;
            }
            else
            {
                if (direction == enDev2ColumnArgumentDirection.Output)
                {
                    db.ArgumentType = enDev2ArgumentType.Output;
                }
            }

            db.Definitions = GenerateDefsFromDataList(datalist, direction);

            return db.Generate();
        }

        public static IList<IDev2Definition> GenerateDefsFromDataList(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection) => Common.GenerateDefsFromDataList(dataList, dev2ColumnArgumentDirection);

        public static IList<IDev2Definition> GenerateDefsFromDataList(string dataList, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, bool includeNoneDirection, ISearch searchParameters) => Common.GenerateDefsFromDataList(dataList, dev2ColumnArgumentDirection, includeNoneDirection, searchParameters);

        internal static bool CheckIODirection(enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, enDev2ColumnArgumentDirection ioDirection,bool includeNoneDirection)
        {
            if(includeNoneDirection && dev2ColumnArgumentDirection == enDev2ColumnArgumentDirection.None)
            {
                return true;
            }
            if (dev2ColumnArgumentDirection == enDev2ColumnArgumentDirection.Both)
            {
                return (ioDirection == enDev2ColumnArgumentDirection.Input || ioDirection == enDev2ColumnArgumentDirection.Output);
            }
            if (ioDirection == enDev2ColumnArgumentDirection.Both)
            {
                return (dev2ColumnArgumentDirection == enDev2ColumnArgumentDirection.Input || dev2ColumnArgumentDirection == enDev2ColumnArgumentDirection.Output);
            }

            return ioDirection == dev2ColumnArgumentDirection;
        }

        internal static enDev2ColumnArgumentDirection GetDev2ColumnArgumentDirection(XmlNode tmpNode)
        {
            var ioDirectionAttribute = tmpNode.Attributes[GlobalConstants.DataListIoColDirection];

            enDev2ColumnArgumentDirection ioDirection;
            ioDirection = ioDirectionAttribute != null ? (enDev2ColumnArgumentDirection)(Dev2EnumConverter.GetEnumFromStringDiscription(ioDirectionAttribute.Value, typeof(enDev2ColumnArgumentDirection)) ?? enDev2ColumnArgumentDirection.Both) : enDev2ColumnArgumentDirection.Both;
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
            var obj = JsonConvert.DeserializeObject<T>(payload.ToString());

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

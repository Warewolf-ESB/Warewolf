/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;

namespace Dev2.Data.SystemTemplates.Models
{
    public class Dev2Decision : IEquatable<Dev2Decision>
    {
        const int TotalCols = 3;

        public string Col1 { get; set; }

        public string Col2 { get; set; }

        public string Col3 { get; set; }

        public IEnumerable<DataStorage.WarewolfAtom> Cols1 { get; set; }

        public IEnumerable<DataStorage.WarewolfAtom> Cols2 { get; set; }

        public IEnumerable<DataStorage.WarewolfAtom> Cols3 { get; set; }

        public int PopulatedColumnCount
        {
            get
            {
                var cnt = 0;

                if (!string.IsNullOrEmpty(Col1))
                {
                    cnt++;
                }

                if (!string.IsNullOrEmpty(Col2))
                {
                    cnt++;
                }

                if (!string.IsNullOrEmpty(Col3))
                {
                    cnt++;
                }

                return cnt;
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enDecisionType EvaluationFn { get; set; }

        public string GenerateToolLabel(IExecutionEnvironment env, Dev2DecisionMode mode, out ErrorResultTO errors)
        {
            return new ToolLabelGenerator(EvaluationFn, PopulatedColumnCount, Col1, Col2, Col3, env, mode).Generate(out errors);
        }

        public string[] FetchColsAsArray()
        {
            var result = new string[TotalCols];

            if (Col1 == null)
            {
                Col1 = string.Empty;
            }

            if (Col2 == null)
            {
                Col2 = string.Empty;
            }

            if (Col3 == null)
            {
                Col3 = string.Empty;
            }

            result[0] = Col1.Replace("\\\\", "\\");
            result[1] = Col2.Replace("\\\\", "\\");
            result[2] = Col3.Replace("\\\\", "\\");

            return result;
        }
        public bool Equals(Dev2Decision other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var cols1Equal = CommonEqualityOps.CollectionEquals(Cols1, other.Cols1, new WarewolfAtomComparer());
            var cols2Equal = CommonEqualityOps.CollectionEquals(Cols2, other.Cols2, new WarewolfAtomComparer());
            var cols3Equal = CommonEqualityOps.CollectionEquals(Cols3, other.Cols3, new WarewolfAtomComparer());

            var isEqual = string.Equals(Col1, other.Col1);
            isEqual &= string.Equals(Col2, other.Col2);
            isEqual &= string.Equals(Col3, other.Col3);
            isEqual &= cols1Equal;
            isEqual &= cols2Equal;
            isEqual &= cols3Equal;
            isEqual &= EvaluationFn == other.EvaluationFn;

            return isEqual;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Dev2Decision) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Col1 != null ? Col1.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Col2 != null ? Col2.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Col3 != null ? Col3.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Cols1 != null ? Cols1.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Cols2 != null ? Cols2.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Cols3 != null ? Cols3.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) EvaluationFn;
                return hashCode;
            }
        }

        class ToolLabelGenerator
        {
            readonly enDecisionType _evaluationFn;
            readonly int _populatedColumnCount;
            readonly string _col1;
            readonly string _col2;
            readonly string _col3;
            readonly IExecutionEnvironment _env;
            readonly Dev2DecisionMode _mode;

            public ToolLabelGenerator(enDecisionType evaluationFn, int populatedColumnCount, string col1, string col2, string col3, IExecutionEnvironment env, Dev2DecisionMode mode)
            {
                this._evaluationFn = evaluationFn;
                this._populatedColumnCount = populatedColumnCount;
                this._col1 = col1;
                this._col2 = col2;
                this._col3 = col3;
                this._env = env;
                this._mode = mode;
            }

#pragma warning disable S3776, S1541 // Complexity of methods should not be too high
            public string Generate(out ErrorResultTO errors)
#pragma warning restore S3776, S1541 // Complexity of methods should not be too high
            {
                errors = new ErrorResultTO();

                if (_evaluationFn == enDecisionType.Dynamic)
                {
                  return "If Dynamic Expression";
                }

                var fn = DecisionDisplayHelper.GetDisplayValue(_evaluationFn);

                if (_populatedColumnCount == 0)
                {
                    return "If " + fn + " ";
                }

                var allErrors = new ErrorResultTO();

                if (_populatedColumnCount == 1)
                {
                    if (DataListUtil.GetRecordsetIndexType(_col1) == enRecordsetIndexType.Star)
                    {
                        var allValues = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(_col1, _env, out errors, 0);
                        if (errors.FetchErrors().Count >= 1)
                        {
                            return "If ";
                        }
                        allErrors.MergeErrors(errors);
                        var expandStarredIndex = new StringBuilder();

                        expandStarredIndex.Append(allValues[0] + " " + fn);
                        allValues.RemoveAt(0);
                        foreach (var value in allValues)
                        {
                            expandStarredIndex.Append(" " + _mode + " " + value + " " + fn);
                        }
                        errors = allErrors;
                        return "If " + expandStarredIndex;
                    }
                    errors = allErrors;
                    return "If " + _col1 + " " + fn + " ";
                }

                if (_populatedColumnCount == 2)
                {
                    var expandStarredIndices = new StringBuilder();
                    if (DataListUtil.GetRecordsetIndexType(_col1) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col2) == enRecordsetIndexType.Star)
                    {
                        var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(_col2, _env, out errors, 0);
                        if (errors.FetchErrors().Count >= 1)
                        {
                            return "If ";
                        }
                        allErrors.MergeErrors(errors);
                        expandStarredIndices.Append(_col1 + " " + fn + " " + allCol2Values[0]);
                        allCol2Values.RemoveAt(0);
                        foreach (var value in allCol2Values)
                        {
                            expandStarredIndices.Append(" " + _mode + " " + _col1 + " " + fn + " " + value);
                        }
                        errors = allErrors;
                        return "If " + expandStarredIndices;
                    }
                    if (DataListUtil.GetRecordsetIndexType(_col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col2) != enRecordsetIndexType.Star)
                    {
                        var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(_col1, _env, out errors, 0);
                        if (errors.FetchErrors().Count >= 1)
                        {
                            return "If ";
                        }
                        allErrors.MergeErrors(errors);
                        expandStarredIndices.Append(allCol1Values[0] + " " + fn + " " + _col2);
                        allCol1Values.RemoveAt(0);
                        foreach (var value in allCol1Values)
                        {
                            expandStarredIndices.Append(" " + _mode + " " + value + " " + fn + " " + _col2);
                        }
                        errors = allErrors;
                        return "If " + expandStarredIndices;
                    }
                    if (DataListUtil.GetRecordsetIndexType(_col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col2) == enRecordsetIndexType.Star || DataListUtil.GetRecordsetIndexType(_col1) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col2) != enRecordsetIndexType.Star)
                    {
                        var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(_col1, _env, out errors, 0);
                        if (errors.FetchErrors().Count >= 1)
                        {
                            return "If ";
                        }
                        allErrors.MergeErrors(errors);
                        var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(_col2, _env, out errors, 0);
                        allErrors.MergeErrors(errors);
                        expandStarredIndices.Append(allCol1Values[0] + " " + fn + " " + allCol2Values[0]);
                        allCol1Values.RemoveAt(0);
                        if (allCol2Values.Count > 0)
                        {
                            allCol2Values.RemoveAt(0);
                        }
                        AddAllColumns(_mode, errors, allErrors, fn, expandStarredIndices, allCol1Values, allCol2Values);
                        errors = allErrors;
                        return "If " + expandStarredIndices;
                    }
                    errors = allErrors;
                    return "If " + _col1 + " " + fn + " " + _col2 + " ";
                }

                if (_populatedColumnCount == 3)
                {
                    var expandStarredIndices = ResolveStarredIndicesForLabel(_env, _mode.ToString(), out errors);
                    allErrors.MergeErrors(errors);
                    if (!string.IsNullOrEmpty(expandStarredIndices))
                    {
                        errors = allErrors;
                        return expandStarredIndices;
                    }
                    errors = allErrors;
                    return "If " + _col1 + " " + fn + " " + _col2 + " and " + _col3;
                }

                errors = allErrors;
                return "<< Internal Error Generating Decision Model: Populated Column Count Cannot Exceed 3 >>";
            }
            private static void AddAllColumns(Dev2DecisionMode mode, ErrorResultTO errors, ErrorResultTO allErrors, string fn, StringBuilder expandStarredIndices, IList<string> allCol1Values, IList<string> allCol2Values)
            {
                for (var i = 0; i < Math.Max(allCol1Values.Count, allCol2Values.Count); i++)
                {
                    if (i > allCol1Values.Count)
                    {
                        allCol1Values.Add(null);
                    }
                    if (i > allCol2Values.Count)
                    {
                        allCol2Values.Add(null);
                    }

                    try
                    {
                        expandStarredIndices.Append(" " + mode + " " + allCol1Values[i] + " " + fn + " " + allCol2Values[i]);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        errors.AddError(ErrorResource.RecordsetsHaveDifferntSizes);
                        allErrors.MergeErrors(errors);
                    }
                }
            }

#pragma warning disable S3776, S1541 // Complexity of methods should not be too high
            string ResolveStarredIndicesForLabel(IExecutionEnvironment env, string mode, out ErrorResultTO errors)
#pragma warning restore S3776, S1541 // Complexity of methods should not be too high
            {
                var fn = DecisionDisplayHelper.GetDisplayValue(_evaluationFn);
                var expandStarredIndices = new StringBuilder();
                if (DataListUtil.GetRecordsetIndexType(_col1) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col2) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col3) == enRecordsetIndexType.Star)
                {
                    var allCol3Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(_col3, env, out errors, 0);

                    expandStarredIndices.Append(_col1 + " " + fn + " " + _col2 + " AND " + allCol3Values[0]);
                    allCol3Values.RemoveAt(0);
                    foreach (var value in allCol3Values)
                    {
                        expandStarredIndices.Append(" " + mode + " " + _col1 + " " + fn + " " + _col2 + " AND " + value);
                    }
                    return "If " + expandStarredIndices;
                }
                if (DataListUtil.GetRecordsetIndexType(_col1) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col2) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col3) != enRecordsetIndexType.Star)
                {
                    var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(_col2, env, out errors, 0);

                    expandStarredIndices.Append(_col1 + " " + fn + " " + allCol2Values[0] + " AND " + _col3);
                    allCol2Values.RemoveAt(0);
                    foreach (var value in allCol2Values)
                    {
                        expandStarredIndices.Append(" " + mode + " " + _col1 + " " + fn + " " + value + " AND " + _col3);
                    }
                    return "If " + expandStarredIndices;
                }
                if (DataListUtil.GetRecordsetIndexType(_col1) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col2) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col3) == enRecordsetIndexType.Star)
                {
                    var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(_col2, env, out errors, 0);
                    var allCol3Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(_col3, env, out errors, 0);

                    expandStarredIndices.Append(_col1 + " " + fn + " " + allCol2Values[0] + " AND " + allCol3Values[0]);
                    allCol2Values.RemoveAt(0);
                    allCol3Values.RemoveAt(0);
                    for (var i = 0; i < Math.Max(allCol2Values.Count, allCol3Values.Count); i++)
                    {
                        if (i > allCol2Values.Count)
                        {
                            allCol2Values.Add(null);
                        }
                        if (i > allCol3Values.Count)
                        {
                            allCol3Values.Add(null);
                        }
                        expandStarredIndices.Append(" " + mode + " " + _col1 + " " + fn + " " + allCol2Values[i] + " AND " + allCol3Values[i]);
                    }
                    return "If " + expandStarredIndices;
                }
                if (DataListUtil.GetRecordsetIndexType(_col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col2) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col3) != enRecordsetIndexType.Star)
                {
                    var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(_col1, env, out errors, 0);

                    expandStarredIndices.Append(allCol1Values[0] + " " + fn + " " + _col2 + " AND " + _col3);
                    allCol1Values.RemoveAt(0);
                    foreach (var value in allCol1Values)
                    {
                        expandStarredIndices.Append(" " + mode + " " + value + " " + fn + " " + _col2 + " AND " + _col3);
                    }
                    return "If " + expandStarredIndices;
                }
                if (DataListUtil.GetRecordsetIndexType(_col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col2) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col3) == enRecordsetIndexType.Star)
                {
                    var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(_col1, env, out errors, 0);
                    var allCol3Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(_col3, env, out errors, 0);

                    expandStarredIndices.Append(allCol1Values[0] + " " + fn + " " + _col2 + " AND " + allCol3Values[0]);
                    allCol1Values.RemoveAt(0);
                    if (allCol3Values.Count > 0)
                    {
                        allCol3Values.RemoveAt(0);
                    }
                    for (var i = 0; i < Math.Max(allCol1Values.Count, allCol3Values.Count); i++)
                    {
                        if (i > allCol1Values.Count)
                        {
                            allCol1Values.Add(null);
                        }
                        if (i > allCol3Values.Count)
                        {
                            allCol3Values.Add(null);
                        }
                        expandStarredIndices.Append(" " + mode + " " + allCol1Values[i] + " " + fn + " " + _col2 + " AND " + allCol3Values[i]);
                    }
                    return "If " + expandStarredIndices;
                }
                if (DataListUtil.GetRecordsetIndexType(_col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col2) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col3) != enRecordsetIndexType.Star)
                {
                    var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(_col1, env, out errors, 0);
                    var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(_col2, env, out errors, 0);

                    expandStarredIndices.Append(allCol1Values[0] + " " + fn + " " + allCol2Values[0] + " AND " + _col3);
                    allCol1Values.RemoveAt(0);
                    if (allCol2Values.Count > 0)
                    {
                        allCol2Values.RemoveAt(0);
                    }
                    for (var i = 0; i < Math.Max(allCol1Values.Count, allCol2Values.Count); i++)
                    {
                        if (i > allCol1Values.Count)
                        {
                            allCol1Values.Add(null);
                        }
                        if (i > allCol2Values.Count)
                        {
                            allCol2Values.Add(null);
                        }
                        expandStarredIndices.Append(" " + mode + " " + allCol1Values[i] + " " + fn + " " + allCol2Values[0] + " AND " + _col3);
                    }
                    return "If " + expandStarredIndices;
                }
                if (DataListUtil.GetRecordsetIndexType(_col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col2) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(_col3) == enRecordsetIndexType.Star)
                {
                    var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(_col1, env, out errors, 0);
                    var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(_col2, env, out errors, 0);
                    var allCol3Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(_col3, env, out errors, 0);

                    expandStarredIndices.Append(allCol1Values[0] + " " + fn + " " + allCol2Values[0] + " AND " + allCol3Values[0]);
                    allCol1Values.RemoveAt(0);
                    if (allCol2Values.Count > 0)
                    {
                        allCol2Values.RemoveAt(0);
                    }
                    if (allCol3Values.Count > 0)
                    {
                        allCol3Values.RemoveAt(0);
                    }
                    for (var i = 0; i < Math.Max(allCol1Values.Count, Math.Max(allCol2Values.Count, allCol3Values.Count)); i++)
                    {
                        if (i > allCol1Values.Count)
                        {
                            allCol1Values.Add(null);
                        }
                        if (i > allCol2Values.Count)
                        {
                            allCol2Values.Add(null);
                        }
                        if (i > allCol3Values.Count)
                        {
                            allCol3Values.Add(null);
                        }
                        expandStarredIndices.Append(" " + mode + " " + allCol1Values[i] + " " + fn + " " + allCol2Values[0] + " AND " + allCol3Values[i]);
                    }
                    return "If " + expandStarredIndices;
                }
                errors = new ErrorResultTO();
                return null;
            }
        }
    }
}
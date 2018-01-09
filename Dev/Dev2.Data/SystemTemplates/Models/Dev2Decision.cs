/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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

        #region Properties

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

        #endregion Properties

        public string GenerateUserFriendlyModel(IExecutionEnvironment env, Dev2DecisionMode mode, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            var allErrors = new ErrorResultTO();
            var fn = DecisionDisplayHelper.GetDisplayValue(EvaluationFn);

            if (PopulatedColumnCount == 0)
            {
                return "If " + fn + " ";
            }

            if (PopulatedColumnCount == 1)
            {
                if (DataListUtil.GetRecordsetIndexType(Col1) == enRecordsetIndexType.Star)
                {
                    var allValues = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col1, env, out errors, 0);
                    allErrors.MergeErrors(errors);
                    var expandStarredIndex = new StringBuilder();

                    expandStarredIndex.Append(allValues[0] + " " + fn);
                    allValues.RemoveAt(0);
                    foreach (var value in allValues)
                    {
                        expandStarredIndex.Append(" " + mode + " " + value + " " + fn);
                    }
                    errors = allErrors;
                    return "If " + expandStarredIndex;
                }
                errors = allErrors;
                return "If " + Col1 + " " + fn + " ";
            }

            if (PopulatedColumnCount == 2)
            {
                var expandStarredIndices = new StringBuilder();
                if (DataListUtil.GetRecordsetIndexType(Col1) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) == enRecordsetIndexType.Star)
                {
                    var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col2, env, out errors, 0);
                    allErrors.MergeErrors(errors);
                    expandStarredIndices.Append(Col1 + " " + fn + " " + allCol2Values[0]);
                    allCol2Values.RemoveAt(0);
                    foreach (var value in allCol2Values)
                    {
                        expandStarredIndices.Append(" " + mode + " " + Col1 + " " + fn + " " + value);
                    }
                    errors = allErrors;
                    return "If " + expandStarredIndices;
                }
                if (DataListUtil.GetRecordsetIndexType(Col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) != enRecordsetIndexType.Star)
                {
                    var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col1, env, out errors, 0);
                    allErrors.MergeErrors(errors);
                    expandStarredIndices.Append(allCol1Values[0] + " " + fn + " " + Col2);
                    allCol1Values.RemoveAt(0);
                    foreach (var value in allCol1Values)
                    {
                        expandStarredIndices.Append(" " + mode + " " + value + " " + fn + " " + Col2);
                    }
                    errors = allErrors;
                    return "If " + expandStarredIndices;
                }
                if (DataListUtil.GetRecordsetIndexType(Col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) == enRecordsetIndexType.Star || DataListUtil.GetRecordsetIndexType(Col1) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) != enRecordsetIndexType.Star)
                {
                    var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col1, env, out errors, 0);
                    allErrors.MergeErrors(errors);
                    var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col2, env, out errors, 0);
                    allErrors.MergeErrors(errors);
                    expandStarredIndices.Append(allCol1Values[0] + " " + fn + " " + allCol2Values[0]);
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

                        try
                        {
                            expandStarredIndices.Append(" " + mode + " " + allCol1Values[i] + " " + fn + " " +
                                                        allCol2Values[i]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            errors.AddError(ErrorResource.RecordsetsHaveDifferntSizes);
                            allErrors.MergeErrors(errors);
                        }
                    }
                    errors = allErrors;
                    return "If " + expandStarredIndices;
                }
                errors = allErrors;
                return "If " + Col1 + " " + fn + " " + Col2 + " ";
            }

            if (PopulatedColumnCount == 3)
            {
                var expandStarredIndices = ResolveStarredIndices(env, mode.ToString(), out errors);
                allErrors.MergeErrors(errors);
                if (!string.IsNullOrEmpty(expandStarredIndices))
                {
                    errors = allErrors;
                    return expandStarredIndices;
                }
                errors = allErrors;
                return "If " + Col1 + " " + fn + " " + Col2 + " and " + Col3;
            }
            errors = allErrors;
            return "<< Internal Error Generating Decision Model: Populated Column Count Cannot Exceed 3 >>";
        }

        string ResolveStarredIndices(IExecutionEnvironment env, string mode, out ErrorResultTO errors)
        {
            var fn = DecisionDisplayHelper.GetDisplayValue(EvaluationFn);
            var expandStarredIndices = new StringBuilder();
            if (DataListUtil.GetRecordsetIndexType(Col1) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col3) == enRecordsetIndexType.Star)
            {
                var allCol3Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col3, env, out errors, 0);

                expandStarredIndices.Append(Col1 + " " + fn + " " + Col2 + " AND " + allCol3Values[0]);
                allCol3Values.RemoveAt(0);
                foreach (var value in allCol3Values)
                {
                    expandStarredIndices.Append(" " + mode + " " + Col1 + " " + fn + " " + Col2 + " AND " + value);
                }
                return "If " + expandStarredIndices;
            }
            if (DataListUtil.GetRecordsetIndexType(Col1) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col3) != enRecordsetIndexType.Star)
            {
                var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col2, env, out errors, 0);

                expandStarredIndices.Append(Col1 + " " + fn + " " + allCol2Values[0] + " AND " + Col3);
                allCol2Values.RemoveAt(0);
                foreach (var value in allCol2Values)
                {
                    expandStarredIndices.Append(" " + mode + " " + Col1 + " " + fn + " " + value + " AND " + Col3);
                }
                return "If " + expandStarredIndices;
            }
            if (DataListUtil.GetRecordsetIndexType(Col1) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col3) == enRecordsetIndexType.Star)
            {
                var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col2, env, out errors, 0);
                var allCol3Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col3, env, out errors, 0);

                expandStarredIndices.Append(Col1 + " " + fn + " " + allCol2Values[0] + " AND " + allCol3Values[0]);
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
                    expandStarredIndices.Append(" " + mode + " " + Col1 + " " + fn + " " + allCol2Values[i] + " AND " + allCol3Values[i]);
                }
                return "If " + expandStarredIndices;
            }
            if (DataListUtil.GetRecordsetIndexType(Col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col3) != enRecordsetIndexType.Star)
            {
                var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col1, env, out errors, 0);

                expandStarredIndices.Append(allCol1Values[0] + " " + fn + " " + Col2 + " AND " + Col3);
                allCol1Values.RemoveAt(0);
                foreach (var value in allCol1Values)
                {
                    expandStarredIndices.Append(" " + mode + " " + value + " " + fn + " " + Col2 + " AND " + Col3);
                }
                return "If " + expandStarredIndices;
            }
            if (DataListUtil.GetRecordsetIndexType(Col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col3) == enRecordsetIndexType.Star)
            {
                var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col1, env, out errors, 0);
                var allCol3Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col3, env, out errors, 0);

                expandStarredIndices.Append(allCol1Values[0] + " " + fn + " " + Col2 + " AND " + allCol3Values[0]);
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
                    expandStarredIndices.Append(" " + mode + " " + allCol1Values[i] + " " + fn + " " + Col2 + " AND " + allCol3Values[i]);
                }
                return "If " + expandStarredIndices;
            }
            if (DataListUtil.GetRecordsetIndexType(Col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col3) != enRecordsetIndexType.Star)
            {
                var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col1, env, out errors, 0);
                var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col2, env, out errors, 0);

                expandStarredIndices.Append(allCol1Values[0] + " " + fn + " " + allCol2Values[0] + " AND " + Col3);
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
                    expandStarredIndices.Append(" " + mode + " " + allCol1Values[i] + " " + fn + " " + allCol2Values[0] + " AND " + Col3);
                }
                return "If " + expandStarredIndices;
            }
            if (DataListUtil.GetRecordsetIndexType(Col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col3) == enRecordsetIndexType.Star)
            {
                var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col1, env, out errors, 0);
                var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col2, env, out errors, 0);
                var allCol3Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col3, env, out errors, 0);

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
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            var cols1Equal = CommonEqualityOps.CollectionEquals(Cols1, other.Cols1, new WarewolfAtomComparer());
            var cols2Equal = CommonEqualityOps.CollectionEquals(Cols2, other.Cols2, new WarewolfAtomComparer());
            var cols3Equal = CommonEqualityOps.CollectionEquals(Cols3, other.Cols3, new WarewolfAtomComparer());
            return string.Equals(Col1, other.Col1) 
                && string.Equals(Col2, other.Col2) 
                && string.Equals(Col3, other.Col3) 
                && cols1Equal
                && cols2Equal
                && cols3Equal
                && EvaluationFn == other.EvaluationFn;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
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
    }
}
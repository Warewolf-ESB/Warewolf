
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Text;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Data.SystemTemplates.Models
{
    public class Dev2Decision : IDev2DataModel
    {
        private const int TotalCols = 3;

        #region Properties

        public string Col1 { get; set; }

        public string Col2 { get; set; }

        public string Col3 { get; set; }

        public int PopulatedColumnCount
        {
            get
            {
                int cnt = 0;

                if(!string.IsNullOrEmpty(Col1))
                {
                    cnt++;
                }

                if(!string.IsNullOrEmpty(Col2))
                {
                    cnt++;
                }

                if(!string.IsNullOrEmpty(Col3))
                {
                    cnt++;
                }

                return cnt;
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enDecisionType EvaluationFn { get; set; }

        [JsonIgnore]
        public string Version { get { return "1.0.0"; } }

        [JsonIgnore]
        public Dev2ModelType ModelName { get { return Dev2ModelType.Dev2Decision; } }

        #endregion

        public string ToWebModel()
        {
            string result = JsonConvert.SerializeObject(this);

            return result;
        }

        public string GenerateUserFriendlyModel(Guid dlid, Dev2DecisionMode mode, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            string fn = DecisionDisplayHelper.GetDisplayValue(EvaluationFn);

            if(PopulatedColumnCount == 0)
            {
                return "If " + fn + " ";
            }

            if(PopulatedColumnCount == 1)
            {
                if(DataListUtil.GetRecordsetIndexType(Col1) == enRecordsetIndexType.Star)
                {
                    var allValues = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col1, dlid, out errors);
                    allErrors.MergeErrors(errors);
                    StringBuilder expandStarredIndex = new StringBuilder();

                    expandStarredIndex.Append(allValues[0] + " " + fn);
                    allValues.RemoveAt(0);
                    foreach(var value in allValues)
                    {
                        expandStarredIndex.Append(" " + mode + " " + value + " " + fn);
                    }
                    errors = allErrors;
                    return "If " + expandStarredIndex;
                }
                errors = allErrors;
                return "If " + Col1 + " " + fn + " ";
            }

            if(PopulatedColumnCount == 2)
            {
                StringBuilder expandStarredIndices = new StringBuilder();
                if(DataListUtil.GetRecordsetIndexType(Col1) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) == enRecordsetIndexType.Star)
                {
                    var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col2, dlid, out errors);
                    allErrors.MergeErrors(errors);
                    expandStarredIndices.Append(Col1 + " " + fn + " " + allCol2Values[0]);
                    allCol2Values.RemoveAt(0);
                    foreach(var value in allCol2Values)
                    {
                        expandStarredIndices.Append(" " + mode + " " + Col1 + " " + fn + " " + value);
                    }
                    errors = allErrors;
                    return "If " + expandStarredIndices;
                }
                if(DataListUtil.GetRecordsetIndexType(Col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) != enRecordsetIndexType.Star)
                {
                    var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col1, dlid, out errors);
                    allErrors.MergeErrors(errors);
                    expandStarredIndices.Append(allCol1Values[0] + " " + fn + " " + Col2);
                    allCol1Values.RemoveAt(0);
                    foreach(var value in allCol1Values)
                    {
                        expandStarredIndices.Append(" " + mode + " " + value + " " + fn + " " + Col2);
                    }
                    errors = allErrors;
                    return "If " + expandStarredIndices;

                }
                if((DataListUtil.GetRecordsetIndexType(Col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) == enRecordsetIndexType.Star) || (DataListUtil.GetRecordsetIndexType(Col1) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) != enRecordsetIndexType.Star))
                {
                    var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col1, dlid, out errors);
                    allErrors.MergeErrors(errors);
                    var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col2, dlid, out errors);
                    allErrors.MergeErrors(errors);
                    expandStarredIndices.Append(allCol1Values[0] + " " + fn + " " + allCol2Values[0]);
                    allCol1Values.RemoveAt(0);
                    allCol2Values.RemoveAt(0);
                    for(var i = 0; i < Math.Max(allCol1Values.Count, allCol2Values.Count); i++)
                    {
                        if(i > allCol1Values.Count)
                        {
                            allCol1Values.Add(null);
                        }
                        if(i > allCol2Values.Count)
                        {
                            allCol2Values.Add(null);
                        }

                        try
                        {
                            expandStarredIndices.Append(" " + mode + " " + allCol1Values[i] + " " + fn + " " +
                                                        allCol2Values[i]);
                        }
                        catch(IndexOutOfRangeException)
                        {
                            errors.AddError("You appear to have recordsets of differnt sizes");
                            allErrors.MergeErrors(errors);
                        }
                    }
                    errors = allErrors;
                    return "If " + expandStarredIndices;

                }
                errors = allErrors;
                return "If " + Col1 + " " + fn + " " + Col2 + " ";
            }

            if(PopulatedColumnCount == 3)
            {
                var expandStarredIndices = ResolveStarredIndices(dlid, mode.ToString(), out errors);
                allErrors.MergeErrors(errors);
                if(!string.IsNullOrEmpty(expandStarredIndices))
                {
                    errors = allErrors;
                    return expandStarredIndices;
                }
                errors = allErrors;
                return "If " + Col1 + " " + fn + " " + Col2 + " and " + Col3;
            }
            errors = allErrors;
            return "<< Internal Error Generating Decision Model: Populated Column Count Cannot Exeed 3 >>";
        }

        string ResolveStarredIndices(Guid dlid, string mode, out ErrorResultTO errors)
        {
            string fn = DecisionDisplayHelper.GetDisplayValue(EvaluationFn);
            StringBuilder expandStarredIndices = new StringBuilder();
            if(DataListUtil.GetRecordsetIndexType(Col1) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col3) == enRecordsetIndexType.Star)
            {
                var allCol3Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col3, dlid, out errors);

                expandStarredIndices.Append(Col1 + " " + fn + " " + Col2 + " AND " + allCol3Values[0]);
                allCol3Values.RemoveAt(0);
                foreach(var value in allCol3Values)
                {
                    expandStarredIndices.Append(" " + mode + " " + Col1 + " " + fn + " " + Col2 + " AND " + value);
                }
                return "If " + expandStarredIndices;
            }
            if(DataListUtil.GetRecordsetIndexType(Col1) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col3) != enRecordsetIndexType.Star)
            {
                var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col2, dlid, out errors);

                expandStarredIndices.Append(Col1 + " " + fn + " " + allCol2Values[0] + " AND " + Col3);
                allCol2Values.RemoveAt(0);
                foreach(var value in allCol2Values)
                {
                    expandStarredIndices.Append(" " + mode + " " + Col1 + " " + fn + " " + value + " AND " + Col3);
                }
                return "If " + expandStarredIndices;
            }
            if(DataListUtil.GetRecordsetIndexType(Col1) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col3) == enRecordsetIndexType.Star)
            {
                var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col2, dlid, out errors);
                var allCol3Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col3, dlid, out errors);

                expandStarredIndices.Append(Col1 + " " + fn + " " + allCol2Values[0] + " AND " + allCol3Values[0]);
                allCol2Values.RemoveAt(0);
                allCol3Values.RemoveAt(0);
                for(var i = 0; i < Math.Max(allCol2Values.Count, allCol3Values.Count); i++)
                {
                    if(i > allCol2Values.Count)
                    {
                        allCol2Values.Add(null);
                    }
                    if(i > allCol3Values.Count)
                    {
                        allCol3Values.Add(null);
                    }
                    expandStarredIndices.Append(" " + mode + " " + Col1 + " " + fn + " " + allCol2Values[i] + " AND " + allCol3Values[i]);
                }
                return "If " + expandStarredIndices;
            }
            if(DataListUtil.GetRecordsetIndexType(Col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col3) != enRecordsetIndexType.Star)
            {
                var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col1, dlid, out errors);

                expandStarredIndices.Append(allCol1Values[0] + " " + fn + " " + Col2 + " AND " + Col3);
                allCol1Values.RemoveAt(0);
                foreach(var value in allCol1Values)
                {
                    expandStarredIndices.Append(" " + mode + " " + value + " " + fn + " " + Col2 + " AND " + Col3);
                }
                return "If " + expandStarredIndices;
            }
            if(DataListUtil.GetRecordsetIndexType(Col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) != enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col3) == enRecordsetIndexType.Star)
            {
                var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col1, dlid, out errors);
                var allCol3Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col3, dlid, out errors);

                expandStarredIndices.Append(allCol1Values[0] + " " + fn + " " + Col2 + " AND " + allCol3Values[0]);
                allCol1Values.RemoveAt(0);
                allCol3Values.RemoveAt(0);
                for(var i = 0; i < Math.Max(allCol1Values.Count, allCol3Values.Count); i++)
                {
                    if(i > allCol1Values.Count)
                    {
                        allCol1Values.Add(null);
                    }
                    if(i > allCol3Values.Count)
                    {
                        allCol3Values.Add(null);
                    }
                    expandStarredIndices.Append(" " + mode + " " + allCol1Values[i] + " " + fn + " " + Col2 + " AND " + allCol3Values[i]);
                }
                return "If " + expandStarredIndices;
            }
            if(DataListUtil.GetRecordsetIndexType(Col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col3) != enRecordsetIndexType.Star)
            {
                var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col1, dlid, out errors);
                var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col2, dlid, out errors);

                expandStarredIndices.Append(allCol1Values[0] + " " + fn + " " + allCol2Values[0] + " AND " + Col3);
                allCol1Values.RemoveAt(0);
                allCol2Values.RemoveAt(0);
                for(var i = 0; i < Math.Max(allCol1Values.Count, allCol2Values.Count); i++)
                {
                    if(i > allCol1Values.Count)
                    {
                        allCol1Values.Add(null);
                    }
                    if(i > allCol2Values.Count)
                    {
                        allCol2Values.Add(null);
                    }
                    expandStarredIndices.Append(" " + mode + " " + allCol1Values[i] + " " + fn + " " + allCol2Values[0] + " AND " + Col3);
                }
                return "If " + expandStarredIndices;
            }
            if(DataListUtil.GetRecordsetIndexType(Col1) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col2) == enRecordsetIndexType.Star && DataListUtil.GetRecordsetIndexType(Col3) == enRecordsetIndexType.Star)
            {
                var allCol1Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col1, dlid, out errors);
                var allCol2Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col2, dlid, out errors);
                var allCol3Values = DataListUtil.GetAllPossibleExpressionsForFunctionOperations(Col3, dlid, out errors);

                expandStarredIndices.Append(allCol1Values[0] + " " + fn + " " + allCol2Values[0] + " AND " + allCol3Values[0]);
                allCol1Values.RemoveAt(0);
                allCol2Values.RemoveAt(0);
                allCol3Values.RemoveAt(0);
                for(var i = 0; i < Math.Max(allCol1Values.Count, Math.Max(allCol2Values.Count, allCol3Values.Count)); i++)
                {
                    if(i > allCol1Values.Count)
                    {
                        allCol1Values.Add(null);
                    }
                    if(i > allCol2Values.Count)
                    {
                        allCol2Values.Add(null);
                    }
                    if(i > allCol3Values.Count)
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

        /// <summary>
        /// Fetches the cols as array.
        /// </summary>
        /// <returns></returns>
        public string[] FetchColsAsArray()
        {
            string[] result = new string[TotalCols];

            if(Col1 == null)
            {
                Col1 = string.Empty;
            }

            if(Col2 == null)
            {
                Col2 = string.Empty;
            }

            if(Col3 == null)
            {
                Col3 = string.Empty;
            }


            result[0] = Col1.Replace("\\\\", "\\");
            result[1] = Col2.Replace("\\\\", "\\");
            result[2] = Col3.Replace("\\\\", "\\");


            return result;

        }

    }
}


/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Convertors.Case;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.Factories;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Validation;
using Warewolf.Core;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
      [ToolDescriptorInfo("Data-CaseConversion", "Case Convert", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Bob", "1.0.0.0", "c:\\", "Data", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]

    public class DsfCaseConvertActivity : DsfActivityAbstract<string>, ICollectionActivity
    {
        #region Properties

        public IList<ICaseConvertTO> ConvertCollection { get; set; }

        #endregion Properties

        #region Ctor

        /// <summary>
        /// The consructor for the activity 
        /// </summary>
        public DsfCaseConvertActivity()
            : base("Case Conversion")
        {
            ConvertCollection = new List<ICaseConvertTO>();
        }

        #endregion Ctor

        #region Overridden NativeActivity Methods

        // ReSharper disable RedundantOverridenMember
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }
        // ReSharper restore RedundantOverridenMember

        /// <summary>
        /// The execute method that is called when the activity is executed at run time and will hold all the logic of the activity
        /// </summary>       
        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();

            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
            toUpsert.IsDebug = dataObject.IsDebugMode();
            toUpsert.ReplaceStarWithFixedIndex = true;

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = DataListExecutionID.Get(context);
            InitializeDebug(dataObject);
            try
            {
                CleanArgs();
                ICaseConverter converter = CaseConverterFactory.CreateCaseConverter();

                allErrors.MergeErrors(errors);

                int index = 1;
                int outIndex = 0;
                foreach(ICaseConvertTO item in ConvertCollection)
                {
                    outIndex++;
                    IBinaryDataListEntry tmp = compiler.Evaluate(executionId, enActionType.User, item.StringToConvert, false, out errors);
                    allErrors.MergeErrors(errors);
                    ValidateVariable(item.Result, compiler, dataObject, out errors);
                    allErrors.MergeErrors(errors);
                    IsSingleValueRule.ApplyIsSingleValueRule(item.ExpressionToConvert, allErrors);
                    if(dataObject.IsDebugMode())
                    {
                        var debugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("", index.ToString(CultureInfo.InvariantCulture)), debugItem);
                        AddDebugItem(new DebugItemVariableParams(item.StringToConvert, "Convert", tmp, executionId), debugItem);
                        AddDebugItem(new DebugItemStaticDataParams(item.ConvertType, "To"), debugItem);
                        _debugInputs.Add(debugItem);
                        index++;
                    }

                    if(tmp != null)
                    {

                        IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(tmp);

                        while(itr.HasMoreRecords())
                        {

                            foreach(IBinaryDataListItem itm in itr.FetchNextRowData())
                            {
                                try
                                {
                                    IBinaryDataListItem res = converter.TryConvert(item.ConvertType, itm);
                                    string expression = item.Result;

                                    // 27.08.2013
                                    // NOTE : The result must remain [ as this is how the fliping studio generates the result when using (*) notation
                                    // There is a proper bug in to fix this issue, but since the studio is spaghetti I will leave this to the experts ;)
                                    // This is a tmp fix to the issue
                                    if(expression == "[" || DataListUtil.GetRecordsetIndexType(expression) == enRecordsetIndexType.Star)
                                    {
                                        expression = DataListUtil.AddBracketsToValueIfNotExist(res.DisplayValue);
                                    }
                                    //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                                    IsSingleValueRule rule = new IsSingleValueRule(() => expression);
                                    var singleresError = rule.Check();
                                    if(singleresError != null)
                                        allErrors.AddError(singleresError.Message);
                                    else
                                    {
                                        toUpsert.Add(expression, res.TheValue);
                                        // Upsert the entire payload
                                    }
                                    allErrors.MergeErrors(errors);
                                }
                                catch(Exception e)
                                {
                                    allErrors.AddError(e.Message);
                                    toUpsert.Add(item.Result, null);
                                }
                            }
                        }
                        compiler.Upsert(executionId, toUpsert, out errors);
                        if(!allErrors.HasErrors() && dataObject.IsDebugMode())
                        {
                            foreach(var debugOutputTo in toUpsert.DebugOutputs)
                            {
                                var debugItem = new DebugItem();
                                AddDebugItem(new DebugItemStaticDataParams("", outIndex.ToString(CultureInfo.InvariantCulture)), debugItem);
                                AddDebugItem(new DebugItemVariableParams(debugOutputTo), debugItem);
                                _debugOutputs.Add(debugItem);
                            }
                            toUpsert.DebugOutputs.Clear();
                        }
                    }
                }

            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfCaseConvertActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebugMode())
                {
                    if(hasErrors)
                    {
                        int outIndex = 1;
                        foreach(ICaseConvertTO item in ConvertCollection)
                        {
                            IBinaryDataListEntry tmp = compiler.Evaluate(executionId, enActionType.User, item.StringToConvert, false, out errors);
                            var debugItem = new DebugItem();
                            AddDebugItem(new DebugItemStaticDataParams("", outIndex.ToString(CultureInfo.InvariantCulture)), debugItem);
                            AddDebugItem(new DebugItemVariableParams(item.Result, "", tmp, executionId), debugItem);
                            _debugOutputs.Add(debugItem);
                            outIndex++;
                        }
                    }
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        static void ValidateVariable(string fieldName, IDataListCompiler compiler, IDSFDataObject dataObject, out ErrorResultTO errors)
        {
            fieldName = DataListUtil.IsValueRecordset(fieldName) ? DataListUtil.ReplaceRecordsetIndexWithBlank(fieldName) : fieldName;
            var datalist = compiler.ConvertFrom(dataObject.DataListID, DataListFormat.CreateFormat(GlobalConstants._Studio_XML), Dev2.DataList.Contract.enTranslationDepth.Shape, out errors).ToString();
            if(!string.IsNullOrEmpty(datalist))
            {
                var isValidExpr = new IsValidExpressionRule(() => fieldName, datalist)
                {
                    LabelText = fieldName
                };

                var errorInfo = isValidExpr.Check();
                if(errorInfo != null)
                {
                    errors.AddError(errorInfo.Message);
                }

            }
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

        #endregion

        #region Private Methods

        private List<string> BreakIntoTokens(string value)
        {
            var parts = value.Split(',');
            var result = parts.Select(r => r.Trim()).ToList();
            return result;
        }

        private void CleanArgs()
        {
            ICaseConvertTO[] workItems = new ICaseConvertTO[ConvertCollection.Count];
            ConvertCollection.CopyTo(workItems, 0);

            // ReSharper disable ForCanBeConvertedToForeach
            for(var i = 0; i < workItems.Length; i++)
            // ReSharper restore ForCanBeConvertedToForeach
            {
                var convertResult = workItems[i].Result;
                var convertTarget = workItems[i].StringToConvert;

                if(!string.IsNullOrEmpty(convertTarget) && !string.IsNullOrEmpty(convertResult))
                {
                    var targetList = BreakIntoTokens(convertTarget);
                    var resultList = BreakIntoTokens(convertResult);

                    // now add them back together
                    if(targetList.Count > 0 && resultList.Count > 0)
                    {
                        // build up the StringToConvert section ;)
                        // existing record
                        ConvertCollection[i].StringToConvert = targetList[0];
                        ConvertCollection[i].Result = resultList[0];
                        var canidateResult = resultList[0];
                        for(var q = 1; q < targetList.Count; q++)
                        {
                            var pos = ConvertCollection.Count + 1;

                            // now process all new results ;)
                            // we always keep the last value in-case we run out of indexes 
                            // as they do not have to balance ;)
                            if(q < resultList.Count)
                            {
                                canidateResult = resultList[q];
                            }

                            ConvertCollection.Add(new CaseConvertTO(targetList[q], ConvertCollection[i].ConvertType, canidateResult, pos));
                        }
                    }
                }
                else
                {
                    ConvertCollection.RemoveAt(i);
                }
            }

        }

        private void InsertToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ConvertCollection"];
            if(modelProperty != null)
            {
                ModelItemCollection mic = modelProperty.Collection;

                if(mic != null)
                {
                    List<ICaseConvertTO> listOfValidRows = ConvertCollection.Where(c => !c.CanRemove()).ToList();
                    if(listOfValidRows.Count > 0)
                    {
                        int startIndex = ConvertCollection.IndexOf(listOfValidRows.Last()) + 1;
                        foreach(string s in listToAdd)
                        {
                            mic.Insert(startIndex, new CaseConvertTO(s, ConvertCollection[startIndex - 1].ConvertType, s, startIndex + 1));
                            startIndex++;
                        }
                        CleanUpCollection(mic, modelItem, startIndex);
                    }
                    else
                    {
                        AddToCollection(listToAdd, modelItem);
                    }
                }
            }
        }

        private void AddToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ConvertCollection"];
            if(modelProperty != null)
            {
                ModelItemCollection mic = modelProperty.Collection;

                if(mic != null)
                {
                    int startIndex = 0;
                    string firstRowConvertType = ConvertCollection[0].ConvertType;
                    mic.Clear();
                    foreach(string s in listToAdd)
                    {
                        mic.Add(new CaseConvertTO(s, firstRowConvertType, s, startIndex + 1));
                        startIndex++;
                    }
                    CleanUpCollection(mic, modelItem, startIndex);
                }
            }
        }

        private void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if(startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new CaseConvertTO(string.Empty, "UPPER", string.Empty, startIndex + 1));
            var modelProperty = modelItem.Properties["DisplayName"];
            if(modelProperty != null)
            {
                modelProperty.SetValue(CreateDisplayName(modelItem, startIndex + 1));
            }
        }

        private string CreateDisplayName(ModelItem modelItem, int count)
        {
            var modelProperty = modelItem.Properties["DisplayName"];
            if(modelProperty != null)
            {
                string currentName = modelProperty.ComputedValue as string;
                if(currentName != null && (currentName.Contains("(") && currentName.Contains(")")))
                {
                    currentName = currentName.Remove(currentName.Contains(" (") ? currentName.IndexOf(" (", StringComparison.Ordinal) : currentName.IndexOf("(", StringComparison.Ordinal));
                }
                currentName = currentName + " (" + (count - 1) + ")";
                return currentName;
            }

            return string.Empty;
        }

        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }


        #endregion

        #region Get ForEach Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach(Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                Tuple<string, string> t1 = t;
                var items = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.StringToConvert) && c.StringToConvert.Contains(t1.Item1));

                // issues updates
                foreach(var a in items)
                {
                    a.StringToConvert = a.StringToConvert.Replace(t.Item1, t.Item2);
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {

            foreach(Tuple<string, string> t in updates)
            {

                // locate all updates for this tuple
                Tuple<string, string> t1 = t;
                var items = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.Result) && c.Result.Contains(t1.Item1));

                // issues updates
                foreach(var a in items)
                {
                    a.Result = a.Result.Replace(t.Item1, t.Item2);
                }
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var result = new List<DsfForEachItem>();

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(var item in ConvertCollection)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                if(!string.IsNullOrEmpty(item.StringToConvert) && item.StringToConvert.Contains("[["))
                {
                    result.Add(new DsfForEachItem { Name = item.StringToConvert, Value = item.Result });
                }
            }

            return result;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var result = new List<DsfForEachItem>();

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(var item in ConvertCollection)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                if(!string.IsNullOrEmpty(item.StringToConvert) && item.StringToConvert.Contains("[["))
                {
                    result.Add(new DsfForEachItem { Name = item.Result, Value = item.StringToConvert });
                }
            }

            return result;
        }

        #endregion

        #region Implementation of ICollectionActivity

        public int GetCollectionCount()
        {
            return ConvertCollection.Count(caseConvertTo => !caseConvertTo.CanRemove());
        }

        public void AddListToCollection(IList<string> listToAdd, bool overwrite, ModelItem modelItem)
        {
            if(!overwrite)
            {
                InsertToCollection(listToAdd, modelItem);
            }
            else
            {
                AddToCollection(listToAdd, modelItem);
            }
        }

        #endregion
    }
}

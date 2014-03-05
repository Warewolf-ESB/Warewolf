using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Data.Factories;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;
using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfCaseConvertActivity : DsfActivityAbstract<string>, ICollectionActivity
    {

        #region Properties

        /// <summary>
        /// The property that holds all the convertions
        /// </summary>
        private IList<ICaseConvertTO> _wtf;
        public IList<ICaseConvertTO> ConvertCollection
        {
            get { return _wtf; }
            set { _wtf = value; }
        }

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
                                foreach(var region in DataListCleaningUtils.SplitIntoRegions(expression))
                                {
                                    toUpsert.Add(region, res.TheValue);
                                }
                                // Upsert the entire payload

                                allErrors.MergeErrors(errors);


                            }
                        }
                        compiler.Upsert(executionId, toUpsert, out errors);
                        if(!allErrors.HasErrors() && dataObject.IsDebugMode())
                        {
                            foreach(var debugOutputTO in toUpsert.DebugOutputs)
                            {
                                var debugItem = new DebugItem();
                                AddDebugItem(new DebugItemStaticDataParams("", outIndex.ToString(CultureInfo.InvariantCulture)), debugItem);
                                AddDebugItem(new DebugItemVariableParams(debugOutputTO), debugItem);
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

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

        #endregion

        #region Private Methods
        private void CleanArgs()
        {
            int count = 0;
            while(count < ConvertCollection.Count)
            {
                //2013.06.03: Ashley Lewis for bug 9498 - Clean line breaks out of result and put them in their own CaseConvertTO
                var result = ConvertCollection[count].Result;
                var input = ConvertCollection[count].StringToConvert;
                string[] openResultParts = Regex.Split(result, @"\[\[");
                string[] closedResultParts = Regex.Split(result, @"\]\]");
                string[] openInputParts = Regex.Split(input, @"\[\[");
                string[] closedInputParts = Regex.Split(input, @"\]\]");
                if(openResultParts.Count() == closedResultParts.Count() && openResultParts.Count() > 2 && closedResultParts.Count() > 2)
                {
                    string cleanResult;
                    if(openResultParts[1].IndexOf("]]") + 2 < openResultParts[1].Length)
                    {
                        cleanResult = "[[" + openResultParts[1].Remove(openResultParts[1].IndexOf("]]") + 2);
                    }
                    else
                    {
                        cleanResult = "[[" + openResultParts[1];
                    }
                    ConvertCollection[count].Result = cleanResult;
                    ConvertCollection[count].StringToConvert = cleanResult;

                    //Add back data after line break
                    ICaseConvertTO splitLineBreak = new CaseConvertTO();
                    splitLineBreak.Result = result.Substring(result.IndexOf(openResultParts[2]) - 2, result.Length - result.IndexOf(openResultParts[2]) + 2);
                    splitLineBreak.StringToConvert = splitLineBreak.Result;

                    //Add back to the end to avoid disturbing index numbering
                    splitLineBreak.IndexNumber = ConvertCollection.Count + 1;
                    splitLineBreak.ConvertType = ConvertCollection[count].ConvertType;
                    ConvertCollection.Add(splitLineBreak);
                }
                else if(openInputParts.Count() == closedInputParts.Count() && openInputParts.Count() > 2 && closedInputParts.Count() > 2)
                {
                    //Handle corrupted result
                    ConvertCollection[count].Result += input.Substring(input.IndexOf("]]") + 2, input.Length - input.IndexOf("]]") - 2);
                }
                else if(string.IsNullOrWhiteSpace(ConvertCollection[count].StringToConvert))
                {
                    ConvertCollection.RemoveAt(count);
                }
                else
                {
                    count++;
                }
            }
        }

        private void InsertToCollection(IList<string> listToAdd, ModelItem modelItem)
        {
            ModelItemCollection mic = modelItem.Properties["ConvertCollection"].Collection;

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

        private void AddToCollection(IList<string> listToAdd, ModelItem modelItem)
        {
            ModelItemCollection mic = modelItem.Properties["ConvertCollection"].Collection;

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

        private void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if(startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new CaseConvertTO(string.Empty, "UPPER", string.Empty, startIndex + 1));
            modelItem.Properties["DisplayName"].SetValue(CreateDisplayName(modelItem, startIndex + 1));
        }

        private string CreateDisplayName(ModelItem modelItem, int count)
        {
            string currentName = modelItem.Properties["DisplayName"].ComputedValue as string;
            if(currentName != null && (currentName.Contains("(") && currentName.Contains(")")))
            {
                if(currentName.Contains(" ("))
                {
                    currentName = currentName.Remove(currentName.IndexOf(" ("));
                }
                else
                {
                    currentName = currentName.Remove(currentName.IndexOf("("));
                }
            }
            currentName = currentName + " (" + (count - 1) + ")";
            return currentName;
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

            foreach(var item in ConvertCollection)
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

            foreach(var item in ConvertCollection)
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
            return ConvertCollection.Count(caseConvertTO => !caseConvertTO.CanRemove());
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

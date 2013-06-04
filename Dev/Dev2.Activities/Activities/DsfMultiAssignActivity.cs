using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;
using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfMultiAssignActivity : DsfActivityAbstract<string>, ICollectionActivity
    {
        #region Constants
        public const string CalculateTextConvertPrefix = GlobalConstants.CalculateTextConvertPrefix;
        public const string CalculateTextConvertSuffix = GlobalConstants.CalculateTextConvertSuffix;
        public const string CalculateTextConvertFormat = GlobalConstants.CalculateTextConvertFormat;
        #endregion

        #region Fields

        private IList<IDebugItem> _debugOutputs = new List<IDebugItem>();

        private IList<ActivityDTO> _fieldsCollection;

        #endregion

        #region Properties

        public IList<ActivityDTO> FieldsCollection
        {
            get
            {
                return _fieldsCollection;
            }
            set
            {
                _fieldsCollection = value;
                OnPropertyChanged("FieldsCollection");
            }
        }

        public bool UpdateAllOccurrences { get; set; }
        public bool CreateBookmark { get; set; }
        public string ServiceHost { get; set; }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Ctor

        public DsfMultiAssignActivity()
            : base("Assign")
        {
            _fieldsCollection = new List<ActivityDTO>();
        }

        #endregion        

        #region Overridden NativeActivity Methods

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);            
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            _debugOutputs.Clear();
            var indexCounter = 1;
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();            
            // 2012.11.05 : Travis.Frisinger - Added for Binary DataList -- Shape Input
            //IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);
            toUpsert.IsDebug = dataObject.IsDebug;
            DispatchDebugState(context, StateType.Before);
            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            Guid executionID = DataListExecutionID.Get(context);

            try
            {
                if (!errors.HasErrors())
                {
                    for (int i = 0; i < FieldsCollection.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(FieldsCollection[i].FieldName))
                        {
                            string eval = FieldsCollection[i].FieldValue;

                            if (eval.StartsWith("@"))
                            {
                                eval = GetEnviromentVariable(dataObject, context, eval);
                            }

                            //2013.06.03: Ashley Lewis for bug 9498 - handle line breaks in multi assign
                            string[] openParts = Regex.Split(FieldsCollection[i].FieldName, @"\[\[");
                            string[] closeParts = Regex.Split(FieldsCollection[i].FieldName, @"\]\]");
                            if(openParts.Count() == closeParts.Count() && openParts.Count() > 2 && closeParts.Count() > 2)
                            {
                                foreach (var newFieldName in openParts)
                                {
                                    if(!string.IsNullOrEmpty(newFieldName))
                                    {
                                        string cleanFieldName = null;
                                        if(newFieldName.IndexOf("]]") + 2 < newFieldName.Length)
                                        {
                                            cleanFieldName = "[[" + newFieldName.Remove(newFieldName.IndexOf("]]") + 2);
                                        }
                                        else
                                        {
                                            cleanFieldName = "[[" + newFieldName;
                                        }
                                        toUpsert.Add(cleanFieldName, eval);
                                    }
                                }
                            }
                            else
                            {
                                toUpsert.Add(FieldsCollection[i].FieldName, eval);
                            }
                        }
                        indexCounter++;
                    }
                    compiler.Upsert(executionID, toUpsert, out errors);

                    if (dataObject.IsDebug)
                    {
                        int innerCount = 0;
                        foreach (DebugOutputTO debugOutputTO in toUpsert.DebugOutputs)
                        {                            
                            AddDebugItem(debugOutputTO.TargetEntry, FieldsCollection[innerCount].FieldValue, debugOutputTO.FromEntry, executionID, innerCount, dataObject, context);
                            innerCount++;
                            if (debugOutputTO.FromEntry != null)
                                debugOutputTO.FromEntry.Dispose();
                            if (debugOutputTO.TargetEntry != null)
                                debugOutputTO.TargetEntry.Dispose();
                        }                                            
                    }
                    allErrors.MergeErrors(errors);
                }

            }
            finally
            {
                // Handle Errors
                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfWebpageActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, allErrors.MakeDataListReady(), out errors);
                }
                if (dataObject != null && dataObject.IsDebug)
                {
                    DispatchDebugState(context, StateType.After);
                }
            }

        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {

            foreach (Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                var items = FieldsCollection.Where(c => !string.IsNullOrEmpty(c.FieldValue) && c.FieldValue.Equals(t.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.FieldValue = t.Item2;
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach (Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                var items = FieldsCollection.Where(c => !string.IsNullOrEmpty(c.FieldName) && c.FieldName.Equals(t.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.FieldName = t.Item2;
                }
            }
        }

        #endregion        

        #region Overridden ActivityAbstact Methods

        public override IBinaryDataList GetWizardData()
        {
            string error = string.Empty;
            IBinaryDataList result = Dev2BinaryDataListFactory.CreateDataList();
            string recordsetName = "FieldsCollection";
            result.TryCreateRecordsetTemplate(recordsetName, string.Empty, new List<Dev2Column>() { DataListFactory.CreateDev2Column("FieldName", string.Empty), DataListFactory.CreateDev2Column("FieldValue", string.Empty) }, true, out error);
            foreach (ActivityDTO item in FieldsCollection)
            {
                result.TryCreateRecordsetValue(item.FieldName, "FieldName", recordsetName, item.IndexNumber, out error);
                result.TryCreateRecordsetValue(item.FieldValue, "FieldValue", recordsetName, item.IndexNumber, out error);
            }
            return result;
        }

        #endregion Overridden ActivityAbstact Methods

        #region Get Debug Inputs/Outputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            return DebugItem.EmptyList;
        }

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> invalidDebugItems = new List<IDebugItem>();
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
                if(string.IsNullOrEmpty(debugOutput.FetchResultsList()[1].Value))
                {
                    invalidDebugItems.Add(debugOutput);
                }
            }
            foreach(IDebugItem invalidDebugItem in invalidDebugItems)
            {
                _debugOutputs.Remove(invalidDebugItem);
            }

            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs
       
        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            var items = FieldsCollection.Where(c => !string.IsNullOrEmpty(c.FieldValue) && c.FieldValue.Contains("[[")).Select(c => c.FieldValue).ToArray();
            return GetForEachItems(context, StateType.Before, items);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            var items = FieldsCollection.Where(c => !string.IsNullOrEmpty(c.FieldName)).Select(c => c.FieldName).ToArray();
            return GetForEachItems(context, StateType.After, items);
        }

        #endregion

        #region Methods

        public void RemoveItem()
        {
            List<int> BlankCount = new List<int>();
            foreach (dynamic item in FieldsCollection)
            {
                if (String.IsNullOrEmpty(item.FieldName) && String.IsNullOrEmpty(item.FieldValue))
                {
                    BlankCount.Add(item.IndexNumber);
                }
            }
            if (BlankCount.Count > 1 && FieldsCollection.Count > 2)
            {
                FieldsCollection.Remove(FieldsCollection[BlankCount[0] - 1]);
                for (int i = BlankCount[0] - 1; i < FieldsCollection.Count; i++)
                {
                    dynamic tmp = FieldsCollection[i] as dynamic;
                    tmp.IndexNumber = i + 1;
                }
            }
        }

        #endregion

        #region Private Method

        private string GetEnviromentVariable(IDSFDataObject dataObject, NativeActivityContext context, string eval)
        {
            if (dataObject != null)
            {
                string bookmarkName = Guid.NewGuid().ToString();
                eval = eval.Replace("@Service", dataObject.ServiceName).Replace("@Instance", context.WorkflowInstanceId.ToString()).Replace("@Bookmark", bookmarkName).Replace("@AppPath", Directory.GetCurrentDirectory());
                Uri hostUri = null;
                if (Uri.TryCreate(ServiceHost, UriKind.Absolute, out hostUri))
                {
                    eval = eval.Replace("@Host", ServiceHost);
                }
                eval = DataListUtil.BindEnvironmentVariables(eval, dataObject.ServiceName);
            }
            return eval;
        }

        private void AddDebugItem(IBinaryDataListEntry fieldEntry, string fieldValue, IBinaryDataListEntry valueEntry, Guid executionId, int indexNumToUse, IDSFDataObject dataObject, NativeActivityContext context)
        {
            if (valueEntry != null)
            {
                if (fieldValue.Contains("@"))
                {
                    string eval = GetEnviromentVariable(dataObject, context, fieldValue);
                    IList<IDebugItemResult> results = CreateDebugItemsFromString(FieldsCollection[indexNumToUse].FieldName, FieldsCollection[indexNumToUse].FieldValue, DataListExecutionID.Get(context), indexNumToUse, enDev2ArgumentType.Output);
                    IDebugItem itemToAdd = new DebugItem();
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = (indexNumToUse + 1).ToString(CultureInfo.InvariantCulture) });
                    results.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression });
                    results.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = eval });
                    itemToAdd.AddRange(results);
                    _debugOutputs.Add(itemToAdd);
                }
                else
                {
                    DebugItem itemToAdd = new DebugItem();
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = (indexNumToUse + 1).ToString(CultureInfo.InvariantCulture) });
                    string fieldName = FieldsCollection[indexNumToUse].FieldName;
                    if(fieldEntry != null && fieldEntry.IsRecordset && 
                        (DataListUtil.GetRecordsetIndexType(FieldsCollection[indexNumToUse].FieldName) 
                        == enRecordsetIndexType.Blank))
                    {
                        fieldName = fieldName.Replace("().", string.Concat("(",fieldEntry.FetchAppendRecordsetIndex().ToString(CultureInfo.InvariantCulture),")."));
                    }
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = fieldName });
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression });

                    itemToAdd.AddRange(CreateDebugItemsFromEntry(fieldValue, valueEntry, executionId, enDev2ArgumentType.Output, valueEntry.FetchAppendRecordsetIndex()));

                    _debugOutputs.Add(itemToAdd);
                }
            }
        }

        private void InsertToCollection(IList<string> listToAdd, ModelItem modelItem)
        {
            ModelItemCollection mic = modelItem.Properties["FieldsCollection"].Collection;

            if (mic != null)
            {
                List<ActivityDTO> listOfValidRows = FieldsCollection.Where(c => !c.CanRemove()).ToList();
                if (listOfValidRows.Count > 0)
                {
                    int startIndex = FieldsCollection.Last(c => !c.CanRemove()).IndexNumber;
                    foreach (string s in listToAdd)
                    {
                        mic.Insert(startIndex, new ActivityDTO(s, string.Empty, startIndex + 1));
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
            ModelItemCollection mic = modelItem.Properties["FieldsCollection"].Collection;

            if (mic != null)
            {
                int startIndex = 0;
                mic.Clear();
                foreach (string s in listToAdd)
                {
                    mic.Add(new ActivityDTO(s, string.Empty, startIndex + 1));
                    startIndex++;
                }
                CleanUpCollection(mic, modelItem, startIndex);
            }
        }

        private void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if (startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new ActivityDTO(string.Empty, string.Empty, startIndex + 1));
            modelItem.Properties["DisplayName"].SetValue(CreateDisplayName(modelItem, startIndex + 1));
        }

        private string CreateDisplayName(ModelItem modelItem, int count)
        {
            string currentName = modelItem.Properties["DisplayName"].ComputedValue as string;
            if (currentName.Contains("(") && currentName.Contains(")"))
            {
                if (currentName.Contains(" ("))
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

        #endregion

        #region Implementation of ICollectionActivity

        public int GetCollectionCount()
        {
            return FieldsCollection.Count(caseConvertTO => !caseConvertTO.CanRemove());
        }

        public void AddListToCollection(IList<string> listToAdd, bool overwrite, ModelItem modelItem)
        {
            if (!overwrite)
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

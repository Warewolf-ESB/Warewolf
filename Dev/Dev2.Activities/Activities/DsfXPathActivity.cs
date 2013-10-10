using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Data.Factories;
using Dev2.Data.Parsers;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities
{
    public class DsfXPathActivity : DsfActivityAbstract<string>, ICollectionActivity
    {
        #region Fields

        IList<XPathDTO> _resultsCollection;
        string _sourceString;

        #endregion

        #region Properties

        public IList<XPathDTO> ResultsCollection
        {
            get
            {
                return _resultsCollection;
            }
            set
            {
                _resultsCollection = value;
                OnPropertyChanged("ResultsCollection");
            }
        }

        public string SourceString
        {
            get
            {
                return _sourceString;
            }
            set
            {
                _sourceString = value;
                OnPropertyChanged("SourceString");
            }
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Ctor

        public DsfXPathActivity()
            : base("XPath")
        {
            ResultsCollection = new List<XPathDTO>();
        }

        #endregion

        #region Overridden NativeActivity Methods

        protected override void OnExecute(NativeActivityContext context)
        {
            _debugOutputs.Clear();

            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IDev2DataListUpsertPayloadBuilder<List<string>> toUpsert = Dev2DataListBuilderFactory.CreateStringListDataListUpsertBuilder();
            toUpsert.IsDebug = (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke);
            toUpsert.ResourceID = dataObject.ResourceID;
            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            Guid executionID = DataListExecutionID.Get(context);
            XPathParser parser =new XPathParser();
            try
            {
                if(!errors.HasErrors())
                {
                    IBinaryDataListEntry expressionsEntry = compiler.Evaluate(executionID, enActionType.User, SourceString, false, out errors);

                    if(dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                    {
                        AddSourceStringDebugInputItem(SourceString, expressionsEntry, executionID);
                        AddResultDebugInputs(ResultsCollection,executionID,compiler);
                    }
                    
                    IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);
                    while(itr.HasMoreRecords())
                    {
                        IList<IBinaryDataListItem> cols = itr.FetchNextRowData();
                        foreach(IBinaryDataListItem c in cols)
                        {
                            for(int i = 0; i < ResultsCollection.Count; i++)
                            {


                                if(!string.IsNullOrEmpty(ResultsCollection[i].OutputVariable))
                                {

                                    IBinaryDataListEntry xpathEntry = compiler.Evaluate(executionID, enActionType.User, ResultsCollection[i].XPath, false, out errors);
                                    IDev2DataListEvaluateIterator xpathItr = Dev2ValueObjectFactory.CreateEvaluateIterator(xpathEntry);
                                    while(xpathItr.HasMoreRecords())
                                    {
                                        IList<IBinaryDataListItem> xpathCols = xpathItr.FetchNextRowData();
                                        foreach (IBinaryDataListItem xPathCol in xpathCols)
                                        {
                                            List<string> eval = parser.ExecuteXPath(c.TheValue, xPathCol.TheValue).ToList();

                                            //2013.06.03: Ashley Lewis for bug 9498 - handle line breaks in multi assign
                                            string[] openParts = Regex.Split(ResultsCollection[i].OutputVariable, @"\[\[");
                                            string[] closeParts = Regex.Split(ResultsCollection[i].OutputVariable, @"\]\]");
                                            if(openParts.Count() == closeParts.Count() && openParts.Count() > 2 && closeParts.Count() > 2)
                                            {
                                                foreach(var newFieldName in openParts)
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
                                                toUpsert.Add(ResultsCollection[i].OutputVariable, eval);
                                            }
                                        }
                                    }
                                }
                            }

                                compiler.Upsert(executionID, toUpsert, out errors);
                            }
                            if(dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                            {
                                int innerCount = 0;
                                foreach(DebugOutputTO debugOutputTO in toUpsert.DebugOutputs)
                                {
                                    var expression = ResultsCollection[innerCount].OutputVariable;
                                    enRecordsetIndexType indexType = DataListUtil.GetRecordsetIndexType(expression);
                                    if (indexType == enRecordsetIndexType.Blank)
                                    {
                                        expression = expression.Replace("().", "(*).");
                                    }
                                    IBinaryDataListEntry binaryDataListEntry = compiler.Evaluate(executionID, enActionType.User, expression, false, out errors);
                                    AddDebugOutputItemFromEntry(expression, binaryDataListEntry, innerCount + 1, executionID);
                                    innerCount++;
                                    if(debugOutputTO.FromEntry != null)
                                        debugOutputTO.FromEntry.Dispose();
                                    if(debugOutputTO.TargetEntry != null)
                                        debugOutputTO.TargetEntry.Dispose();
                                }
                                toUpsert.DebugOutputs.Clear();
                            }
                            allErrors.MergeErrors(errors);
                        }
                    }

                }
            
            catch (Exception ex)
            {
                allErrors.AddError(ex.Message);
            }
            finally
            {
                // Handle Errors
                if(allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfXPathActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }

        }

        void AddResultDebugInputs(IList<XPathDTO> resultsCollection, Guid executionID, IDataListCompiler compiler)
        {
            foreach(var xPathDto in resultsCollection)
            {
                if(!String.IsNullOrEmpty(xPathDto.OutputVariable))
                {
                    var itemToAdd = new DebugItem();
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = xPathDto.OutputVariable });
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression });
                    IBinaryDataListEntry expressionsEntry = compiler.Evaluate(executionID, enActionType.User, xPathDto.XPath, false, out errors);
                    itemToAdd.AddRange(CreateDebugItemsFromEntry(xPathDto.XPath, expressionsEntry, executionID, enDev2ArgumentType.Input));
                    _debugInputs.Add(itemToAdd);
                }
            }            
        }

        private void AddSourceStringDebugInputItem(string expression, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "XML Source" });

            itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));

            _debugInputs.Add(itemToAdd);
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.MixedActivity;
        }

        #endregion

        #region Private Methods


        void AddDebugOutputItemFromEntry(string expression, IBinaryDataListEntry value, int indexCount, Guid dlId)
        {
            var itemToAdd = new DebugItem();

            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = indexCount.ToString(CultureInfo.InvariantCulture) });
            
            itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, value, dlId, enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);
        }

        void InsertToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ResultsCollection"];
            if(modelProperty == null)
            {
                return;
            }
            var mic = modelProperty.Collection;

            if(mic == null)
            {
                return;
            }
            var listOfValidRows = ResultsCollection.Where(c => !c.CanRemove()).ToList();
            if(listOfValidRows.Count > 0)
            {
                var startIndex = ResultsCollection.Last(c => !c.CanRemove()).IndexNumber;
                foreach(var s in listToAdd)
                {
                    mic.Insert(startIndex, new XPathDTO(s, ResultsCollection[startIndex - 1].XPath, startIndex + 1));
                    startIndex++;
                }
                CleanUpCollection(mic, modelItem, startIndex);
            }
            else
            {
                AddToCollection(listToAdd, modelItem);
            }
        }

        void AddToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ResultsCollection"];
            if(modelProperty == null)
            {
                return;
            }
            var mic = modelProperty.Collection;

            if(mic == null)
            {
                return;
            }
            var startIndex = 0;
            var firstRowXPath = ResultsCollection[0].XPath;
            mic.Clear();
            foreach(var s in listToAdd)
            {
                mic.Add(new XPathDTO(s, firstRowXPath, startIndex + 1));
                startIndex++;
            }
            CleanUpCollection(mic, modelItem, startIndex);
        }

        void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if(startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new XPathDTO(string.Empty, "", startIndex + 1));
            var modelProperty = modelItem.Properties["DisplayName"];
            if(modelProperty != null)
            {
                modelProperty.SetValue(CreateDisplayName(modelItem, startIndex + 1));
            }
        }

        string CreateDisplayName(ModelItem modelItem, int count)
        {
            var modelProperty = modelItem.Properties["DisplayName"];
            if(modelProperty == null)
            {
                return "";
            }
            var currentName = modelProperty.ComputedValue as string;
            if(currentName != null && (currentName.Contains("(") && currentName.Contains(")")))
            {
                currentName = currentName.Remove(currentName.Contains(" (") ? currentName.IndexOf(" (", StringComparison.Ordinal) : currentName.IndexOf("(", StringComparison.Ordinal));
            }
            currentName = currentName + " (" + (count - 1) + ")";
            return currentName;
        }


        void CleanArguments(IList<XPathDTO> args)
        {
            var count = 0;
            while(count < args.Count)
            {
                if(string.IsNullOrEmpty(args[count].OutputVariable))
                {
                    args.RemoveAt(count);
                }
                else
                {
                    count++;
                }
            }
        }

        #endregion Private Methods

        #region Overridden ActivityAbstact Methods

        public override IBinaryDataList GetWizardData()
        {
            string error;
            var result = Dev2BinaryDataListFactory.CreateDataList();
            const string RecordsetName = "ResultsCollection";
            result.TryCreateScalarTemplate(string.Empty, "SourceString", string.Empty, true, out error);
            result.TryCreateScalarValue(SourceString, "SourceString", out error);
            result.TryCreateRecordsetTemplate(RecordsetName, string.Empty, new List<Dev2Column> { DataListFactory.CreateDev2Column("XPath", string.Empty), DataListFactory.CreateDev2Column("At", string.Empty), DataListFactory.CreateDev2Column("Include", string.Empty), DataListFactory.CreateDev2Column("Result", string.Empty) }, true, out error);
            foreach(var item in ResultsCollection)
            {
                result.TryCreateRecordsetValue(item.XPath, "XPath", RecordsetName, item.IndexNumber, out error);
                result.TryCreateRecordsetValue(item.OutputVariable, "Result", RecordsetName, item.IndexNumber, out error);
            }
            return result;
        }

        #endregion Overridden ActivityAbstact Methods

        #region Get Debug Inputs/Outputs

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        #endregion

        #region GetDebugOutputs

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion

        #endregion

        #region Get ForEach Inputs/Ouputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {
                    // locate all updates for this tuple
                    Tuple<string, string> t1 = t;
                    var items = ResultsCollection.Where(c => !string.IsNullOrEmpty(c.XPath) && c.XPath.Equals(t1.Item1));

                    // issues updates
                    foreach(var a in items)
                    {
                        a.XPath = t.Item2;
                    }

                    if(SourceString == t.Item1)
                    {
                        SourceString = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {
                    // locate all updates for this tuple
                    var t1 = t;
                    var items = ResultsCollection.Where(c => !string.IsNullOrEmpty(c.OutputVariable) && c.OutputVariable.Equals(t1.Item1));

                    // issues updates
                    foreach(var a in items)
                    {
                        a.OutputVariable = t.Item2;
                    }
                }
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var items = (new[] { SourceString }).Union(ResultsCollection.Where(c => !string.IsNullOrEmpty(c.XPath)).Select(c => c.XPath)).ToArray();
            return GetForEachItems(items);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var items = ResultsCollection.Where(c => !string.IsNullOrEmpty(c.OutputVariable)).Select(c => c.OutputVariable).ToArray();
            return GetForEachItems(items);
        }

        #endregion

        #region Implementation of ICollectionActivity

        public int GetCollectionCount()
        {
            return ResultsCollection.Count(xPathDto => !xPathDto.CanRemove());
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
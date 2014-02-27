using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Dev2.Activities.Debug;
using Dev2.Data.Factories;
using Dev2.Data.Parsers;
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
        bool _isDebugMode;

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
            _isDebugMode = dataObject.IsDebugMode();
            toUpsert.IsDebug = _isDebugMode;
            toUpsert.ResourceID = dataObject.ResourceID;
            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            Guid executionID = DataListExecutionID.Get(context);
            XPathParser parser = new XPathParser();
            int i = 0;

            InitializeDebug(dataObject);
            try
            {
                if(!errors.HasErrors())
                {
                    IBinaryDataListEntry expressionsEntry = compiler.Evaluate(executionID, enActionType.User, SourceString, false, out errors);

                    if(_isDebugMode)
                    {
                        AddSourceStringDebugInputItem(SourceString, expressionsEntry, executionID);
                        AddResultDebugInputs(ResultsCollection, executionID, compiler, out errors);
                        allErrors.MergeErrors(errors);
                    }
                    if(!allErrors.HasErrors())
                    {
                        IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);
                        while(itr.HasMoreRecords())
                        {
                            IList<IBinaryDataListItem> cols = itr.FetchNextRowData();
                            foreach(IBinaryDataListItem c in cols)
                            {
                                for(i = 0; i < ResultsCollection.Count; i++)
                                {

                                    if(!string.IsNullOrEmpty(ResultsCollection[i].OutputVariable))
                                    {
                                        IBinaryDataListEntry xpathEntry = compiler.Evaluate(executionID, enActionType.User, ResultsCollection[i].XPath, false, out errors);
                                        IDev2DataListEvaluateIterator xpathItr = Dev2ValueObjectFactory.CreateEvaluateIterator(xpathEntry);
                                        while(xpathItr.HasMoreRecords())
                                        {
                                            IList<IBinaryDataListItem> xpathCols = xpathItr.FetchNextRowData();
                                            foreach(IBinaryDataListItem xPathCol in xpathCols)
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
                                                            string cleanFieldName;
                                                            if(newFieldName.IndexOf("]]", StringComparison.Ordinal) + 2 < newFieldName.Length)
                                                            {
                                                                cleanFieldName = "[[" + newFieldName.Remove(newFieldName.IndexOf("]]", StringComparison.Ordinal) + 2);
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

                            allErrors.MergeErrors(errors);
                        }
                    }
                    if(_isDebugMode && !allErrors.HasErrors())
                    {
                        var innerCount = 1;
                        foreach(var debugOutputTO in toUpsert.DebugOutputs)
                        {
                            var itemToAdd = new DebugItem();
                            AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), itemToAdd);
                            AddDebugItem(new DebugItemVariableParams(debugOutputTO, ""), itemToAdd);
                            _debugOutputs.Add(itemToAdd);
                            innerCount++;
                        }
                        toUpsert.DebugOutputs.Clear();
                    }
                }

            }
            catch(Exception ex)
            {
                allErrors.AddError(ex.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfXPathActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if(_isDebugMode)
                {
                    if(hasErrors)
                    {
                        if(_isDebugMode)
                        {
                            ResultsCollection[i].XPath = "";
                            var itemToAdd = new DebugItem();
                            AddDebugItem(new DebugItemStaticDataParams("", (i + 1).ToString(CultureInfo.InvariantCulture)), itemToAdd);
                            AddDebugItem(new DebugOutputParams(ResultsCollection[i].OutputVariable, "", executionID, i + 1, ""), itemToAdd);
                            _debugOutputs.Add(itemToAdd);
                        }
                    }
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        void AddResultDebugInputs(IEnumerable<XPathDTO> resultsCollection, Guid executionID, IDataListCompiler compiler, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            var i = 1;
            foreach(var xPathDto in resultsCollection)
            {
                if(!String.IsNullOrEmpty(xPathDto.OutputVariable))
                {
                    var expressionsEntry = compiler.Evaluate(executionID, enActionType.User, xPathDto.XPath, false, out errorsTo);
                    errors.MergeErrors(errorsTo);
                    compiler.Evaluate(executionID, enActionType.User, xPathDto.OutputVariable, false, out errorsTo);
                    errors.MergeErrors(errorsTo);
                    if(_isDebugMode)
                    {
                        var itemToAdd = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("", i.ToString(CultureInfo.InvariantCulture)), itemToAdd);
                        AddDebugItem(new DebugItemVariableParams(xPathDto.OutputVariable, "", expressionsEntry, executionID), itemToAdd);
                        _debugInputs.Add(itemToAdd);
                        i++;
                    }
                }
            }
        }

        private void AddSourceStringDebugInputItem(string expression, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            AddDebugInputItem(new DebugItemVariableParams(expression, "XML", valueEntry, executionId));
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.MixedActivity;
        }

        #endregion

        #region Private Methods
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
                XPathDTO xPathDto = ResultsCollection.Last(c => !c.CanRemove());
                var startIndex = ResultsCollection.IndexOf(xPathDto) + 1;
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

        #endregion Private Methods

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
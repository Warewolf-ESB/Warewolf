/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Text.RegularExpressions;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data;
using Dev2.Data.Parsers;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Storage;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("Utility-Path", "XPath", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_Xpath_Tags")]
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

        protected override bool CanInduceIdle => true;

        #endregion

        #region Ctor

        public DsfXPathActivity()
            : base("XPath")
        {
            ResultsCollection = new List<XPathDTO>();
        }

        #endregion

        public override List<string> GetOutputs()
        {
            return ResultsCollection.Select(dto => dto.OutputVariable).ToList();
        }

        #region Overridden NativeActivity Methods

        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _debugOutputs.Clear();

            _isDebugMode = dataObject.IsDebugMode();
            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            XPathParser parser = new XPathParser();
            int i = 0;

            InitializeDebug(dataObject);
            try
            {
                if(!errors.HasErrors())
                {
                    if(_isDebugMode)
                    {
                        AddSourceStringDebugInputItem(SourceString, dataObject.Environment, update);
                        AddResultDebugInputs(ResultsCollection, out errors);
                        allErrors.MergeErrors(errors);
                    }
                    if(!allErrors.HasErrors())
                    {
                        var itr = new WarewolfListIterator();
                        var sourceIterator = new WarewolfIterator(dataObject.Environment.Eval(SourceString, update));
                        itr.AddVariableToIterateOn(sourceIterator);
                        while(itr.HasMoreData())
                        {
                            var c = itr.FetchNextValue(sourceIterator);
                            //foreach(IBinaryDataListItem c in cols)
                            {
                                for(i = 0; i < ResultsCollection.Count; i++)
                                {
                                    if(!string.IsNullOrEmpty(ResultsCollection[i].OutputVariable))
                                    {
                                        var xpathEntry = dataObject.Environment.Eval(ResultsCollection[i].XPath, update);
                                        var xpathIterator = new WarewolfIterator(xpathEntry);
                                        while(xpathIterator.HasMoreData())
                                        {
                                            var xpathCol = xpathIterator.GetNextValue();
                                            //foreach(IBinaryDataListItem xPathCol in xpathCols)
                                            {
                                                try
                                                {
                                                    List<string> eval = parser.ExecuteXPath(c, xpathCol).ToList();

                                                    //2013.06.03: Ashley Lewis for bug 9498 - handle line breaks in multi assign
                                                    string[] openParts = Regex.Split(ResultsCollection[i].OutputVariable, @"\[\[");
                                                    string[] closeParts = Regex.Split(ResultsCollection[i].OutputVariable, @"\]\]");
                                                    if(openParts.Length == closeParts.Length && openParts.Length > 2 && closeParts.Length > 2)
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
                                                                AssignResult(cleanFieldName, dataObject, eval, update);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var variable = ResultsCollection[i].OutputVariable;
                                                        AssignResult(variable, dataObject, eval, update);
                                                    }
                                                }
                                                catch(Exception e)
                                                {
                                                    allErrors.AddError(e.Message);
                                                    dataObject.Environment.Assign(ResultsCollection[i].OutputVariable, null, update);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            allErrors.MergeErrors(errors);
                        }
                    }
                    if(_isDebugMode && !allErrors.HasErrors())
                    {
                        var innerCount = 1;
                        foreach(var debugOutputTo in ResultsCollection)
                        {
                            if (!string.IsNullOrEmpty(debugOutputTo.OutputVariable))
                            {
                                var itemToAdd = new DebugItem();
                                AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), itemToAdd);
                                AddDebugItem(new DebugEvalResult(DataListUtil.ReplaceRecordsetBlankWithStar(debugOutputTo.OutputVariable), "", dataObject.Environment, update), itemToAdd);
                                _debugOutputs.Add(itemToAdd);
                                innerCount++;
                            }
                        }
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

                var actualIndex = i - 1;
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfXPathActivity", allErrors);
                    var errorString = allErrors.MakeDataListReady();
                    dataObject.Environment.AddError(errorString);
                    if (actualIndex > -1)
                    {
                        dataObject.Environment.Assign(ResultsCollection[actualIndex].OutputVariable, null, update);
                    }
                }
                if(_isDebugMode)
                {
                    if(hasErrors)
                    {
                        if(_isDebugMode)
                        {
                            var itemToAdd = new DebugItem();
                            if (actualIndex < 0)
                            {
                                actualIndex = 0;
                            }
                            AddDebugItem(new DebugItemStaticDataParams("", (actualIndex + 1).ToString(CultureInfo.InvariantCulture)), itemToAdd);

                            AddDebugItem(new DebugEvalResult(ResultsCollection[actualIndex].OutputVariable, "", dataObject.Environment, update), itemToAdd);
                            _debugOutputs.Add(itemToAdd);
                        }
                    }
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        void AssignResult(string variable, IDSFDataObject dataObject, IEnumerable<string> eval, int update)
        {
            var index = 1;
            if(DataListUtil.IsValueScalar(variable))
            {
                var values = eval as IList<string> ?? eval.ToList();
                dataObject.Environment.Assign(variable, eval != null ? values.LastOrDefault() : "", update);
            }
            else
            {
                
                foreach(var val in eval)
                {
                    var correctedVariable = variable;
                    if(DataListUtil.IsValueRecordset(variable) && DataListUtil.IsStarIndex(variable))
                    {
                        correctedVariable = DataListUtil.ReplaceStarWithFixedIndex(variable, index);
                    }
                    dataObject.Environment.Assign(correctedVariable, val, update);
                    index++;
                }
            }
        }

        void AddResultDebugInputs(IEnumerable<XPathDTO> resultsCollection, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            var i = 1;
            foreach(var xPathDto in resultsCollection)
            {
                if(!String.IsNullOrEmpty(xPathDto.OutputVariable))
                {
                    if(_isDebugMode)
                    {
                        var itemToAdd = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("", i.ToString(CultureInfo.InvariantCulture)), itemToAdd);
                        AddDebugItem(new DebugItemWarewolfAtomResult(xPathDto.XPath,xPathDto.OutputVariable, ""), itemToAdd);
                        _debugInputs.Add(itemToAdd);
                        i++;
                    }
                }
            }
        }

        private void AddSourceStringDebugInputItem(string expression, IExecutionEnvironment environment, int update)
        {
            AddDebugInputItem(new DebugEvalResult(expression, "XML", environment, update));
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
            if(currentName != null && currentName.Contains("(") && currentName.Contains(")"))
            {
                currentName = currentName.Remove(currentName.Contains(" (") ? currentName.IndexOf(" (", StringComparison.Ordinal) : currentName.IndexOf("(", StringComparison.Ordinal));
            }
            currentName = currentName + " (" + (count - 1) + ")";
            return currentName;
        }

        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList, int update)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        #endregion

        #region GetDebugOutputs

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
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

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
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

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
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
            var items = new[] { SourceString }.Union(ResultsCollection.Where(c => !string.IsNullOrEmpty(c.XPath)).Select(c => c.XPath)).ToArray();
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

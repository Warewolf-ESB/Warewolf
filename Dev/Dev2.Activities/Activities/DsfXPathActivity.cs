#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Data;
using Dev2.Data.Parsers;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("Utility-Path", "XPath", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_Xpath")]
    public class DsfXPathActivity : DsfActivityAbstract<string>, ICollectionActivity,IEquatable<DsfXPathActivity>
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


        public override IEnumerable<StateVariable> GetState()
        {
            return new[] {
                new StateVariable
                {
                    Name = "SourceString",
                    Value = SourceString,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name="ResultsCollection",
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(ResultsCollection),
                    Type = StateVariable.StateType.Output
                }
            };
        }


        #endregion

        public override List<string> GetOutputs() => ResultsCollection?.Select(dto => dto.OutputVariable).ToList() ?? new List<string>();

        #region Overridden NativeActivity Methods

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();

            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _debugOutputs.Clear();

            _isDebugMode = dataObject.IsDebugMode();
            var errors = new ErrorResultTO();
            var allErrors = new ErrorResultTO();
            var parser = new XPathParser();
            var i = 0;

            InitializeDebug(dataObject);
            try
            {
                if (!string.IsNullOrEmpty(SourceString))
                {
                    if (_isDebugMode)
                    {
                        AddSourceStringDebugInputItem(SourceString, dataObject.Environment, update);
                        AddResultDebugInputs(ResultsCollection, out errors);
                        allErrors.MergeErrors(errors);
                    }
                    if (!allErrors.HasErrors())
                    {
                        i = Process(dataObject, update, i, parser, allErrors, errors);
                    }
                    DoDebug(dataObject, update, allErrors);
                }
            }
            catch(Exception ex)
            {
                allErrors.AddError(ex.Message);
            }
            finally
            {
                var actualIndex = i - 1;
                var hasErrors = allErrors.HasErrors();
                ProcessErrors(dataObject, update, hasErrors, allErrors, actualIndex);

                DispatchDebugState(dataObject, StateType.Before, update);
                DispatchDebugState(dataObject, StateType.After, update);
            }
        }

        void ProcessErrors(IDSFDataObject dataObject, int update, bool hasErrors, ErrorResultTO allErrors, int actualIndex)
        {
            if (hasErrors)
            {
                DisplayAndWriteError("DsfXPathActivity", allErrors);
                var errorString = allErrors.MakeDataListReady();
                dataObject.Environment.AddError(errorString);
                if (actualIndex > -1)
                {
                    dataObject.Environment.Assign(ResultsCollection[actualIndex].OutputVariable, null, update);
                }
                if (_isDebugMode)
                {
                    var itemToAdd = new DebugItem();
                    if (actualIndex < 0)
                    {
                        actualIndex = 0;
                    }
                    AddDebugItem(new DebugItemStaticDataParams("", (actualIndex + 1).ToString(CultureInfo.InvariantCulture)), itemToAdd);

                    var outputVariable = "";
                    if (actualIndex >= 0)
                    {
                        outputVariable = ResultsCollection[actualIndex].OutputVariable;
                    }
                    AddDebugItem(new DebugEvalResult(outputVariable, "", dataObject.Environment, update), itemToAdd);
                    _debugOutputs.Add(itemToAdd);

                }
            }

        }

        void DoDebug(IDSFDataObject dataObject, int update, ErrorResultTO allErrors)
        {
            if (_isDebugMode && !allErrors.HasErrors())
            {
                var innerCount = 1;
                foreach (var debugOutputTo in ResultsCollection)
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

        int Process(IDSFDataObject dataObject, int update, int i, XPathParser parser, ErrorResultTO allErrors, ErrorResultTO errors)
        {
            if (!string.IsNullOrEmpty(SourceString))
            {
                var itr = new WarewolfListIterator();
                var sourceIterator = new WarewolfIterator(dataObject.Environment.Eval(SourceString, update));
                itr.AddVariableToIterateOn(sourceIterator);
                while (itr.HasMoreData())
                {
                    var c = itr.FetchNextValue(sourceIterator);
                    i = ProcessResultsCollection(dataObject, update, parser, allErrors, c);
                    allErrors.MergeErrors(errors);
                }
            }
            return i;
        }

        int ProcessResultsCollection(IDSFDataObject dataObject, int update, XPathParser parser, ErrorResultTO allErrors, string c)
        {
            int i;
            for (i = 0; i < ResultsCollection.Count; i++)
            {
                if (!string.IsNullOrEmpty(ResultsCollection[i].OutputVariable))
                {
                    IterateOverXPath(dataObject, update, parser, allErrors, c, i);
                }
            }

            return i;
        }

        void IterateOverXPath(IDSFDataObject dataObject, int update, XPathParser parser, ErrorResultTO allErrors, string c, int i)
        {
            var xpathEntry = dataObject.Environment.Eval(ResultsCollection[i].XPath, update);
            var xpathIterator = new WarewolfIterator(xpathEntry);
            while (xpathIterator.HasMoreData())
            {
                var xpathCol = xpathIterator.GetNextValue();
                try
                {
                    var eval = parser.ExecuteXPath(c, xpathCol).ToList();
                    var variable = ResultsCollection[i].OutputVariable;
                    AssignResult(variable, dataObject, eval, update);
                }
                catch (Exception e)
                {
                    allErrors.AddError(e.Message);
                    dataObject.Environment.Assign(ResultsCollection[i].OutputVariable, null, update);
                }
            }
        }

        void AssignResult(string variable, IDSFDataObject dataObject, IEnumerable<string> eval, int update)
        {
            var index = 1;
            if(DataListUtil.IsValueScalar(variable))
            {
                var values = eval as IList<string> ?? eval.ToList();
                dataObject.Environment.Assign(variable, values.LastOrDefault(), update);
            }
            else
            {
                
                foreach(var val in eval)
                {
                    var correctedVariable = variable;
                    if(DataListUtil.IsValueRecordset(variable) && DataListUtil.IsStarIndex(variable) && update==0)
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
                if (!String.IsNullOrEmpty(xPathDto.OutputVariable) && _isDebugMode)
                {
                    var itemToAdd = new DebugItem();
                    AddDebugItem(new DebugItemStaticDataParams("", i.ToString(CultureInfo.InvariantCulture)), itemToAdd);
                    AddDebugItem(new DebugItemWarewolfAtomResult(xPathDto.XPath, xPathDto.OutputVariable, ""), itemToAdd);
                    _debugInputs.Add(itemToAdd);
                    i++;
                }

            }
        }

        void AddSourceStringDebugInputItem(string expression, IExecutionEnvironment environment, int update)
        {
            AddDebugInputItem(new DebugEvalResult(expression, "XML", environment, update));
        }

        public override enFindMissingType GetFindMissingType() => enFindMissingType.MixedActivity;

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
                var xPathDto = ResultsCollection.Last(c => !c.CanRemove());
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
            if (currentName != null && currentName.Contains("(") && currentName.Contains(")"))
            {
                currentName = currentName.Remove(currentName.Contains(" (") ? currentName.IndexOf(" (", StringComparison.Ordinal) : currentName.IndexOf("(", StringComparison.Ordinal));
            }
            currentName = currentName + " (" + (count - 1) + ")";
            return currentName;
        }

        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        #endregion

        #region GetDebugOutputs

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
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
                    var t1 = t;
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

        public int GetCollectionCount() => ResultsCollection.Count(xPathDto => !xPathDto.CanRemove());

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

        public bool Equals(DsfXPathActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other)
                && ResultsCollection.SequenceEqual(other.ResultsCollection)
                && string.Equals(SourceString, other.SourceString) ;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
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

            return Equals((DsfXPathActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (ResultsCollection != null ? ResultsCollection.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SourceString != null ? SourceString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _isDebugMode.GetHashCode();
                return hashCode;
            }
        }
    }
}

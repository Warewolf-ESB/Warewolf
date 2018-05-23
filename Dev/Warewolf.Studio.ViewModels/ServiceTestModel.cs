﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Util;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ServiceTestModel : BindableBase, IServiceTestModel
    {
        string _testName;
        string _userName;
        bool _testPassed;
        bool _testFailing;
        bool _testPending;
        bool _testInvalid;
        string _password;
        ObservableCollection<IServiceTestInput> _inputs;
        ObservableCollection<IServiceTestOutput> _outputs;
        bool _noErrorExpected;
        bool _errorExpected;
        bool _isNewTest;
        bool _isTestSelected;
        DateTime _lastRunDate;
        bool _enabled;
        string _runSelectedTestUrl;
        AuthenticationType _authenticationType;
        Guid _parentId;
        string _oldTestName;
        bool _newTest;
        string _nameForDisplay;
        ServiceTestModel _item;
        bool _isTestRunning;
        string _neverRunString;
        string _duplicateTestTooltip;
        bool _lastRunDateVisibility;
        bool _neverRunStringVisibility;
        IList<IDebugState> _debugForTest;
        ObservableCollection<IServiceTestStep> _testSteps;
        string _errorContainsText;
        bool _isTestLoading;
        bool _isValidatingIsDirty;
#pragma warning disable S1450 // Private fields only used as local variables in methods should become local variables
        bool _isDirty;
#pragma warning restore S1450 // Private fields only used as local variables in methods should become local variables

        public string NeverRunString
        {
            get { return _neverRunString; }
            set
            {
                _neverRunString = value;
                OnPropertyChanged(NeverRunString);
            }
        }

        public bool LastRunDateVisibility
        {
            get { return _lastRunDateVisibility; }
            set
            {
                if (value)
                {
                    NeverRunStringVisibility = false;
                }
                _lastRunDateVisibility = value;
                OnPropertyChanged(() => LastRunDateVisibility);
            }
        }

        public bool NeverRunStringVisibility
        {
            get { return _neverRunStringVisibility; }
            set
            {
                if (value)
                {
                    LastRunDateVisibility = false;
                }
                _neverRunStringVisibility = value;
                OnPropertyChanged(() => NeverRunStringVisibility);
            }
        }
        public IList<IDebugState> DebugForTest
        {
            get { return _debugForTest; }
            set
            {
                _debugForTest = value;
                OnPropertyChanged(() => DebugForTest);
            }
        }

        public string DuplicateTestTooltip
        {
            get { return _duplicateTestTooltip; }
            set
            {
                _duplicateTestTooltip = value;
                OnPropertyChanged(() => DuplicateTestTooltip);
            }
        }

        public ServiceTestModel()
        {
            NeverRunString = "Never run";
            NeverRunStringVisibility = true;
            IsTestRunning = false;
            TestSteps = new ObservableCollection<IServiceTestStep>();
        }
        public ServiceTestModel(Guid resourceId)
        {
            ParentId = resourceId;
            NeverRunString = "Never run";
            NeverRunStringVisibility = true;
            IsTestRunning = false;
            TestSteps = new ObservableCollection<IServiceTestStep>();
        }

        public ServiceTestModel Item
        {
            private get { return _item; }
            set
            {
                _item = value;
                OnPropertyChanged(() => Item);
                var dirty = IsDirty;
                SetDisplayName(dirty);
                OnPropertyChanged(() => IsDirty);
            }
        }

        public Guid ParentId
        {
            get { return _parentId; }
            set
            {
                _parentId = value;
                OnPropertyChanged(() => ParentId);
            }
        }

        public string OldTestName
        {
            get { return _oldTestName; }
            set
            {
                _oldTestName = value;
                OnPropertyChanged(() => OldTestName);
            }
        }

        public string TestName
        {
            get { return _testName; }
            set
            {
                _testName = value;
                OnPropertyChanged(() => TestName);
                OnPropertyChanged(() => IsDirty);
            }
        }

        public string NameForDisplay
        {
            get { return _nameForDisplay; }
            set
            {
                _nameForDisplay = value;
                OnPropertyChanged(() => NameForDisplay);
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged(() => UserName);
                OnPropertyChanged(() => IsDirty);
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(() => Password);
                OnPropertyChanged(() => IsDirty);
            }
        }
        public DateTime LastRunDate
        {
            get { return _lastRunDate; }
            set
            {
                _lastRunDate = value;
                if (_lastRunDate != default(DateTime))
                {
                    LastRunDateVisibility = true;
                }
                OnPropertyChanged(() => LastRunDate);
            }
        }

        public ObservableCollection<IServiceTestInput> Inputs
        {
            get { return _inputs; }
            set
            {
                _inputs = value;
                OnPropertyChanged(() => Inputs);
                OnPropertyChanged(() => IsDirty);
            }
        }

        public ObservableCollection<IServiceTestOutput> Outputs
        {
            get { return _outputs; }
            set
            {
                _outputs = value;
                OnPropertyChanged(() => Outputs);
                OnPropertyChanged(() => IsDirty);
            }
        }
        public bool NoErrorExpected
        {
            get { return _noErrorExpected; }
            set
            {
                if (value != _noErrorExpected)
                {
                    _noErrorExpected = value;
                    ErrorExpected = !_noErrorExpected;
                    OnPropertyChanged(() => NoErrorExpected);
                    OnPropertyChanged(() => IsDirty);
                }
            }
        }
        public bool ErrorExpected
        {
            get { return _errorExpected; }
            set
            {
                if (value != _errorExpected)
                {
                    _errorExpected = value;
                    NoErrorExpected = !_errorExpected;
                    OnPropertyChanged(() => ErrorExpected);
                    OnPropertyChanged(() => IsDirty);
                }
            }
        }

        public string ErrorContainsText
        {
            get { return _errorContainsText; }
            set
            {
                _errorContainsText = value;
                OnPropertyChanged(() => ErrorContainsText);
                OnPropertyChanged(() => IsDirty);
            }
        }

        public bool IsNewTest
        {
            get { return _isNewTest; }
            set
            {
                _isNewTest = value;
                OnPropertyChanged(() => IsNewTest);
            }
        }
        public bool IsTestSelected
        {
            get { return _isTestSelected; }
            set
            {
                _isTestSelected = value;
                OnPropertyChanged(() => IsTestSelected);
            }
        }

        public bool IsTestLoading
        {
            get { return _isTestLoading; }
            set
            {
                _isTestLoading = value;
                OnPropertyChanged(() => IsTestLoading);
            }
        }

        public bool TestPassed
        {
            get { return _testPassed; }
            set
            {
                _testPassed = value;
                if (_testPassed)
                {
                    TestPending = false;
                    TestFailing = false;
                    TestInvalid = false;
                }
                OnPropertyChanged(() => TestPassed);
            }
        }

        public bool TestFailing
        {
            get { return _testFailing; }
            set
            {
                _testFailing = value;
                if (_testFailing)
                {
                    TestPending = false;
                    TestInvalid = false;
                    TestPassed = false;
                }
                OnPropertyChanged(() => TestFailing);
            }
        }

        public bool TestInvalid
        {
            get { return _testInvalid; }
            set
            {
                _testInvalid = value;
                if (_testInvalid)
                {
                    TestPending = false;
                    TestFailing = false;
                    TestPassed = false;
                }
                OnPropertyChanged(() => TestInvalid);
            }
        }

        public bool TestPending
        {
            get { return _testPending; }
            set
            {
                _testPending = value;
                if (_testPending)
                {
                    TestFailing = false;
                    TestInvalid = false;
                    TestPassed = false;
                }
                OnPropertyChanged(() => TestPending);
            }
        }
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                OnPropertyChanged(() => Enabled);
                OnPropertyChanged(() => IsDirty);
            }
        }
        public string RunSelectedTestUrl
        {
            get { return _runSelectedTestUrl; }
            set
            {
                _runSelectedTestUrl = value;
                OnPropertyChanged(() => RunSelectedTestUrl);
            }
        }
        public AuthenticationType AuthenticationType
        {
            get { return _authenticationType; }
            set
            {
                _authenticationType = value;
                OnPropertyChanged(() => AuthenticationType);
                OnPropertyChanged(() => UserAuthenticationSelected);
                OnPropertyChanged(() => IsDirty);
            }
        }
        public bool IsDirty
        {
            get
            {
                if (_isValidatingIsDirty)
                {
                    return false;
                }
                _isValidatingIsDirty = true;
                _isDirty = false;
                var notEquals = !Equals(Item);
                if (NewTest)
                {
                    _isDirty = true;
                }
                else
                {
                    if (notEquals)
                    {
                        _isDirty = true;
                    }
                }

                SetDisplayName(_isDirty);
                _isValidatingIsDirty = false;
                return _isDirty;
            }
        }

        void SetDisplayName(bool isDirty)
        {
            NameForDisplay = isDirty ? TestName + " *" : TestName;
        }

        public bool UserAuthenticationSelected => AuthenticationType == AuthenticationType.User;

        public bool NewTest
        {
            get { return _newTest; }
            set
            {
                _newTest = value;
                OnPropertyChanged(() => NewTest);
            }
        }

        public bool IsTestRunning
        {
            get { return _isTestRunning; }
            set
            {
                _isTestRunning = value;
                OnPropertyChanged(() => IsTestRunning);
            }
        }

        public ObservableCollection<IServiceTestStep> TestSteps
        {
            get { return _testSteps ?? (_testSteps = new ObservableCollection<IServiceTestStep>()); }
            set
            {
                _testSteps = value;
                OnPropertyChanged(() => TestSteps);
            }
        }

        public void SetItem(IServiceTestModel model)
        {
            Item = model as ServiceTestModel;
        }

        public IServiceTestStep AddDebugItemTestStep(IDebugState debugItemContent, ObservableCollection<IServiceTestOutput> serviceTestOutputs) => AddTestStep(debugItemContent.ID.ToString(), debugItemContent.DisplayName, debugItemContent.ActualType, serviceTestOutputs, StepType.Assert);
        public IServiceTestStep AddTestStep(string activityUniqueId, string activityDisplayName, string activityTypeName, ObservableCollection<IServiceTestOutput> serviceTestOutputs) => AddTestStep(activityUniqueId, activityDisplayName, activityTypeName, serviceTestOutputs, StepType.Assert);
        public IServiceTestStep AddTestStep(string activityUniqueId, string activityDisplayName, string activityTypeName, ObservableCollection<IServiceTestOutput> serviceTestOutputs, StepType stepType)
        {
            if (string.IsNullOrEmpty(activityUniqueId))
            {
                throw new ArgumentNullException(nameof(activityUniqueId));
            }

            if (string.IsNullOrEmpty(activityTypeName))
            {
                throw new ArgumentNullException(nameof(activityTypeName));
            }

            if (serviceTestOutputs == null)
            {
                throw new ArgumentNullException(nameof(serviceTestOutputs));
            }

            var testStep = new ServiceTestStep(Guid.Parse(activityUniqueId), activityTypeName, serviceTestOutputs, stepType) { StepDescription = activityDisplayName };
            TestSteps.Add(testStep);
            return testStep;
        }

        public void AddRow(IServiceTestInput itemToAdd, IDataListModel dataList)
        {
            if (itemToAdd != null && DataListUtil.IsValueRecordset(itemToAdd.Variable))
            {
                var recordsetNameFromValue = DataListUtil.ExtractRecordsetNameFromValue(itemToAdd.Variable);
                var recordset = dataList.ShapeRecordSets.FirstOrDefault(set => set.Name == recordsetNameFromValue);
                if (recordset == null)
                {
                    return;
                }
                var recsetCols = new List<IScalar>();
                foreach (var column in recordset.Columns)
                {
                    var cols = column.Value.Where(scalar => scalar.IODirection == enDev2ColumnArgumentDirection.Input || scalar.IODirection == enDev2ColumnArgumentDirection.Both);
                    recsetCols.AddRange(cols);
                }

                var numberOfRows = Inputs.Where(c => DataListUtil.ExtractRecordsetNameFromValue(c.Variable) == recordsetNameFromValue);
                IEnumerable<IServiceTestInput> dataListItems = numberOfRows as IServiceTestInput[] ?? numberOfRows.ToArray();
                var lastItem = dataListItems.Last();
                var indexToInsertAt = Inputs.IndexOf(lastItem);
                var indexString = DataListUtil.ExtractIndexRegionFromRecordset(lastItem.Variable);
                var indexNum = Convert.ToInt32(indexString) + 1;
                var lastRow = dataListItems.Where(c => DataListUtil.ExtractIndexRegionFromRecordset(c.Variable) == indexString);
                var addRow = false;
                foreach (var item in lastRow)
                {
                    if (item.Value != string.Empty)
                    {
                        addRow = true;
                    }
                }
                if (addRow)
                {
                    AddBlankRowToRecordset(itemToAdd, recsetCols, indexToInsertAt, indexNum, dataList);
                }

            }
        }

        void AddBlankRowToRecordset(IServiceTestInput dlItem, IList<IScalar> columns, int indexToInsertAt, int indexNum, IDataListModel dataList)
        {
            IList<IScalar> recsetCols = columns.Distinct(Scalar.Comparer).ToList();
            string colName = null;
            foreach (var col in recsetCols.Distinct(new ScalarNameComparer()))
            {
                if (string.IsNullOrEmpty(colName) || !colName.Equals(col.Name))
                {
                    var recSetName = DataListUtil.ExtractRecordsetNameFromValue(dlItem.Variable);
                    var varName = string.Concat(recSetName, @"(", indexNum, @").", col.Name);
                    var serviceTestInput = new ServiceTestInput(varName, string.Empty);
                    serviceTestInput.AddNewAction = () => AddRow(serviceTestInput, dataList);
                    Inputs.Insert(indexToInsertAt + 1, serviceTestInput);
                    indexToInsertAt++;
                }
                colName = col.Name;
            }
        }

        public void AddRow(IServiceTestOutput itemToAdd, IDataListModel dataList)
        {
            if (itemToAdd != null && DataListUtil.IsValueRecordset(itemToAdd.Variable))
            {
                var recordsetNameFromValue = DataListUtil.ExtractRecordsetNameFromValue(itemToAdd.Variable);
                var recordset = dataList.ShapeRecordSets.FirstOrDefault(set => set.Name == recordsetNameFromValue);
                if (recordset == null)
                {
                    return;
                }
                var recsetCols = new List<IScalar>();
                foreach (var column in recordset.Columns)
                {
                    var cols = column.Value.Where(scalar => scalar.IODirection == enDev2ColumnArgumentDirection.Output || scalar.IODirection == enDev2ColumnArgumentDirection.Both);
                    recsetCols.AddRange(cols);
                }

                var numberOfRows = Outputs.Where(c => DataListUtil.ExtractRecordsetNameFromValue(c.Variable) == recordsetNameFromValue);
                IEnumerable<IServiceTestOutput> dataListItems = numberOfRows as IServiceTestOutput[] ?? numberOfRows.ToArray();
                var lastItem = dataListItems.Last();
                var indexToInsertAt = Outputs.IndexOf(lastItem);
                var indexString = DataListUtil.ExtractIndexRegionFromRecordset(lastItem.Variable);
                var indexNum = Convert.ToInt32(indexString) + 1;
                var lastRow = dataListItems.Where(c => DataListUtil.ExtractIndexRegionFromRecordset(c.Variable) == indexString);
                var addRow = false;
                foreach (var item in lastRow)
                {
                    if (item.Value != string.Empty)
                    {
                        addRow = true;
                    }
                }
                if (addRow)
                {
                    AddBlankRowToRecordset(itemToAdd, recsetCols, indexToInsertAt, indexNum, dataList);
                }

            }
        }

        void AddBlankRowToRecordset(IServiceTestOutput dlItem, IList<IScalar> columns, int indexToInsertAt, int indexNum, IDataListModel dataList)
        {
            IList<IScalar> recsetCols = columns.Distinct(Scalar.Comparer).ToList();
            string colName = null;
            foreach (var col in recsetCols.Distinct(new ScalarNameComparer()))
            {
                if (string.IsNullOrEmpty(colName) || !colName.Equals(col.Name))
                {
                    var recSetName = DataListUtil.ExtractRecordsetNameFromValue(dlItem.Variable);
                    var varName = string.Concat(recSetName, @"(", indexNum, @").", col.Name);
                    var serviceTestOutput = new ServiceTestOutput(varName, string.Empty, string.Empty, string.Empty);
                    serviceTestOutput.AddNewAction = () => AddRow(serviceTestOutput, dataList);
                    Outputs.Insert(indexToInsertAt + 1, serviceTestOutput);
                    indexToInsertAt++;
                }
                colName = col.Name;
            }
        }

        bool Equals(ServiceTestModel other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var equalsSeq = EqualsSeq(other);
            var inputCompare = InputCompare(other);
            var outputCompare = OutputCompare(other);
            var testStepCompare = TestStepCompare(other);
            var @equals = equalsSeq && inputCompare && testStepCompare && outputCompare;

            return @equals;
        }

        bool TestStepCompare(ServiceTestModel other)
        {
            if (_testSteps == null)
            {
                return true;
            }
            if (_testSteps.Count != other._testSteps.Count)
            {
                return false;
            }
            var stepCompare = true;
            for (int i = 0; i < _testSteps.Count; i++)
            {
                stepCompare &= TestSteps[i].Type == other.TestSteps[i].Type;
                stepCompare &= TestSteps[i].StepOutputs.Count == other.TestSteps[i].StepOutputs.Count;
                stepCompare &= TestSteps[i].Children.Count == other.TestSteps[i].Children.Count;
                if (TestSteps[i].Children.Count > 0)
                {
                    var stepChildren = TestSteps[i].Children;
                    var otherStepChildren = other.TestSteps[i].Children;

                    stepCompare = StepChildrenCompare(stepChildren, otherStepChildren);
                }
                if (!stepCompare)
                {
                    return stepCompare;
                }

                var stepOutputs = TestSteps[i].StepOutputs;
                var otherStepOutputs = other.TestSteps[i].StepOutputs;
                stepCompare = StepOutputsCompare(stepOutputs, otherStepOutputs);
                if (!stepCompare)
                {
                    return stepCompare;
                }
            }
            return stepCompare;
        }

        static bool StepChildrenCompare(ObservableCollection<IServiceTestStep> stepChildren, ObservableCollection<IServiceTestStep> otherStepChildren)
        {
            var stepCompare = true;
            for (int c = 0; c < stepChildren.Count; c++)
            {
                stepCompare &= stepChildren[c].Type == otherStepChildren[c].Type;
                if (!stepCompare)
                {
                    return stepCompare;
                }

                var childStepOutputs = stepChildren[c].StepOutputs;
                var otherChildStepOutputs = otherStepChildren[c].StepOutputs;
                stepCompare = StepOutputsCompare(childStepOutputs, otherChildStepOutputs);
                if (!stepCompare)
                {
                    return stepCompare;
                }

                if (stepChildren[c].Children.Count > 0)
                {
                    var stepChildren1 = stepChildren[c].Children;
                    var otherStepChildren1 = otherStepChildren[c].Children;

                    stepCompare = StepChildrenCompare(stepChildren1, otherStepChildren1);
                    if (!stepCompare)
                    {
                        return stepCompare;
                    }
                }
            }
            return stepCompare;
        }

        static bool StepOutputsCompare(ObservableCollection<IServiceTestOutput> stepOutputs, ObservableCollection<IServiceTestOutput> otherStepOutputs)
        {
            var stepCompare = true;
            for (int c = 0; c < stepOutputs.Count; c++)
            {
                stepCompare &= stepOutputs[c].AssertOp == otherStepOutputs[c].AssertOp;
                stepCompare &= stepOutputs[c].Value == otherStepOutputs[c].Value;
                stepCompare &= stepOutputs[c].From == otherStepOutputs[c].From;
                stepCompare &= stepOutputs[c].To == otherStepOutputs[c].To;
                stepCompare &= stepOutputs[c].Variable == otherStepOutputs[c].Variable;
                if (!stepCompare)
                {
                    return stepCompare;
                }
            }
            return stepCompare;
        }

        bool InputCompare(ServiceTestModel other)
        {
            if (_inputs == null)
            {
                return true;
            }
            if (_inputs.Count != other._inputs.Count)
            {
                return false;
            }
            var inputCompare = true;
            for (int i = 0; i < _inputs.Count; i++)
            {
                inputCompare &= Inputs[i].Value == other.Inputs[i].Value;
                inputCompare &= Inputs[i].Variable == other.Inputs[i].Variable;
                inputCompare &= Inputs[i].EmptyIsNull == other.Inputs[i].EmptyIsNull;
                if (!inputCompare)
                {
                    return inputCompare;
                }
            }
            return inputCompare;
        }

        bool OutputCompare(ServiceTestModel other)
        {
            if (_outputs == null)
            {
                return true;
            }
            if (_outputs.Count != other._outputs.Count)
            {
                return false;
            }
            var outputCompare = true;
            for (int i = 0; i < _outputs.Count; i++)
            {
                outputCompare &= _outputs[i].Value == other._outputs[i].Value;
                outputCompare &= _outputs[i].Variable == other._outputs[i].Variable;
                outputCompare &= _outputs[i].AssertOp == other._outputs[i].AssertOp;
                outputCompare &= _outputs[i].From == other._outputs[i].From;
                outputCompare &= _outputs[i].To == other._outputs[i].To;
                if (!outputCompare)
                {
                    return outputCompare;
                }
            }
            return outputCompare;
        }

        bool EqualsSeq(ServiceTestModel other)
        {
            var equalsSeq = string.Equals(_testName, other._testName);
            equalsSeq &= string.Equals(_userName, other._userName);
            equalsSeq &= string.Equals(_password, other._password);
            equalsSeq &= _noErrorExpected == other._noErrorExpected;
            equalsSeq &= _errorExpected == other._errorExpected;
            equalsSeq &= _errorContainsText == other._errorContainsText;
            equalsSeq &= _enabled == other._enabled;
            equalsSeq &= _authenticationType == other._authenticationType;

            return equalsSeq;
        }

        public IServiceTestModel Clone()
        {
            var serializer = new Dev2JsonSerializer();
            IServiceTestModel serviceTestModel = this;
            var ser = serializer.SerializeToBuilder(serviceTestModel);
            var clone = serializer.Deserialize<IServiceTestModel>(ser);
            return clone;
        }
    }
}
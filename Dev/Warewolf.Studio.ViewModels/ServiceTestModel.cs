#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System;
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

        public string NeverRunString
        {
            get => _neverRunString;
            set
            {
                _neverRunString = value;
                OnPropertyChanged(NeverRunString);
            }
        }

        public bool LastRunDateVisibility
        {
            get => _lastRunDateVisibility;
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
            get => _neverRunStringVisibility;
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
            get => _debugForTest;
            set
            {
                _debugForTest = value;
                OnPropertyChanged(() => DebugForTest);
            }
        }

        public string DuplicateTestTooltip
        {
            get => _duplicateTestTooltip;
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
            private get => _item;
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
            get => _parentId;
            set
            {
                _parentId = value;
                OnPropertyChanged(() => ParentId);
            }
        }

        public string OldTestName
        {
            get => _oldTestName;
            set
            {
                _oldTestName = value;
                OriginalTestName = value;
                OnPropertyChanged(() => OldTestName);
            }
        }

        private string OriginalTestName { get; set; }

        public string TestName
        {
            get => _testName;
            set
            {
                if (OriginalTestName == null)
                {
                    OldTestName = _testName;
                }
                _testName = value;
                OnPropertyChanged(() => TestName);
                OnPropertyChanged(() => IsDirty);
            }
        }

        public string NameForDisplay
        {
            get => _nameForDisplay;
            set
            {
                _nameForDisplay = value;
                OnPropertyChanged(() => NameForDisplay);
            }
        }

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged(() => UserName);
                OnPropertyChanged(() => IsDirty);
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(() => Password);
                OnPropertyChanged(() => IsDirty);
            }
        }
        public DateTime LastRunDate
        {
            get => _lastRunDate;
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
            get => _inputs;
            set
            {
                _inputs = value;
                OnPropertyChanged(() => Inputs);
                OnPropertyChanged(() => IsDirty);
            }
        }

        public ObservableCollection<IServiceTestOutput> Outputs
        {
            get => _outputs;
            set
            {
                _outputs = value;
                OnPropertyChanged(() => Outputs);
                OnPropertyChanged(() => IsDirty);
            }
        }
        public bool NoErrorExpected
        {
            get => _noErrorExpected;
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
            get => _errorExpected;
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
            get => _errorContainsText;
            set
            {
                _errorContainsText = value;
                OnPropertyChanged(() => ErrorContainsText);
                OnPropertyChanged(() => IsDirty);
            }
        }

        public bool IsNewTest
        {
            get => _isNewTest;
            set
            {
                _isNewTest = value;
                OnPropertyChanged(() => IsNewTest);
            }
        }
        public bool IsTestSelected
        {
            get => _isTestSelected;
            set
            {
                _isTestSelected = value;
                OnPropertyChanged(() => IsTestSelected);
            }
        }

        public bool IsTestLoading
        {
            get => _isTestLoading;
            set
            {
                _isTestLoading = value;
                OnPropertyChanged(() => IsTestLoading);
            }
        }

        public bool TestPassed
        {
            get => _testPassed;
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
            get => _testFailing;
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
            get => _testInvalid;
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
            get => _testPending;
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
            get => _enabled;
            set
            {
                _enabled = value;
                OnPropertyChanged(() => Enabled);
                OnPropertyChanged(() => IsDirty);
            }
        }
        public string RunSelectedTestUrl
        {
            get => _runSelectedTestUrl;
            set
            {
                _runSelectedTestUrl = value;
                OnPropertyChanged(() => RunSelectedTestUrl);
            }
        }
        public AuthenticationType AuthenticationType
        {
            get => _authenticationType;
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
                var isDirty = false;
                var notEquals = !Equals(Item);
                if (NewTest)
                {
                    isDirty = true;
                }
                else
                {
                    if (notEquals)
                    {
                        isDirty = true;
                    }
                }

                SetDisplayName(isDirty);
                _isValidatingIsDirty = false;
                return isDirty;
            }
        }

        void SetDisplayName(bool isDirty)
        {
            NameForDisplay = isDirty ? TestName + " *" : TestName;
        }

        public bool UserAuthenticationSelected => AuthenticationType == AuthenticationType.User;

        public bool NewTest
        {
            get => _newTest;
            set
            {
                _newTest = value;
                OnPropertyChanged(() => NewTest);
            }
        }

        public bool IsTestRunning
        {
            get => _isTestRunning;
            set
            {
                _isTestRunning = value;
                OnPropertyChanged(() => IsTestRunning);
            }
        }

        public ObservableCollection<IServiceTestStep> TestSteps
        {
            get => _testSteps ?? (_testSteps = new ObservableCollection<IServiceTestStep>());
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

        public void ResetOldTestName()
        {
            OriginalTestName = null;
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
                var scalars = new List<IScalar>();
                foreach (var column in recordset.Columns)
                {
                    var cols = column.Value.Where(scalar => scalar.IODirection == enDev2ColumnArgumentDirection.Input || scalar.IODirection == enDev2ColumnArgumentDirection.Both);
                    scalars.AddRange(cols);
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
                    AddBlankRowToRecordset(itemToAdd, scalars, indexToInsertAt, indexNum, dataList);
                }
            }
        }

        void AddBlankRowToRecordset(IServiceTestInput dlItem, IList<IScalar> columns, int indexToInsertAt, int indexNum, IDataListModel dataList)
        {
            IList<IScalar> scalars = columns.Distinct(Scalar.Comparer).ToList();
            string colName = null;
            foreach (var col in scalars.Distinct(new ScalarEqualityComparer()))
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
                var scalars = new List<IScalar>();
                foreach (var column in recordset.Columns)
                {
                    var cols = column.Value.Where(scalar => scalar.IODirection == enDev2ColumnArgumentDirection.Output || scalar.IODirection == enDev2ColumnArgumentDirection.Both);
                    scalars.AddRange(cols);
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
                    AddBlankRowToRecordset(itemToAdd, scalars, indexToInsertAt, indexNum, dataList);
                }
            }
        }

        void AddBlankRowToRecordset(IServiceTestOutput dlItem, IList<IScalar> columns, int indexToInsertAt, int indexNum, IDataListModel dataList)
        {
            IList<IScalar> scalars = columns.Distinct(Scalar.Comparer).ToList();
            string colName = null;
            foreach (var col in scalars.Distinct(new ScalarEqualityComparer()))
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
            var equals = equalsSeq && inputCompare && testStepCompare && outputCompare;

            return equals;
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
                    return false;
                }

                var stepOutputs = TestSteps[i].StepOutputs;
                var otherStepOutputs = other.TestSteps[i].StepOutputs;
                stepCompare = StepOutputsCompare(stepOutputs, otherStepOutputs);
                if (!stepCompare)
                {
                    return false;
                }
            }
            return true;
        }

        static bool StepChildrenCompare(ObservableCollection<IServiceTestStep> stepChildren, ObservableCollection<IServiceTestStep> otherStepChildren)
        {
            var stepCompare = true;
            for (int c = 0; c < stepChildren.Count; c++)
            {
                stepCompare &= stepChildren[c].Type == otherStepChildren[c].Type;
                if (!stepCompare)
                {
                    return false;
                }

                var childStepOutputs = stepChildren[c].StepOutputs;
                var otherChildStepOutputs = otherStepChildren[c].StepOutputs;
                stepCompare = StepOutputsCompare(childStepOutputs, otherChildStepOutputs);
                if (!stepCompare)
                {
                    return false;
                }

                if (stepChildren[c].Children.Count > 0)
                {
                    var stepChildren1 = stepChildren[c].Children;
                    var otherStepChildren1 = otherStepChildren[c].Children;

                    stepCompare = StepChildrenCompare(stepChildren1, otherStepChildren1);
                    if (!stepCompare)
                    {
                        return false;
                    }
                }
            }
            return true;
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
                    return false;
                }
            }
            return true;
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
                    return false;
                }
            }
            return true;
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
                    return false;
                }
            }
            return true;
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

        T IServiceTestModel.As<T>() => this as T;
    }
}
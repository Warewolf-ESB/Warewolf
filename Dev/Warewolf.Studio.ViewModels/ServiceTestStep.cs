using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Dev2.Common.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Util;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;

namespace Warewolf.Studio.ViewModels
{
    public class ServiceTestStep : BindableBase, IServiceTestStep
    {
        string _stepDescription;
        StepType _type;
        string _activityType;
        ObservableCollection<IServiceTestOutput> _stepOutputs;
        Guid _uniqueId;
        IServiceTestStep _parent;
        ObservableCollection<IServiceTestStep> _children;
        bool _isTestStepExpanded;
        bool _isTestStepExpanderEnabled;
        bool _assertSelected;
        bool _mockSelected;
        ImageSource _stepIcon;
        bool _testPassed;
        bool _testPending;
        bool _testInvalid;
        bool _testFailing;
        TestRunResult _result;

        public ServiceTestStep(Guid uniqueId, string activityTypeName, ObservableCollection<IServiceTestOutput> serviceTestOutputs, StepType stepType)
        {
            UniqueId = uniqueId;
            ActivityType = activityTypeName;
            StepOutputs = serviceTestOutputs;
            Type = stepType;
            StepDescription = activityTypeName;
            Children = new ObservableCollection<IServiceTestStep>();
            AssertSelected = Type == StepType.Assert;
            MockSelected = Type == StepType.Mock;
            IsTestStepExpanded = StepOutputs?.Count > 0;
            IsTestStepExpanderEnabled = StepOutputs?.Count > 0;
            TestPending = true;
        }

        public Guid UniqueId
        {
            get { return _uniqueId; }
            set
            {
                _uniqueId = value;
                OnPropertyChanged(() => UniqueId);
            }
        }

        [JsonIgnore]
        public ImageSource StepIcon
        {
            get { return _stepIcon; }
            set
            {
                _stepIcon = value;
                OnPropertyChanged(() => StepIcon);
            }
        }

        public string ActivityType
        {
            get { return _activityType; }
            set
            {
                _activityType = value;
                OnPropertyChanged(() => ActivityType);
            }
        }

        public StepType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged(() => Type);
            }
        }

        public ObservableCollection<IServiceTestOutput> StepOutputs
        {
            get { return _stepOutputs; }
            set
            {
                _stepOutputs = value;
                if (value != null)
                {
                    SetControlFlowValues(value);
                }
                OnPropertyChanged(() => StepOutputs);
                IsTestStepExpanded = StepOutputs?.Count > 0;
                IsTestStepExpanderEnabled = StepOutputs?.Count > 0;
            }
        }

        public void AddNewEmptyRow()
        {
            if (_stepOutputs?.Count >= 1)
            {
                var lastOrDefault = _stepOutputs.LastOrDefault(
                        output => !string.IsNullOrWhiteSpace(output.Variable) && !string.IsNullOrWhiteSpace(output.Value));
                if (lastOrDefault != null && DataListUtil.IsValueRecordset(lastOrDefault?.Variable))
                {
                    var serviceTestOutput = new ServiceTestOutput("", "", "", "")
                    {
                        AddNewAction = () => AddNewOutput(_stepOutputs.LastOrDefault().Variable)
                    };
                    _stepOutputs.Add(serviceTestOutput);
                }

            }
        }

        void SetControlFlowValues(ObservableCollection<IServiceTestOutput> value)
        {
            if (ActivityType != "DsfDecision" && ActivityType != "DsfSwitch")
            {
                return;
            }

            foreach (var testOutput in value.OfType<ServiceTestOutput>())
            {
                testOutput.AssertOps = new ObservableCollection<string> { "=" };
                testOutput.CanEditVariable = false;
            }
        }

        public IServiceTestStep Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                OnPropertyChanged(() => Parent);
            }
        }

        public ObservableCollection<IServiceTestStep> Children
        {
            get { return _children; }
            set
            {
                _children = value;
                OnPropertyChanged(() => Children);
            }
        }

        public string StepDescription
        {
            get { return _stepDescription; }
            set
            {
                _stepDescription = value;
                OnPropertyChanged(() => StepDescription);
            }
        }

        public TestRunResult Result
        {
            get { return _result; }
            set
            {
                _result = value;

                if (_result != null)
                {
                    UpdateTestPassed();
                    UpdateTestFailed();
                    UpdateTestInvalid();
                    UpdateTestPending();
                }

                OnPropertyChanged(() => Result);
            }
        }

        void UpdateTestPassed()
        {
            var testPassed = _result.RunTestResult == RunResult.TestPassed;
            TestPassed = !MockSelected && testPassed;
        }

        void UpdateTestFailed()
        {
            var testFailed = _result.RunTestResult == RunResult.TestFailed;
            TestFailing = !MockSelected && testFailed;
        }

        void UpdateTestInvalid()
        {
            var testInvalid = _result.RunTestResult == RunResult.TestInvalid ||
                              _result.RunTestResult == RunResult.TestResourceDeleted ||
                              _result.RunTestResult == RunResult.TestResourcePathUpdated;
            TestInvalid = !MockSelected && testInvalid;
        }

        void UpdateTestPending()
        {
            var testPending = _result.RunTestResult != RunResult.TestFailed &&
                                  _result.RunTestResult != RunResult.TestPassed &&
                                  _result.RunTestResult != RunResult.TestInvalid &&
                                  _result.RunTestResult != RunResult.TestResourceDeleted &&
                                  _result.RunTestResult != RunResult.TestResourcePathUpdated;

            TestPending = !MockSelected && testPending;
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

        public bool IsTestStepExpanded
        {
            get { return _isTestStepExpanded; }
            set
            {
                _isTestStepExpanded = value;
                OnPropertyChanged(() => IsTestStepExpanded);
            }
        }

        public bool IsTestStepExpanderEnabled
        {
            get { return _isTestStepExpanderEnabled; }
            set
            {
                _isTestStepExpanderEnabled = value;
                OnPropertyChanged(() => IsTestStepExpanderEnabled);
            }
        }

        public bool IsExpanderVisible => Children.Count > 0;

        public bool AssertSelected
        {
            get { return _assertSelected; }
            set
            {
                _assertSelected = value;
                if (_assertSelected)
                {
                    Type = StepType.Assert;
                    if (StepOutputs != null)
                    {
                        foreach (var serviceTestOutput in StepOutputs)
                        {
                            var item = serviceTestOutput as ServiceTestOutput;
                            item?.OnSearchTypeChanged();
                        }
                    }
                }
                OnPropertyChanged(() => AssertSelected);
            }
        }

        public bool MockSelected
        {
            get => _mockSelected;
            set
            {
                _mockSelected = value;
                if (_mockSelected)
                {
                    Type = StepType.Mock;
                    if (StepOutputs != null)
                    {
                        foreach (var serviceTestOutput in StepOutputs)
                        {
                            MockSelectedOutput(serviceTestOutput);
                        }
                    }
                }
                OnPropertyChanged(() => MockSelected);
            }
        }

        static void MockSelectedOutput(IServiceTestOutput serviceTestOutput)
        {
            if (serviceTestOutput is ServiceTestOutput item)
            {
                if (!item.IsSearchCriteriaEnabled)
                {
                    item.IsSearchCriteriaEnabled = true;
                }
                if (!item.IsSinglematchCriteriaVisible)
                {
                    item.IsSinglematchCriteriaVisible = true;
                    item.IsBetweenCriteriaVisible = false;
                }
            }
        }

        public void AddNewOutput(string varName)
        {
            if (DataListUtil.IsValueRecordset(varName))
            {
                AddNewRecordsetOutput(varName);
            }
            else
            {
                var lastRow = StepOutputs.LastOrDefault();
                if (lastRow != null && !string.IsNullOrEmpty(lastRow.Variable.Trim()))
                {
                    var serviceTestOutput = new ServiceTestOutput("", "", "", "")
                    {
                        AddNewAction = () => AddNewOutput("")
                    };
                    StepOutputs?.Add(serviceTestOutput);
                }

            }
        }

        void AddNewRecordsetOutput(string varName)
        {
            if (DataListUtil.GetRecordsetIndexType(varName) == enRecordsetIndexType.Numeric)
            {
                var extractedIndex = DataListUtil.ExtractIndexRegionFromRecordset(varName);
                if (int.TryParse(extractedIndex, out int intIndex))
                {
                    intIndex = AddNewRecordsetOutput(varName, intIndex);
                }
            }
            else
            {
                if (StepOutputs != null && StepOutputs.Count >= 1)
                {
                    AddNewRecordsetOutput(varName, StepOutputs.LastOrDefault());
                }
                else
                {
                    var serviceTestOutput = new ServiceTestOutput(varName, "", "", "")
                    {
                        AddNewAction = () => AddNewOutput(varName)
                    };
                    StepOutputs?.Add(serviceTestOutput);
                }
            }
        }

        private void AddNewRecordsetOutput(string varName, IServiceTestOutput testOutput)
        {
            if (string.IsNullOrWhiteSpace(testOutput?.Variable) && string.IsNullOrWhiteSpace(testOutput?.Value))
            {
                if (testOutput != null)
                {
                    testOutput.Variable = varName;
                }
            }
            else
            {
                var serviceTestOutput = new ServiceTestOutput(varName, "", "", "")
                {
                    AddNewAction = () => AddNewOutput(varName)
                };
                StepOutputs?.Add(serviceTestOutput);
            }
        }

        int AddNewRecordsetOutput(string varName, int intIndex)
        {
            intIndex++;
            var blankName = DataListUtil.ReplaceRecordsetIndexWithBlank(varName);
            var indexedName = DataListUtil.ReplaceRecordsetBlankWithIndex(blankName, intIndex);
            var lastInput = StepOutputs.LastOrDefault();
            if (lastInput != null)
            {
                if (string.IsNullOrEmpty(lastInput.Variable.Trim()))
                {
                    lastInput.Variable = indexedName;
                }
                else
                {
                    if (!string.IsNullOrEmpty(lastInput.Value.Trim()))
                    {
                        var serviceTestOutput = new ServiceTestOutput(indexedName, "", "", "") { AddNewAction = () => AddNewOutput(indexedName) };
                        StepOutputs?.Add(serviceTestOutput);
                    }
                }
            }
            else
            {
                var serviceTestOutput = new ServiceTestOutput(indexedName, "", "", "") { AddNewAction = () => AddNewOutput(indexedName) };
                StepOutputs?.Add(serviceTestOutput);
            }

            return intIndex;
        }
    }
}
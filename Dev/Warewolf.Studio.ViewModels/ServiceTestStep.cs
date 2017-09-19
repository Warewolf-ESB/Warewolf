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
        private string _stepDescription;
        private StepType _type;
        private string _activityType;
        private ObservableCollection<IServiceTestOutput> _stepOutputs;
        private Guid _uniqueId;
        private IServiceTestStep _parent;
        private ObservableCollection<IServiceTestStep> _children;
        private bool _isTestStepExpanded;
        private bool _isTestStepExpanderEnabled;
        private bool _assertSelected;
        private bool _mockSelected;
        private ImageSource _stepIcon;
        private bool _testPassed;
        private bool _testPending;
        private bool _testInvalid;
        private bool _testFailing;
        private TestRunResult _result;

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
                    if (value.Count < 1)
                    {
                        TestInvalid = true;
                    }
                    SetControlFlowValues(value);
                }
                //AddNewEmptyRow();
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
                if (lastOrDefault != null)
                {
                    if (DataListUtil.IsValueRecordset(lastOrDefault?.Variable))
                    {
                        var serviceTestOutput = new ServiceTestOutput("", "", "", "")
                        {
                            AddNewAction = () => AddNewOutput(_stepOutputs.LastOrDefault().Variable)
                        };
                        _stepOutputs.Add(serviceTestOutput);
                    }
                }
            }
        }

        private void SetControlFlowValues(ObservableCollection<IServiceTestOutput> value)
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

        private void UpdateTestPassed()
        {
            var testPassed = _result.RunTestResult == RunResult.TestPassed;
            TestPassed = !MockSelected && testPassed;
        }

        private void UpdateTestFailed()
        {
            var testFailed = _result.RunTestResult == RunResult.TestFailed;
            TestFailing = !MockSelected && testFailed;
        }

        private void UpdateTestInvalid()
        {
            var testInvalid = _result.RunTestResult == RunResult.TestInvalid ||
                              _result.RunTestResult == RunResult.TestResourceDeleted ||
                              _result.RunTestResult == RunResult.TestResourcePathUpdated;
            TestInvalid = !MockSelected && testInvalid;
        }

        private void UpdateTestPending()
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
            get { return _mockSelected; }
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
                            var item = serviceTestOutput as ServiceTestOutput;
                            if (item != null)
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
                    }
                }
                OnPropertyChanged(() => MockSelected);
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
                if (lastRow != null)
                {
                    if (!string.IsNullOrEmpty(lastRow.Variable.Trim()))
                    {
                        var serviceTestOutput = new ServiceTestOutput("", "", "", "")
                        {
                            AddNewAction = () => AddNewOutput("")
                        };
                        StepOutputs?.Add(serviceTestOutput);
                    }
                }
            }
        }

        private void AddNewRecordsetOutput(string varName)
        {
            if (DataListUtil.GetRecordsetIndexType(varName) == enRecordsetIndexType.Numeric)
            {
                var extractedIndex = DataListUtil.ExtractIndexRegionFromRecordset(varName);
                if (int.TryParse(extractedIndex, out int intIndex))
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
                }
            }
            else
            {
                if (StepOutputs != null && StepOutputs.Count >= 1)
                {
                    var testOutput = StepOutputs.LastOrDefault();
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
    }
}
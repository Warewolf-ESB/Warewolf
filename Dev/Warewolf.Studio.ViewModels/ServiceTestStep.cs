#pragma warning disable
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
        Guid _activityId;
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
            ActivityID = uniqueId;
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

        public Guid ActivityID
        {
            get => _activityId;
            set
            {
                _activityId = value;
                OnPropertyChanged(() => ActivityID);
            }
        }

        public Guid UniqueID
        {
            get => _uniqueId;
            set
            {
                _uniqueId = value;
                OnPropertyChanged(() => UniqueID);
            }
        }


        [JsonIgnore]
        public ImageSource StepIcon
        {
            get => _stepIcon;
            set
            {
                _stepIcon = value;
                OnPropertyChanged(() => StepIcon);
            }
        }

        public string ActivityType
        {
            get => _activityType;
            set
            {
                _activityType = value;
                OnPropertyChanged(() => ActivityType);
            }
        }

        public StepType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged(() => Type);
            }
        }

        public ObservableCollection<IServiceTestOutput> StepOutputs
        {
            get => _stepOutputs;
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
            get => _parent;
            set
            {
                _parent = value;
                OnPropertyChanged(() => Parent);
            }
        }

        public ObservableCollection<IServiceTestStep> Children
        {
            get => _children;
            set
            {
                _children = value;
                OnPropertyChanged(() => Children);
            }
        }

        public string StepDescription
        {
            get => _stepDescription;
            set
            {
                _stepDescription = value;
                OnPropertyChanged(() => StepDescription);
            }
        }

        public TestRunResult Result
        {
            get => _result;
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
            var testPending = _result.RunTestResult != RunResult.TestFailed;
            testPending &= _result.RunTestResult != RunResult.TestPassed;
            testPending &= _result.RunTestResult != RunResult.TestInvalid;
            testPending &= _result.RunTestResult != RunResult.TestResourceDeleted;
            testPending &= _result.RunTestResult != RunResult.TestResourcePathUpdated;

            TestPending = !MockSelected && testPending;
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

        public bool IsTestStepExpanded
        {
            get => _isTestStepExpanded;
            set
            {
                _isTestStepExpanded = value;
                OnPropertyChanged(() => IsTestStepExpanded);
            }
        }

        public bool IsTestStepExpanderEnabled
        {
            get => _isTestStepExpanderEnabled;
            set
            {
                _isTestStepExpanderEnabled = value;
                OnPropertyChanged(() => IsTestStepExpanderEnabled);
            }
        }

        public bool IsExpanderVisible => Children.Count > 0;

        public bool AssertSelected
        {
            get => _assertSelected;
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
                            serviceTestOutput?.OnSearchTypeChanged();
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
                if (!item.IsSingleMatchCriteriaVisible)
                {
                    item.IsSingleMatchCriteriaVisible = true;
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

        T IServiceTestStep.As<T>() => this as T;
    }
}
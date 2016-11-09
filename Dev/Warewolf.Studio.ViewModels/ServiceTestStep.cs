using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Media;
using Dev2.Common.Interfaces;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;

namespace Warewolf.Studio.ViewModels
{
    public class ServiceTestStep : BindableBase, IServiceTestStep
    {
        private readonly ITestResultCompiler _testResultCompiler;
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
            : this(new TestResultCompiler())
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

        public ServiceTestStep(ITestResultCompiler testResultCompiler)
        {
            _testResultCompiler = testResultCompiler;
        }

        public ServiceTestStep()
        {
            
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
        [ExcludeFromCodeCoverage]
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
                    if (ActivityType == "DsfDecision" || ActivityType == "DsfSwitch")
                    {
                        foreach (var serviceTestOutput in value)
                        {
                            var testOutput = serviceTestOutput as ServiceTestOutput;
                            if (testOutput != null)
                                testOutput.AssertOps = new ObservableCollection<string> { "=" };
                        }
                    }
                }
                OnPropertyChanged(() => StepOutputs);
                IsTestStepExpanded = StepOutputs?.Count > 0;
                IsTestStepExpanderEnabled = StepOutputs?.Count > 0;
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
                    TestPassed = _testResultCompiler.GetPassingResult(_result.RunTestResult);
                    TestFailing = _testResultCompiler.GetFailingResult(_result.RunTestResult);
                    TestInvalid = _testResultCompiler.GetTestInvalidResult(_result.RunTestResult);
                    TestPending = _testResultCompiler.GetTestPendingResult(_result.RunTestResult);
                }

                OnPropertyChanged(()=> Result);
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
                        foreach (var serviceTestOutput in StepOutputs)
                        {
                            var testOutput = serviceTestOutput as ServiceTestOutput;
                            testOutput?.OnSearchTypeChanged();
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
                OnPropertyChanged(() => MockSelected);
            }
        }

        public void AddNewOutput(string varName)
        {
            if (DataListUtil.IsValueRecordset(varName))
            {
                if (DataListUtil.GetRecordsetIndexType(varName) == enRecordsetIndexType.Numeric)
                {
                    var extractedIndex = DataListUtil.ExtractIndexRegionFromRecordset(varName);
                    int intIndex;
                    if (int.TryParse(extractedIndex, out intIndex))
                    {
                        intIndex++;
                        var blankName = DataListUtil.ReplaceRecordsetIndexWithBlank(varName);
                        var indexedName = DataListUtil.ReplaceRecordsetBlankWithIndex(blankName, intIndex);
                        if (StepOutputs.FirstOrDefault(output => output.Variable.Equals(indexedName, StringComparison.InvariantCultureIgnoreCase)) == null)
                        {
                            var serviceTestOutput = new ServiceTestOutput(indexedName, "", "", "") { AddNewAction = () => AddNewOutput(indexedName) };
                            StepOutputs.Add(serviceTestOutput);
                        }
                    }
                }
                else
                {
                    var serviceTestOutput = new ServiceTestOutput(varName, "", "", "") { AddNewAction = () => AddNewOutput(varName) };
                    StepOutputs.Add(serviceTestOutput);
                }
            }
        }
    }
}
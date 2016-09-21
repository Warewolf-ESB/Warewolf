using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Util;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.Mvvm;
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable ArrangeThisQualifier

namespace Warewolf.Studio.ViewModels
{
    public class ServiceTestModel : BindableBase, IServiceTestModel
    {

        private string _testName;
        private string _userName;
        private bool _testPassed;
        private bool _testFailing;
        private bool _testPending;
        private bool _testInvalid;
        private string _password;
        private ObservableCollection<IServiceTestInput> _inputs;
        private ObservableCollection<IServiceTestOutput> _outputs;
        private bool _noErrorExpected;
        private bool _errorExpected;
        private bool _isNewTest;
        private bool _isTestSelected;
        private DateTime _lastRunDate;
        private bool _enabled;
        private string _runSelectedTestUrl;
        private AuthenticationType _authenticationType;
        private Guid _parentId;
        private string _oldTestName;
        private bool _newTest;
        private string _nameForDisplay;
        private ServiceTestModel _item;
        private bool _isTestRunning;
        private string _neverRunString;
        private string _duplicateTestTooltip;
        private bool _lastRunDateVisibility;
        private bool _neverRunStringVisibility;
        private IList<IDebugState> _debugForTest;
        private IServiceTestStep _selectedTestStep;
        private ObservableCollection<IServiceTestStep> _testSteps;

        public string NeverRunString
        {
            get
            {
                return _neverRunString;
            }
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
            get
            {
                return _debugForTest;
            }
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
            }
        }

        #region Implementation of IServiceTestModel

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
            get
            {

                return _nameForDisplay;
            }
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
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                OnPropertyChanged(() => Password);
                OnPropertyChanged(() => IsDirty);
            }
        }
        public DateTime LastRunDate
        {
            get
            {
                return _lastRunDate;
            }
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
            get
            {
                return _noErrorExpected;
            }
            set
            {
                _noErrorExpected = value;
                OnPropertyChanged(() => NoErrorExpected);
                OnPropertyChanged(() => NameForDisplay);
            }
        }
        public bool ErrorExpected
        {
            get
            {
                return _errorExpected;
            }
            set
            {
                _errorExpected = value;
                OnPropertyChanged(() => ErrorExpected);
                OnPropertyChanged(() => IsDirty);
            }
        }

        public bool IsNewTest
        {
            get
            {
                return _isNewTest;
            }
            set
            {
                _isNewTest = value;
                OnPropertyChanged(() => IsNewTest);
            }
        }
        public bool IsTestSelected
        {
            get
            {
                return _isTestSelected;
            }
            set
            {
                _isTestSelected = value;
                OnPropertyChanged(() => IsTestSelected);
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
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                OnPropertyChanged(() => Enabled);
                OnPropertyChanged(() => IsDirty);
            }
        }
        public string RunSelectedTestUrl
        {
            get
            {
                return _runSelectedTestUrl;
            }
            set
            {
                _runSelectedTestUrl = value;
                OnPropertyChanged(() => RunSelectedTestUrl);
            }
        }
        public AuthenticationType AuthenticationType
        {
            get
            {
                return _authenticationType;
            }
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

                var isDirty = false;
                var notEquals = !Equals(Item);
                if (NewTest)
                {
                    isDirty = true;
                }
                else if (notEquals)
                {
                    isDirty = true;
                }

                if (isDirty)
                {
                    NameForDisplay = TestName + " *";
                }
                else
                {
                    NameForDisplay = TestName;
                }
                return isDirty;
            }
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
            get { return _testSteps; }
            set
            {
                _testSteps = value; 
                OnPropertyChanged(() => TestSteps);
            }
        }

        public IServiceTestStep SelectedTestStep
        {
            get { return _selectedTestStep; }
            set
            {
                _selectedTestStep = value; 
                OnPropertyChanged(() => SelectedTestStep);
            }
        }

        public void SetItem(IServiceTestModel model)
        {
            Item = model as ServiceTestModel;
        }

        public void AddTestStep(string activityUniqueID, string activityTypeName, List<IServiceTestOutput> serviceTestOutputs)
        {
            if(string.IsNullOrEmpty(activityUniqueID))
                throw new ArgumentNullException(nameof(activityUniqueID));
            if (string.IsNullOrEmpty(activityTypeName))
                throw new ArgumentNullException(nameof(activityTypeName));
            if (serviceTestOutputs == null)
                throw new ArgumentNullException(nameof(serviceTestOutputs));
            var testStep = new ServiceTestStep(Guid.Parse(activityUniqueID), activityTypeName, serviceTestOutputs, StepType.Mock);
            var testStepChild1 = new ServiceTestStep(Guid.Parse(activityUniqueID), activityTypeName, serviceTestOutputs, StepType.Mock);
            var testStepChild2 = new ServiceTestStep(Guid.Parse(activityUniqueID), activityTypeName, serviceTestOutputs, StepType.Mock);
            var testStepChild3 = new ServiceTestStep(Guid.Parse(activityUniqueID), activityTypeName, serviceTestOutputs, StepType.Mock);

            testStepChild1.Parent = testStep;
            testStepChild1.Children.Add(testStepChild2);

            testStepChild2.Parent = testStepChild1;
            testStep.Children.Add(testStepChild1);

            testStepChild3.Parent = testStep;
            testStep.Children.Add(testStepChild3);

            TestSteps.Add(testStep);
        }

        #endregion

        public void AddRow(IServiceTestInput itemToAdd, IDataListModel dataList)
        {
            if (itemToAdd != null && DataListUtil.IsValueRecordset(itemToAdd.Variable))
            {
                var recordsetNameFromValue = DataListUtil.ExtractRecordsetNameFromValue(itemToAdd.Variable);
                IRecordSet recordset = dataList.ShapeRecordSets.FirstOrDefault(set => set.Name == recordsetNameFromValue);
                if (recordset != null)
                {
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
                        
                        AddBlankRowToRecordset(itemToAdd, recsetCols, indexToInsertAt, indexNum,dataList);
                    }
                }
            }
        }

        private void AddBlankRowToRecordset(IServiceTestInput dlItem, IList<IScalar> columns, int indexToInsertAt, int indexNum, IDataListModel dataList)
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
                    serviceTestInput.AddNewAction = ()=>AddRow(serviceTestInput, dataList);
                    Inputs.Insert(indexToInsertAt + 1, serviceTestInput);
                    indexToInsertAt++;
                }
                colName = col.Name;
            }
        }

        private bool Equals(ServiceTestModel other)
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
            var inputCompare = InputCompare(other, true);
            var outputCompare = OutputCompare(other, true);
            var @equals = equalsSeq && inputCompare && outputCompare;

            return @equals;
        }

        private bool InputCompare(ServiceTestModel other, bool inputCompare)
        {
            if (_inputs == null)
            {
                return true;
            }
            if (_inputs.Count != other._inputs.Count)
            {
                return false;
            }
            for (int i = 0; i < _inputs.Count; i++)
            {
                if (Inputs[i].Value != other.Inputs[i].Value)
                {
                    inputCompare = false;
                }
                if (!inputCompare) continue;
                if (Inputs[i].Variable != other.Inputs[i].Variable)
                {
                    inputCompare = false;
                }
                if (!inputCompare) continue;
                if (Inputs[i].EmptyIsNull != other.Inputs[i].EmptyIsNull)
                {
                    inputCompare = false;
                }
            }
            return inputCompare;
        }

        private bool OutputCompare(ServiceTestModel other, bool outputCompare)
        {
            if (_outputs == null)
            {
                return true;
            }
            if(_outputs.Count!=other._outputs.Count)
            {
                return false;
            }
            for (int i = 0; i < _outputs.Count; i++)
            {
                if (_outputs[i].Value != other._outputs[i].Value)
                {
                    outputCompare = false;
                }
                if (!outputCompare) continue;
                if (_outputs[i].Variable != other._outputs[i].Variable)
                {
                    outputCompare = false;
                }
            }
            return outputCompare;
        }

        private bool EqualsSeq(ServiceTestModel other)
        {
            return string.Equals(_testName, other._testName) &&
                   string.Equals(_userName, other._userName) &&
                   string.Equals(_password, other._password) &&
                   _noErrorExpected == other._noErrorExpected &&
                   _errorExpected == other._errorExpected &&
                   _enabled == other._enabled &&
                   _authenticationType == other._authenticationType;
        }


        #region Implementation of ICloneable



        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            var memberwiseClone = (ServiceTestModel)MemberwiseClone();
            if (Inputs != null)
            {
                memberwiseClone.Inputs = new ObservableCollection<IServiceTestInput>();
                var serviceTestInputs = Inputs.ToList();
                foreach (var serviceTestInput in serviceTestInputs)
                {
                    memberwiseClone.Inputs.Add(new ServiceTestInput(serviceTestInput.Variable, serviceTestInput.Value)
                    {
                        EmptyIsNull = serviceTestInput.EmptyIsNull
                    });
                }
            }
            if (Outputs != null)
            {
                memberwiseClone.Outputs = new ObservableCollection<IServiceTestOutput>();
                var serviceTestInputs = Outputs.ToList();
                foreach (var serviceTestInput in serviceTestInputs)
                {
                    memberwiseClone.Outputs.Add(new ServiceTestOutput(serviceTestInput.Variable, serviceTestInput.Value));
                }
            }
            return memberwiseClone;
        }

        #endregion
    }

    public class ServiceTestStep : BindableBase, IServiceTestStep
    {
        private string _stepDescription;
        private StepType _type;
        private string _activityType;
        private List<IServiceTestOutput> _stepOutputs;
        private Guid _uniqueId;
        private IServiceTestStep _parent;
        private ObservableCollection<IServiceTestStep> _children;
        private bool _assertSelected;
        private bool _mockSelected;

        public ServiceTestStep(Guid uniqueId, string activityTypeName, List<IServiceTestOutput> serviceTestOutputs, StepType stepType)
        {
            UniqueId = uniqueId;
            ActivityType = activityTypeName;
            StepOutputs = serviceTestOutputs;
            Type = stepType;

            AssertSelected = true;
            MockSelected = false;
            StepDescription = "New Test Description";
            Children = new ObservableCollection<IServiceTestStep>();
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

        public List<IServiceTestOutput> StepOutputs
        {
            get { return _stepOutputs; }
            set
            {
                _stepOutputs = value; 
                OnPropertyChanged(() => StepOutputs);
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
                }
                OnPropertyChanged(() => MockSelected);
            }
        }
    }
}
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

namespace Warewolf.Studio.ViewModels
{
    public class ServiceTestModel : BindableBase, IServiceTestModel, IEquatable<ServiceTestModel>
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
        }

        public ServiceTestModel Item
        {
            get { return _item; }
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

        public void SetItem(IServiceTestModel model)
        {
            Item = model as ServiceTestModel;
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
                        AddBlankRowToRecordset(itemToAdd, recsetCols, indexToInsertAt, indexNum);
                    }
                }
            }
        }

        private void AddBlankRowToRecordset(IServiceTestInput dlItem, IList<IScalar> columns, int indexToInsertAt, int indexNum)
        {
            IList<IScalar> recsetCols = columns.Distinct(Scalar.Comparer).ToList();
            string colName = null;
            foreach (var col in recsetCols.Distinct(new ScalarNameComparer()))
            {
                if (string.IsNullOrEmpty(colName) || !colName.Equals(col.Name))
                {
                    var recSetName = DataListUtil.ExtractRecordsetNameFromValue(dlItem.Variable);
                    var varName = string.Concat(recSetName, @"(", indexNum, @").", col.Name);
                    Inputs.Insert(indexToInsertAt + 1, new ServiceTestInput(varName, string.Empty));
                    indexToInsertAt++;
                }
                colName = col.Name;
            }
        }

        public bool Equals(ServiceTestModel other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (GetHashCode() == other.GetHashCode())
                return true;

            var @equals = EqualsSeq(other) && InputCompare(other, true) && OutputCompare(other, true);
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
                if (_inputs[i].Value != other.Inputs[i].Value)
                {
                    inputCompare = false;
                }
                if (!inputCompare) continue;
                if (_inputs[i].Variable != other.Inputs[i].Variable)
                {
                    inputCompare = false;
                }
                if (!inputCompare) continue;
                if (_inputs[i].EmptyIsNull != other.Inputs[i].EmptyIsNull)
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
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((ServiceTestModel)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _testName?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (_userName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_password?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_inputs?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_outputs?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ _noErrorExpected.GetHashCode();
                hashCode = (hashCode * 397) ^ _errorExpected.GetHashCode();
                hashCode = (hashCode * 397) ^ _enabled.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)_authenticationType;
                return hashCode;
            }
        }
    }
}
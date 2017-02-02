using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;
// ReSharper disable MemberCanBePrivate.Global

namespace Warewolf.Studio.ViewModels
{
    public class ServiceTestOutput : BindableBase, IServiceTestOutput
    {
        private string _variable;
        private string _value;
        private string _assertOp;
        private ObservableCollection<string> _assertOps;
        private bool _hasOptionsForValue;
        private bool _isSinglematchCriteriaVisible;
        private bool _isBetweenCriteriaVisible;
        private bool _isSearchCriteriaEnabled;
        private bool _isSearchCriteriaVisible;
        private List<string> _optionsForValue;
        private string _from;
        private string _to;
        private readonly IList<string> _requiresSearchCriteria = new List<string> { "Doesn't Contain", "Contains", "=", "<> (Not Equal)", "Ends With", "Doesn't Start With", "Doesn't End With", "Starts With", "Is Regex", "Not Regex", ">", "<", "<=", ">=" };
        private readonly IList<IFindRecsetOptions> _findRecsetOptions;
        private bool _testPassed;
        private bool _testPending;
        private bool _testInvalid;
        private bool _testFailing;
        private TestRunResult _result;
        private bool _canEditVariable;

        // ReSharper disable once UnusedMember.Global
        public ServiceTestOutput()
        {
            
        }

        public ServiceTestOutput(string variable, string value, string from, string to)
        {
            if (variable == null)
                throw new ArgumentNullException(nameof(variable));
            Variable = variable;
            Value = value;
            From = from;
            To = to;
            _findRecsetOptions = FindRecsetOptions.FindAllDecision();
            var collection = _findRecsetOptions.Select(c => c.HandlesType());
            AssertOps = new ObservableCollection<string>(collection);
            AssertOp = "=";
            CanEditVariable = true;
            IsSinglematchCriteriaVisible = true;
            TestPending = true;
        }

        public string Variable
        {
            get
            {
                return _variable;
            }
            set
            {
                _variable = value;
                OnPropertyChanged(() => Variable);
            }
        }
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                if (!string.IsNullOrEmpty(_value))
                {
                    if (AddNewAction != null)
                    {
                        AddNewAction.Invoke();
                    }
                    else
                    {
                        AddStepOutputRow?.Invoke(Variable);
                    }
                }
                OnPropertyChanged(() => Value);
            }
        }

        public string From
        {
            get
            {
                return _from;
            }
            set
            {
                _from = value;
                if (!string.IsNullOrEmpty(_from))
                {
                    if (AddNewAction != null)
                    {
                        AddNewAction.Invoke();
                    }
                    else
                    {
                        AddStepOutputRow?.Invoke(Variable);
                    }
                }
                OnPropertyChanged(() => From);
            }
        }
        public string To
        {
            get
            {
                return _to;
            }
            set
            {
                _to = value;
                if (!string.IsNullOrEmpty(_to))
                {
                    if (AddNewAction != null)
                    {
                        AddNewAction.Invoke();
                    }
                    else
                    {
                        AddStepOutputRow?.Invoke(Variable);
                    }
                }
                OnPropertyChanged(() => To);
            }
        }

        public string AssertOp
        {
            get { return _assertOp; }
            set
            {
                _assertOp = value;
                OnPropertyChanged(() => AssertOp);
                OnSearchTypeChanged();
            }
        }

        public bool HasOptionsForValue
        {
            get { return _hasOptionsForValue; }
            set
            {
                _hasOptionsForValue = value;
                OnPropertyChanged(() => HasOptionsForValue);
            }
        }

        public bool IsSinglematchCriteriaVisible
        {
            get
            {
                return _isSinglematchCriteriaVisible;
            }
            set
            {
                _isSinglematchCriteriaVisible = value;
                OnPropertyChanged(() => IsSinglematchCriteriaVisible);
            }
        }
        public bool IsBetweenCriteriaVisible
        {
            get
            {
                return _isBetweenCriteriaVisible;
            }
            set
            {
                _isBetweenCriteriaVisible = value;
                OnPropertyChanged(() => IsBetweenCriteriaVisible);
            }
        }

        public bool IsSearchCriteriaEnabled
        {
            get
            {
                return _isSearchCriteriaEnabled;
            }
            set
            {
                _isSearchCriteriaEnabled = value;
                OnPropertyChanged(() => IsSearchCriteriaEnabled);
            }
        }

        public bool IsSearchCriteriaVisible
        {
            get
            {
                return _isSearchCriteriaVisible;
            }
            set
            {
                _isSearchCriteriaVisible = value;
                OnPropertyChanged(() => IsSearchCriteriaVisible);
            }
        }

        public List<string> OptionsForValue
        {
            get { return _optionsForValue; }
            set
            {
                _optionsForValue = value;
                OnPropertyChanged(() => OptionsForValue);
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
                    UpdateTestFailing();
                    UpdateTestInvalid();
                    UpdateTestPending();
                }

                OnPropertyChanged(() => Result);
            }
        }

        private void UpdateTestPassed()
        {
            TestPassed = _result.RunTestResult == RunResult.TestPassed;
        }

        private void UpdateTestFailing()
        {
            TestFailing = _result.RunTestResult == RunResult.TestFailed;
        }

        private void UpdateTestInvalid()
        {
            TestInvalid = _result.RunTestResult == RunResult.TestInvalid ||
                          _result.RunTestResult == RunResult.TestResourceDeleted ||
                          _result.RunTestResult == RunResult.TestResourcePathUpdated;
        }

        private void UpdateTestPending()
        {
            TestPending = _result.RunTestResult != RunResult.TestFailed &&
                          _result.RunTestResult != RunResult.TestPassed &&
                          _result.RunTestResult != RunResult.TestInvalid &&
                          _result.RunTestResult != RunResult.TestResourceDeleted &&
                          _result.RunTestResult != RunResult.TestResourcePathUpdated;
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

        public void OnSearchTypeChanged()
        {
            UpdateMatchVisibility(_assertOp, _findRecsetOptions ?? FindRecsetOptions.FindAllDecision());

            var requiresCriteria = _requiresSearchCriteria.Contains(_assertOp);
            IsSearchCriteriaEnabled = requiresCriteria;
            if (!requiresCriteria)
            {
                Value = string.Empty;
            }
        }

        public void UpdateMatchVisibility(string value, IList<IFindRecsetOptions> whereOptions)
        {

            var opt = whereOptions.FirstOrDefault(a => value != null && value.ToLower().StartsWith(a.HandlesType().ToLower()));
            if (opt != null)
            {
                switch (opt.ArgumentCount)
                {
                    case 1:
                        IsSearchCriteriaVisible = false;
                        IsBetweenCriteriaVisible = false;
                        IsSinglematchCriteriaVisible = false;
                        break;
                    case 2:
                        IsSearchCriteriaVisible = true;
                        IsBetweenCriteriaVisible = false;
                        IsSinglematchCriteriaVisible = true;
                        break;
                    case 3:
                        IsSearchCriteriaVisible = true;
                        IsBetweenCriteriaVisible = true;
                        IsSinglematchCriteriaVisible = false;
                        break;

                }
            }
        }

        public ObservableCollection<string> AssertOps
        {
            get { return _assertOps; }
            set
            {
                _assertOps = value;
                OnPropertyChanged(() => AssertOps);
            }
        }

        public bool CanEditVariable
        {
            get { return _canEditVariable; }
            set
            {
                _canEditVariable = value; 
                OnPropertyChanged(() => CanEditVariable);
            }
        }

        [JsonIgnore]
        public Action AddNewAction { get; set; }
        [JsonIgnore]
        public Action<string> AddStepOutputRow { get; set; }

       
    }
}
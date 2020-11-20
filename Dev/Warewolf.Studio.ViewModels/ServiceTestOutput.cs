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
using Dev2.DataList;
using Dev2.DataList.Contract;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;

namespace Warewolf.Studio.ViewModels
{
    public class ServiceTestOutput : BindableBase, IServiceTestOutput
    {
        string _variable;
        string _value;
        string _assertOp;
        ObservableCollection<string> _assertOps;
        bool _hasOptionsForValue;
        bool _isSingleMatchCriteriaVisible;
        bool _isBetweenCriteriaVisible;
        bool _isSearchCriteriaEnabled;
        bool _isSearchCriteriaVisible;
        List<string> _optionsForValue;
        string _from;
        string _to;
        readonly IList<string> _requiresSearchCriteria = new List<string>
        {
            "Doesn't Contain",
            "Contains", "=",
            "<> (Not Equal)",
            "Ends With",
            "Doesn't Start With",
            "Doesn't End With",
            "Starts With",
            "Is Regex",
            "Not Regex",
            ">",
            "<",
            "<=",
            ">="
        };
        readonly IList<IFindRecsetOptions> _findRecSetOptions;
        bool _testPassed;
        bool _testPending;
        bool _testInvalid;
        bool _testFailing;
        TestRunResult _result;
        bool _canEditVariable;

        public ServiceTestOutput()
        {
            
        }

        public ServiceTestOutput(string variable, string value, string from, string to)
        {
            Variable = variable ?? throw new ArgumentNullException(nameof(variable));
            Value = value;
            From = from;
            To = to;
            _findRecSetOptions = FindRecsetOptions.FindAllDecision();
            var collection = _findRecSetOptions.Select(c => c.HandlesType());
            AssertOps = new ObservableCollection<string>(collection);
            AssertOp = "=";
            CanEditVariable = true;
            IsSingleMatchCriteriaVisible = true;
            TestPending = true;
        }

        public string Variable
        {
            get => _variable;
            set
            {
                _variable = value;
                OnPropertyChanged(() => Variable);
            }
        }
        public string Value
        {
            get => _value;
            set
            {
                if (string.IsNullOrEmpty(_value) && !string.IsNullOrEmpty(value))
                {
                    InvokeAction();
                }
                _value = value;
                OnPropertyChanged(() => Value);
            }
        }

        public string From
        {
            get => _from;
            set
            {
                _from = value;
                if (!string.IsNullOrEmpty(_from))
                {
                    InvokeAction();
                }
                OnPropertyChanged(() => From);
            }
        }
        public string To
        {
            get => _to;
            set
            {
                _to = value;
                if (!string.IsNullOrEmpty(_to))
                {
                    InvokeAction();
                }
                OnPropertyChanged(() => To);
            }
        }

        private void InvokeAction()
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

        public string AssertOp
        {
            get => _assertOp;
            set
            {
                _assertOp = value;
                OnPropertyChanged(() => AssertOp);
                OnSearchTypeChanged();
            }
        }

        public bool HasOptionsForValue
        {
            get => _hasOptionsForValue;
            set
            {
                _hasOptionsForValue = value;
                OnPropertyChanged(() => HasOptionsForValue);
            }
        }

        public bool IsSingleMatchCriteriaVisible
        {
            get => _isSingleMatchCriteriaVisible;
            set
            {
                _isSingleMatchCriteriaVisible = value;
                OnPropertyChanged(() => IsSingleMatchCriteriaVisible);
            }
        }
        public bool IsBetweenCriteriaVisible
        {
            get => _isBetweenCriteriaVisible;
            set
            {
                _isBetweenCriteriaVisible = value;
                OnPropertyChanged(() => IsBetweenCriteriaVisible);
            }
        }

        public bool IsSearchCriteriaEnabled
        {
            get => _isSearchCriteriaEnabled;
            set
            {
                _isSearchCriteriaEnabled = value;
                OnPropertyChanged(() => IsSearchCriteriaEnabled);
            }
        }

        public bool IsSearchCriteriaVisible
        {
            get => _isSearchCriteriaVisible;
            set
            {
                _isSearchCriteriaVisible = value;
                OnPropertyChanged(() => IsSearchCriteriaVisible);
            }
        }

        public List<string> OptionsForValue
        {
            get => _optionsForValue;
            set
            {
                _optionsForValue = value;
                OnPropertyChanged(() => OptionsForValue);
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
                    UpdateTestFailing();
                    UpdateTestInvalid();
                    UpdateTestPending();
                }

                OnPropertyChanged(() => Result);
            }
        }

        void UpdateTestPassed()
        {
            TestPassed = _result.RunTestResult == RunResult.TestPassed;
        }

        void UpdateTestFailing()
        {
            TestFailing = _result.RunTestResult == RunResult.TestFailed;
        }

        void UpdateTestInvalid()
        {
            TestInvalid = _result.RunTestResult == RunResult.TestInvalid ||
                          _result.RunTestResult == RunResult.TestResourceDeleted ||
                          _result.RunTestResult == RunResult.TestResourcePathUpdated;
        }

        void UpdateTestPending()
        {
            var testPending = _result.RunTestResult != RunResult.TestFailed;
            testPending &= _result.RunTestResult != RunResult.TestPassed;
            testPending &= _result.RunTestResult != RunResult.TestInvalid;
            testPending &= _result.RunTestResult != RunResult.TestResourceDeleted;
            testPending &= _result.RunTestResult != RunResult.TestResourcePathUpdated;

            TestPending = testPending;
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

        public void OnSearchTypeChanged()
        {
            UpdateMatchVisibility(_assertOp, _findRecSetOptions ?? FindRecsetOptions.FindAllDecision());

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
                        IsSingleMatchCriteriaVisible = false;
                        break;
                    case 2:
                        IsSearchCriteriaVisible = true;
                        IsBetweenCriteriaVisible = false;
                        IsSingleMatchCriteriaVisible = true;
                        break;
                    case 3:
                        IsSearchCriteriaVisible = true;
                        IsBetweenCriteriaVisible = true;
                        IsSingleMatchCriteriaVisible = false;
                        break;
                    default:
                        break;
                }
            }
        }

        public ObservableCollection<string> AssertOps
        {
            get => _assertOps;
            set
            {
                _assertOps = value;
                OnPropertyChanged(() => AssertOps);
            }
        }

        public bool CanEditVariable
        {
            get => _canEditVariable;
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
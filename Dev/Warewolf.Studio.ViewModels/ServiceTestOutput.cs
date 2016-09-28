using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Microsoft.Practices.Prism.Mvvm;

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

        public ServiceTestOutput(string variable, string value, string from, string to)
        {
            if(variable == null)
                throw new ArgumentNullException(nameof(variable));
            Variable = variable;
            Value = value;
            From = from;
            To = to;
            _findRecsetOptions = FindRecsetOptions.FindAllDecision();
            var collection = _findRecsetOptions.Select(c => c.HandlesType());
            AssertOps = new ObservableCollection<string>(collection);
            AssertOp = "=";
            IsSinglematchCriteriaVisible = true;
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
                OnPropertyChanged(()=>Variable);
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

        public void OnSearchTypeChanged()
        {
            UpdateMatchVisibility(_assertOp, _findRecsetOptions);
            var requiresCriteria = _requiresSearchCriteria.Contains(_assertOp);
            IsSearchCriteriaEnabled = requiresCriteria;
            if (!requiresCriteria)
            {
                Value = string.Empty;
            }
        }

        public void UpdateMatchVisibility( string value, IList<IFindRecsetOptions> whereOptions)
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
    }
}
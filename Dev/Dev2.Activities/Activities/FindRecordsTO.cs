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

using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.TO;
using Dev2.Util;
using Dev2.Utilities;
using Dev2.Validation;
using Dev2.Common;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class FindRecordsTO : ValidatedObject, IDev2TOFn
    {
        int _indexNum;
        string _searchType;
        bool _isSearchCriteriaEnabled;
        bool _isSearchCriteriaFocused;
        bool _isSearchTypeFocused;
        string _searchCriteria;
        string _from;
        string _to;
        bool _isSearchCriteriaVisible;
        bool _isFromFocused;
        bool _isToFocused;

        public FindRecordsTO()
            : this("Match On", "Equal", 0)
        {
        }

        public IList<string> WhereOptionList { get; set; }

        public FindRecordsTO(string searchCriteria, string searchType, int indexNum)
            : this(searchCriteria, searchType, indexNum, false, "", "")
        {
        }

        public FindRecordsTO(string searchCriteria, string searchType, int indexNum, bool inserted)
            : this(searchCriteria, searchType, indexNum, inserted, "", "")
        {
        }

        public FindRecordsTO(string searchCriteria, string searchType, int indexNum, bool inserted, string from, string to)
        {
            Inserted = inserted;
            SearchCriteria = searchCriteria;
            SearchType = searchType;
            IndexNumber = indexNum;
            IsSearchCriteriaEnabled = false;
            IsSearchCriteriaVisible = true;
            From = from;
            To = to;
            IsSearchTypeFocused = false;
        }

        [FindMissing]
        public string From
        {
            get => _from;
            set
            {
                _from = value;
                OnPropertyChanged();
                RaiseCanAddRemoveChanged();
            }
        }

        public bool IsFromFocused { get => _isFromFocused; set => OnPropertyChanged(ref _isFromFocused, value); }

        [FindMissing]
        public string To
        {
            get => _to;
            set
            {
                _to = value;
                OnPropertyChanged();
                RaiseCanAddRemoveChanged();
            }
        }

        public bool IsToFocused { get => _isToFocused; set => OnPropertyChanged(ref _isToFocused, value); }

        [FindMissing]
        public string SearchCriteria
        {
            get => _searchCriteria;
            set
            {
                _searchCriteria = value;
                OnPropertyChanged();
                RaiseCanAddRemoveChanged();
            }
        }

        public bool IsSearchCriteriaFocused { get => _isSearchCriteriaFocused; set => OnPropertyChanged(ref _isSearchCriteriaFocused, value); }

        public string SearchType
        {
            get => _searchType;
            set
            {
                if (value != null)
                {
                    _searchType = FindRecordsDisplayUtil.ConvertForDisplay(value);
                    OnPropertyChanged();
                    RaiseCanAddRemoveChanged();
                }
            }
        }

        public bool IsSearchTypeFocused { get => _isSearchTypeFocused; set => OnPropertyChanged(ref _isSearchTypeFocused, value); }

        void RaiseCanAddRemoveChanged()
        {
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
        }

        public bool IsSearchCriteriaEnabled
        {
            get => _isSearchCriteriaEnabled;
            set
            {
                _isSearchCriteriaEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsSearchCriteriaVisible
        {
            get => _isSearchCriteriaVisible;
            set
            {
                _isSearchCriteriaVisible = value;
                OnPropertyChanged(ref _isSearchCriteriaVisible, value);
            }
        }

        public int IndexNumber
        {
            get => _indexNum;
            set
            {
                _indexNum = value;
                OnPropertyChanged();
            }
        }

        public bool CanRemove()
        {
            if (string.IsNullOrEmpty(SearchCriteria) && string.IsNullOrEmpty(SearchType))
            {
                return true;
            }
            return false;
        }

        public bool CanAdd() => !string.IsNullOrEmpty(SearchType);

        public void ClearRow()
        {
            SearchCriteria = string.Empty;
            SearchType = "";
        }

        public bool Inserted { get; set; }

        public bool IsEmpty() => string.IsNullOrEmpty(SearchType) && string.IsNullOrEmpty(SearchCriteria);

#pragma warning disable S1541 // Methods and properties should not be too complex
        public override IRuleSet GetRuleSet(string propertyName, string datalist)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            var ruleSet = new RuleSet();
            if (IsEmpty())
            {
                return ruleSet;
            }
            switch (propertyName)
            {
                case "SearchType":
                    if (SearchType == "Starts With" || SearchType == "Ends With" || SearchType == "Doesn't Start With" || SearchType == "Doesn't End With")
                    {
                        ruleSet.Add(new IsStringEmptyRule(() => SearchType));
                        ruleSet.Add(new IsValidExpressionRule(() => SearchType, datalist, "1", new VariableUtils()));
                    }
                    break;
                case "From":
                    if (SearchType == "Is Between" || SearchType == "Is Not Between")
                    {
                        ruleSet.Add(new IsStringEmptyRule(() => From));
                        ruleSet.Add(new IsValidExpressionRule(() => From, datalist, "1", new VariableUtils()));
                    }
                    break;
                case "To":
                    if (SearchType == "Is Between" || SearchType == "Is Not Between")
                    {
                        ruleSet.Add(new IsStringEmptyRule(() => To));
                        ruleSet.Add(new IsValidExpressionRule(() => To, datalist, "1", new VariableUtils()));
                    }
                    break;
                case "SearchCriteria":

                    ruleSet.Add(new IsValidExpressionRule(() => SearchCriteria, datalist, "1", new VariableUtils()));
                    break;
                default:
                    Dev2Logger.Info("No Rule Set for the Find Records TO Property Name: " + propertyName, GlobalConstants.WarewolfInfo);
                    break;
            }

            return ruleSet;
        }
    }
}

using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;
using Dev2.Utilities;
using Dev2.Validation;
using System;

namespace Dev2.TO
{
    public class SharepointSearchTo : ValidatedObject, IDev2TOFn
    {
        int _indexNum;
        string _searchType;
        bool _isSearchCriteriaEnabled;
        bool _isSearchCriteriaFocused;
        bool _isSearchTypeFocused;
        string _valueToMatch;
        string _from;
        string _to;
        bool _isSearchCriteriaVisible;
        bool _isFromFocused;
        bool _isToFocused;
        string _fieldName;
        string _internalName;

        public SharepointSearchTo()
            : this("Field Name", "Equal","", 0)
        {
        }

        public SharepointSearchTo(string fieldName,string searchType, string valueToMatch, int indexNum, bool inserted = false, string from = "", string to = "")
        {
            FieldName = fieldName;
            Inserted = inserted;
            ValueToMatch = valueToMatch;
            SearchType = searchType;
            IndexNumber = indexNum;
            IsSearchCriteriaEnabled = false;
            IsSearchCriteriaVisible = true;
            From = @from;
            To = to;
            IsSearchTypeFocused = false;
        }

        [FindMissing]
        public string From
        {
            get
            {
                return _from;
            }
            set
            {
                _from = value;
                OnPropertyChanged();
                RaiseCanAddRemoveChanged();
            }
        }

        public bool IsFromFocused { get { return _isFromFocused; } set { OnPropertyChanged(ref _isFromFocused, value); } }

        [FindMissing]
        public string To
        {
            get
            {
                return _to;
            }
            set
            {
                _to = value;
                OnPropertyChanged();
                RaiseCanAddRemoveChanged();
            }
        }



        public bool IsToFocused { get { return _isToFocused; } set { OnPropertyChanged(ref _isToFocused, value); } }

        [FindMissing]
        public string ValueToMatch
        {
            get
            {
                return _valueToMatch;
            }
            set
            {
                _valueToMatch = value;
                OnPropertyChanged();
                RaiseCanAddRemoveChanged();
            }
        }

        public bool IsSearchCriteriaFocused { get { return _isSearchCriteriaFocused; } set { OnPropertyChanged(ref _isSearchCriteriaFocused, value); } }

        public string SearchType
        {
            get
            {
                return _searchType;
            }
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

        public bool IsSearchTypeFocused { get { return _isSearchTypeFocused; } set { OnPropertyChanged(ref _isSearchTypeFocused, value); } }

        void RaiseCanAddRemoveChanged()
        {
            
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
            
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
                OnPropertyChanged();
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
                OnPropertyChanged(ref _isSearchCriteriaVisible, value);
            }
        }

        public int IndexNumber
        {
            get
            {
                return _indexNum;
            }
            set
            {
                _indexNum = value;
                OnPropertyChanged();
            }
        }

        public bool CanRemove()
        {
            return string.IsNullOrEmpty(FieldName);
        }

        public bool CanAdd()
        {
            return !string.IsNullOrEmpty(SearchType) && !string.IsNullOrEmpty(FieldName);
        }

        public void ClearRow()
        {
            ValueToMatch = string.Empty;
            SearchType = "";
        }

        public string FieldName
        {
            get
            {
                return _fieldName;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _fieldName = value;
                OnPropertyChanged();
                RaiseCanAddRemoveChanged();
            }
        }

        public string InternalName
        {
            get
            {
                return _internalName;
            }
            set
            {
                if(value==null)
                {
                    return;
                }

                _internalName = value;
                OnPropertyChanged();
                RaiseCanAddRemoveChanged();
            }
        }
        public bool Inserted { get; set; }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(SearchType) && string.IsNullOrEmpty(ValueToMatch);
        }

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            RuleSet ruleSet = new RuleSet();
            if (IsEmpty())
            {
                return ruleSet;
            }
            switch (propertyName)
            {
                case "FieldName":
                    if (FieldName.Length == 0)
                    {
                        ruleSet.Add(new IsStringEmptyRule(() => FieldName));
                    }
                    break;
                case "ValueToMatch":
                    if (ValueToMatch.Length == 0)
                    {
                        ruleSet.Add(new IsStringEmptyRule(() => ValueToMatch));
                    }
                    ruleSet.Add(new IsValidExpressionRule(() => ValueToMatch, datalist, "1"));
                    break;
                default:
                    break;
            }

            return ruleSet;
        }
    }

    public static class SharepointSearchOptions
    {
        public static List<string> SearchOptions()
        {
            var searchOptions = new List<string>
            {
                "Begins With",
                "In",
                "Contains",
                "=",
                ">",
                ">=",
                "<",
                "<=",
                "<>"
            };
            return searchOptions;
        }

        public static string GetStartTagForSearchOption(string searchOption)
        {
            switch (searchOption)
            {
                case "Begins With":
                    return "<BeginsWith>";
                case "In":
                    return "<In>";
                case "Contains":
                    return "<Contains>";
                case "=":
                    return "<Eq>";
                case ">":
                    return "<Gt>";
                case ">=":
                    return "<Geq>";
                case "<":
                    return "<Lt>";
                case "<=":
                    return "<Leq>";
                case "<>":
                    return "<Neq>";
                default:
                    return null;
            }
        }

        public static string GetEndTagForSearchOption(string searchOption)
        {
            switch (searchOption)
            {
                case "Begins With":
                    return "</BeginsWith>";
                case "In":
                    return "</In>";
                case "Contains":
                    return "</Contains>";
                case "=":
                    return "</Eq>";
                case ">":
                    return "</Gt>";
                case ">=":
                    return "</Geq>";
                case "<":
                    return "</Lt>";
                case "<=":
                    return "</Leq>";
                case "<>":
                    return "</Neq>";
                default:
                    return null;
            }
        }
    }
}
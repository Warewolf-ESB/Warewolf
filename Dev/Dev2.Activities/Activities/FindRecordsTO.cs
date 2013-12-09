using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;
using Dev2.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class FindRecordsTO : IDev2TOFn, IPerformsValidation
    {
        int _indexNum;
        string _searchType;
        bool _isSearchCriteriaEnabled;

        string _searchCriteria;
        string _from;
        string _to;
        bool _isSearchCriteriaVisible;
        Dictionary<string, List<IActionableErrorInfo>> _errors = new Dictionary<string, List<IActionableErrorInfo>>();
        

        public FindRecordsTO()
            : this("Match On", "Equal", 0)
        {
        }

        // TODO: Remove WhereOptionList property - DO NOT USE FOR BINDING, USE VIEWMODEL PROPERTY INSTEAD!
        public IList<string> WhereOptionList { get; set; }
        public FindRecordsTO(string searchCriteria, string searchType, int indexNum, bool include = false, bool inserted = false,string from = "",string to = "")
        {
            Inserted = inserted;
            SearchCriteria = searchCriteria;
            SearchType = searchType;
            IndexNumber = indexNum;
            IsSearchCriteriaEnabled = false;
            IsSearchCriteriaVisible = true;
            From = from;
            To = to;
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
                OnPropertyChanged("From");
                RaiseCanAddRemoveChanged();
            }
        }

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
                OnPropertyChanged("To");
                RaiseCanAddRemoveChanged();
            }
        }

        [FindMissing]
        public string SearchCriteria
        {
            get
            {
                return _searchCriteria;
            }
            set
            {
                _searchCriteria = value;
                OnPropertyChanged("SearchCriteria");
                RaiseCanAddRemoveChanged();
            }
        }

        public string SearchType
        {
            get
            {
                return _searchType;
            }
            set
            {
                _searchType = FindRecordsDisplayUtil.ConvertForDisplay(value);
                OnPropertyChanged("SearchType");
                RaiseCanAddRemoveChanged();
            }
        }

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
                OnPropertyChanged("IsSearchCriteriaEnabled");
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
                OnPropertyChanged("IsSearchCriteriaVisible");
            }
        }

        #region Implementation of INotifyPropertyChanged

        public int IndexNumber
        {
            get
            {
                return _indexNum;
            }
            set
            {
                _indexNum = value;
                OnPropertyChanged("IndexNumber");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool CanRemove()
        {
            if(string.IsNullOrEmpty(SearchCriteria) && string.IsNullOrEmpty(SearchType))
            {
                return true;
            }
            return false;
        }

        public bool CanAdd()
        {
            return !string.IsNullOrEmpty(SearchType);            
        }

        public void ClearRow()
        {
            SearchCriteria = string.Empty;
            SearchType = "";
        }

        public bool Inserted { get; set; }

        #endregion

        #region Implementation of IDataErrorInfo

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get. </param>
        public string this[string columnName] { get { return null; } }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        /// An error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        public string Error { get; private set; }

        #endregion

        #region Implementation of IPerformsValidation

        public Dictionary<string, List<IActionableErrorInfo>> Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                _errors = value;
                OnPropertyChanged("Errors");
            }
        }

        public bool Validate(string propertyName, RuleSet ruleSet)
        {
            // TODO: Implement Validate(string propertyName, RuleSet ruleSet) - see ActivityDTO
            return true;
        }

        public bool Validate(string propertyName)
        {
            // TODO: Implement Validate(string propertyName) - see ActivityDTO
            return Validate(propertyName, new RuleSet());
        }

        #endregion
    }
}
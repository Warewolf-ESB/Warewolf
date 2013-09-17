using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dev2.DataList;
using Dev2.Interfaces;
using Dev2.Util;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class FindRecordsTO : INotifyPropertyChanged, IDev2TOFn
    {
        int _indexNum;
        string _searchType;
        bool _isSearchCriteriaEnabled;
        [NonSerialized]
        readonly IList<string> _requiresSearchCriteria = new List<string> { "Not Contains" ,"Contains" ,"Equal" ,"Not Equal" ,"Ends With" ,"Starts With" ,"Regex" ,">" ,"<","<=",">=" };

        string _searchCriteria;

        public FindRecordsTO()
            : this("Match On", "Equal", 0)
        {
        }
        public IList<string> WhereOptionList { get; set; }
        public FindRecordsTO(string searchCriteria, string searchType, int indexNum, bool include = false, bool inserted = false)
        {
            Inserted = inserted;
            SearchCriteria = searchCriteria;
            SearchType = searchType;
            IndexNumber = indexNum;
            IsSearchCriteriaEnabled = false;
            WhereOptionList = FindRecsetOptions.FindAll().Select(c => c.HandlesType()).OrderBy(c => c).ToList();
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
                _searchType = value;
                OnPropertyChanged("SearchType");
                UpdateIsCriteriaEnabled();
            }
        }

        void UpdateIsCriteriaEnabled()
        {
            if(_requiresSearchCriteria.Contains(SearchType))
            {
                IsSearchCriteriaEnabled = true;
            }
            else
            {
                IsSearchCriteriaEnabled = false;
                SearchCriteria = string.Empty;
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
                OnPropertyChanged("IsSearchCriteriaEnabled");
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
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        public bool CanAdd()
        {
            var result = !string.IsNullOrEmpty(SearchType);
            return result;
        }

        public void ClearRow()
        {
            SearchCriteria = string.Empty;
            SearchType = "";
        }

        public bool Inserted { get; set; }

        #endregion
    }
}
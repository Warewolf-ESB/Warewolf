using Dev2.Common;
using Dev2.Common.Interfaces.Search;
using Dev2.Common.Utils;
using Dev2.Runtime.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dev2.Runtime.Search
{
    public class ActivitySearcher : ISearcher
    {
        private readonly IResourceCatalog _resourceCatalog;

        public ActivitySearcher(IResourceCatalog resourceCatalog)
        {
            _resourceCatalog = resourceCatalog;
        }
        public List<ISearchResult> GetSearchResults(ISearchValue searchParameters)
        {
            var searchResults = new List<ISearchResult>();
            if (searchParameters.SearchOptions.IsToolTitleSelected)
            {
                foreach (var resourceActivity in _resourceCatalog.GetResourceActivityCache(GlobalConstants.ServerWorkspaceID).Cache)
                {
                    var activity = resourceActivity.Value;
                    var nextNodes = new List<IDev2Activity> { activity };
                    var allNodes = nextNodes.Flatten(act => act.GetNextNodes()).Flatten(act => act.GetChildrenNodes());
                    foreach (var next in allNodes)
                    {
                        PerformSearchOnActivity(searchParameters, searchResults, resourceActivity, next);
                    }
                }
            }
            return searchResults;
        }

        private void PerformSearchOnActivity(ISearchValue searchParameters, List<ISearchResult> searchResults, KeyValuePair<Guid, IDev2Activity> resourceActivity, IDev2Activity activity)
        {
            if (activity != null)
            {
                var foundMatch = SearchUtils.FilterText(activity.GetDisplayName(), searchParameters);
                if (foundMatch)
                {
                    var resource = _resourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceActivity.Key);
                    var searchResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), SearchItemType.WorkflowName, activity.GetDisplayName());
                    searchResults.Add(searchResult);
                }
            }
        }
    }

    public class SearchValue : ISearchValue
    {
        public SearchValue()
        {
            SearchInput = string.Empty;
            SearchOptions = new SearchOptions();
        }
        public string SearchInput { get; set; }
        public ISearchOptions SearchOptions { get; set; }

        public List<ISearchResult> GetSearchResults(List<ISearcher> searchers)
        {
            var searchResults = new List<ISearchResult>();  
            foreach(var searcher in searchers)
            {
                searchResults.AddRange(searcher.GetSearchResults(this));
            }
            return searchResults;
        }
    }

    public class SearchResult : ISearchResult
    {
        public SearchResult()
        {
        }

        public SearchResult(Guid resourceId, string name, string path, SearchItemType type, string match)
        {
            ResourceId = resourceId;
            Name = name;
            Path = path;
            Type = type;
            Match = match;
        }

        public Guid ResourceId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public SearchItemType Type { get; set; }
        public string Match { get; set; }
    }

    public class SearchOptions : ISearchOptions
    {
        bool _isAllSelected;
        bool _isWorkflowNameSelected;
        bool _isToolTitleSelected;
        bool _isScalarNameSelected;
        bool _isObjectNameSelected;
        bool _isRecSetNameSelected;
        bool _isInputVariableSelected;
        bool _isOutputVariableSelected;
        bool _isTestNameSelected;
        bool _isMatchCaseSelected;
        bool _isMatchWholeWordSelected;

        public SearchOptions()
        {
            IsAllSelected = true;
        }

        private bool IsManualUpdate
        {
            get
            {
                bool isChecked = IsAllChecked();
                bool isUnChecked = IsAllUnChecked();
                return isChecked || isUnChecked;
            }
        }

        private bool IsAllChecked()
        {
            var isChecked = IsWorkflowNameSelected;
            isChecked &= IsToolTitleSelected;
            isChecked &= IsScalarNameSelected;
            isChecked &= IsObjectNameSelected;
            isChecked &= IsRecSetNameSelected;
            isChecked &= IsInputVariableSelected;
            isChecked &= IsOutputVariableSelected;
            isChecked &= IsTestNameSelected;
            return isChecked;
        }

        private bool IsAllUnChecked()
        {
            var isUnChecked = !IsWorkflowNameSelected;
            isUnChecked &= !IsToolTitleSelected;
            isUnChecked &= !IsScalarNameSelected;
            isUnChecked &= !IsObjectNameSelected;
            isUnChecked &= !IsRecSetNameSelected;
            isUnChecked &= !IsInputVariableSelected;
            isUnChecked &= !IsOutputVariableSelected;
            isUnChecked &= !IsTestNameSelected;
            return isUnChecked;
        }

        public bool IsAllSelected
        {
            get => _isAllSelected;
            set
            {
                _isAllSelected = value;
                if (IsManualUpdate)
                {
                    UpdateAllStates(value);
                }
                OnPropertyChanged();
            }
        }

        private void UpdateAllStates(bool value)
        {
            IsWorkflowNameSelected = value;
            IsToolTitleSelected = value;
            IsScalarNameSelected = value;
            IsObjectNameSelected = value;
            IsRecSetNameSelected = value;
            IsInputVariableSelected = value;
            IsOutputVariableSelected = value;
            IsTestNameSelected = value;
        }

        public bool IsWorkflowNameSelected
        {
            get => _isWorkflowNameSelected;
            set
            {
                _isWorkflowNameSelected = value;
                OnPropertyChanged();
            }
        }
        public bool IsTestNameSelected
        {
            get => _isTestNameSelected;
            set
            {
                _isTestNameSelected = value;
                OnPropertyChanged();
            }
        }
        public bool IsScalarNameSelected
        {
            get => _isScalarNameSelected;
            set
            {
                _isScalarNameSelected = value;
                OnPropertyChanged();
            }
        }
        public bool IsObjectNameSelected
        {
            get => _isObjectNameSelected;
            set
            {
                _isObjectNameSelected = value;
                OnPropertyChanged();
            }
        }
        public bool IsRecSetNameSelected
        {
            get => _isRecSetNameSelected;
            set
            {
                _isRecSetNameSelected = value;
                OnPropertyChanged();
            }
        }
        public bool IsToolTitleSelected
        {
            get => _isToolTitleSelected;
            set
            {
                _isToolTitleSelected = value;
                OnPropertyChanged();
            }
        }
        public bool IsInputVariableSelected
        {
            get => _isInputVariableSelected;
            set
            {
                _isInputVariableSelected = value;
                OnPropertyChanged();
            }
        }
        public bool IsOutputVariableSelected
        {
            get => _isOutputVariableSelected;
            set
            {
                _isOutputVariableSelected = value;
                OnPropertyChanged();
            }
        }
        public bool IsMatchCaseSelected
        {
            get => _isMatchCaseSelected;
            set
            {
                _isMatchCaseSelected = value;
                OnPropertyChanged();
            }
        }
        public bool IsMatchWholeWordSelected
        {
            get => _isMatchWholeWordSelected;
            set
            {
                _isMatchWholeWordSelected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

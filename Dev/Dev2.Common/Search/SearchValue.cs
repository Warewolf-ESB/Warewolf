using Dev2.Common.Interfaces.Search;
using System;

namespace Dev2.Common.Search
{
    public class SearchValue : ISearchValue
    {
        public string SearchInput { get; set; }
        public ISearchOptions SearchOptions { get; set; }
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

        public bool IsAllSelected
        {
            get => _isAllSelected;
            set
            {
                _isAllSelected = value;

                IsWorkflowNameSelected = value;
                IsToolTitleSelected = value;
                IsScalarNameSelected = value;
                IsObjectNameSelected = value;
                IsRecSetNameSelected = value;
                IsInputVariableSelected = value;
                IsOutputVariableSelected = value;
                IsTestNameSelected = value;
            }
        }
        public bool IsWorkflowNameSelected
        {
            get => _isWorkflowNameSelected;
            set => _isWorkflowNameSelected = value;
        }
        public bool IsTestNameSelected
        {
            get => _isTestNameSelected;
            set => _isTestNameSelected = value;
        }
        public bool IsScalarNameSelected
        {
            get => _isScalarNameSelected;
            set => _isScalarNameSelected = value;
        }
        public bool IsObjectNameSelected
        {
            get => _isObjectNameSelected;
            set => _isObjectNameSelected = value;
        }
        public bool IsRecSetNameSelected
        {
            get => _isRecSetNameSelected;
            set => _isRecSetNameSelected = value;
        }
        public bool IsToolTitleSelected
        {
            get => _isToolTitleSelected;
            set => _isToolTitleSelected = value;
        }
        public bool IsInputVariableSelected
        {
            get => _isInputVariableSelected;
            set => _isInputVariableSelected = value;
        }
        public bool IsOutputVariableSelected
        {
            get => _isOutputVariableSelected;
            set => _isOutputVariableSelected = value;
        }
        public bool IsMatchCaseSelected
        {
            get => _isMatchCaseSelected;
            set => _isMatchCaseSelected = value;
        }
        public bool IsMatchWholeWordSelected
        {
            get => _isMatchWholeWordSelected;
            set => _isMatchWholeWordSelected = value;
        }
    }
}

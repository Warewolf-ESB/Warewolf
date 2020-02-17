/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Dev2.Common.Interfaces.Search;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Warewolf;

namespace Dev2.Common.Search
{
    public class Search : BindableBase, ISearch
    {
        string _searchInput;
        public Search()
        {
            SearchInput = string.Empty;
            SearchOptions = new SearchOptions();
        }
        public string SearchInput
        {
            get { return _searchInput; }
            set => SetProperty(ref _searchInput, value);
        }
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

    public class SearchResult : ISearchResult, IEquatable<ISearchResult>
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

        public bool Equals(ISearchResult other)
        {
            if (other == null)
            {
                return false;
            }
            var equals = other.Match == Match;
            equals &= other.ResourceId == ResourceId;
            equals &= other.Name == Name;
            equals &= other.Path == Path;
            equals &= other.Type == Type;
            return equals;
        }

        public override bool Equals(object obj)
        {
            if (obj is ISearchResult searchResult)
            {
                return Equals(searchResult);
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 271001031;
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(ResourceId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Match);
            return hashCode;
        }
    }

    public class SearchOptions : BindableBase, ISearchOptions
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
            IsAllSelected = false;
            UpdateAllStates(false);
        }

        public bool IsAllSelected
        {
            get => _isAllSelected;
            set => SetProperty(ref _isAllSelected, value);
        }
        public bool IsWorkflowNameSelected
        {
            get => _isWorkflowNameSelected;
            set => SetProperty(ref _isWorkflowNameSelected, value);
        }
        public bool IsTestNameSelected
        {
            get => _isTestNameSelected;
            set => SetProperty(ref _isTestNameSelected, value);
        }
        public bool IsScalarNameSelected
        {
            get => _isScalarNameSelected;
            set => SetProperty(ref _isScalarNameSelected, value);
        }
        public bool IsObjectNameSelected
        {
            get => _isObjectNameSelected;
            set => SetProperty(ref _isObjectNameSelected, value);
        }
        public bool IsRecSetNameSelected
        {
            get => _isRecSetNameSelected;
            set => SetProperty(ref _isRecSetNameSelected, value);
        }
        public bool IsToolTitleSelected
        {
            get => _isToolTitleSelected;
            set => SetProperty(ref _isToolTitleSelected, value);
        }
        public bool IsInputVariableSelected
        {
            get => _isInputVariableSelected;
            set => SetProperty(ref _isInputVariableSelected, value);
        }
        public bool IsOutputVariableSelected
        {
            get => _isOutputVariableSelected;
            set => SetProperty(ref _isOutputVariableSelected, value);
        }

        public bool IsVariableSelected => GetAnyVariableSelected();

        private bool GetAnyVariableSelected()
        {
            var isSelected = IsScalarNameSelected;
            isSelected |= IsRecSetNameSelected;
            isSelected |= IsObjectNameSelected;
            isSelected |= IsInputVariableSelected;
            isSelected |= IsOutputVariableSelected;
            return isSelected;
        }

        public void UpdateAllStates(bool value)
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

        public bool IsMatchCaseSelected
        {
            get => _isMatchCaseSelected;
            set => SetProperty(ref _isMatchCaseSelected, value);
        }
        public bool IsMatchWholeWordSelected
        {
            get => _isMatchWholeWordSelected;
            set => SetProperty(ref _isMatchWholeWordSelected, value);
        }
    }
}

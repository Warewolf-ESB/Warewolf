using Dev2.Common;
using Dev2.Common.Interfaces.Search;
using Dev2.Common.Search;
using Dev2.Common.Utils;
using Dev2.Runtime.Interfaces;
using System;
using System.Collections.Generic;

namespace Dev2.Runtime.Search
{
    public class ActivitySearcher : ISearcher
    {
        private readonly IResourceCatalog _resourceCatalog;

        public ActivitySearcher(IResourceCatalog resourceCatalog)
        {
            _resourceCatalog = resourceCatalog ?? throw new ArgumentNullException(nameof(resourceCatalog));
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
                    var searchResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), SearchItemType.ToolTitle, activity.GetDisplayName());
                    searchResults.Add(searchResult);
                }
            }
        }
    }
}

using Dev2.Common;
using Dev2.Common.Interfaces.Search;
using Dev2.Common.Search;
using Dev2.Common.Utils;
using Dev2.Runtime.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Runtime.Search
{
    public class ResourceSearcher : ISearcher
    {
        private readonly IResourceCatalog _resourceCatalog;

        public ResourceSearcher(IResourceCatalog resourceCatalog)
        {
            _resourceCatalog = resourceCatalog ?? throw new ArgumentNullException(nameof(resourceCatalog));
        }

        public List<ISearchResult> GetSearchResults(ISearch searchParameters)
        {
            var searchResults = new List<ISearchResult>();

            if (searchParameters.SearchOptions.IsWorkflowNameSelected)
            {
                var allResources = _resourceCatalog.GetResources(GlobalConstants.ServerWorkspaceID).Where(res => res.ResourceType != "ReservedService");
                foreach (var resource in allResources)
                {
                    if(SearchUtils.FilterText(resource.ResourceName, searchParameters))
                    {
                        var searchItemType = resource.IsSource ? SearchItemType.SourceName : SearchItemType.WorkflowName;
                        var searchResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), searchItemType, resource.ResourceName);
                        searchResults.Add(searchResult);
                    }
                }
            }
            return searchResults;
        }
    }
}

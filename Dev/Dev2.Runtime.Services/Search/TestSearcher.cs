using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Search;
using Dev2.Common.Search;
using Dev2.Common.Utils;
using Dev2.Runtime.Interfaces;
using System;
using System.Collections.Generic;

namespace Dev2.Runtime.Search
{

    public class TestSearcher : ISearcher
    {
        private readonly IResourceCatalog _resourceCatalog;
        private readonly ITestCatalog _testCatalog;

        public TestSearcher(IResourceCatalog resourceCatalog, ITestCatalog testCatalog)
        {
            _resourceCatalog = resourceCatalog ?? throw new ArgumentNullException(nameof(resourceCatalog));
            _testCatalog = testCatalog ?? throw new ArgumentNullException(nameof(testCatalog));
        }

        public List<ISearchResult> GetSearchResults(ISearch searchParameters)
        {
            var foundItems = new List<ISearchResult>();

            if (searchParameters.SearchOptions.IsTestNameSelected)
            {
                var tests = _testCatalog.FetchAllTests();
                foreach (var test in tests)
                {
                    var found = SearchUtils.FilterText(test.TestName, searchParameters);
                    if (found)
                    {
                        var resource = _resourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, test.ResourceId);

                        if(resource != null)
                        {
                            var searchResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), SearchItemType.TestName, test.TestName);
                            foundItems.Add(searchResult);
                        }
                    }
                }
            }
            return foundItems;
        }
    }
}

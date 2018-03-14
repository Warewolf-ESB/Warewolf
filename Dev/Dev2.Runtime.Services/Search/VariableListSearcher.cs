using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Search;
using Dev2.Common.Search;
using Dev2.Common.Utils;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Util;
using Dev2.Runtime.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Runtime.Search
{
    public class VariableListSearcher : ISearcher
    {
        private readonly IResourceCatalog _resourceCatalog;

        public VariableListSearcher(IResourceCatalog resourceCatalog)
        {           
            _resourceCatalog = resourceCatalog ?? throw new ArgumentNullException(nameof(resourceCatalog));
        }

        public List<ISearchResult> GetSearchResults(ISearchValue searchParameters)
        {
            var searchResults = new List<ISearchResult>();
            var ioType = GetIoType(searchParameters);
            var allResources = _resourceCatalog.GetResources(GlobalConstants.ServerWorkspaceID);
            foreach (var resource in allResources)
            {
                var variableList = resource.DataList.ToString();
                var variableDefinitions = DataListUtil.GenerateDefsFromDataList(variableList, ioType,true);
                GetScalarResults(searchParameters, searchResults, resource, variableDefinitions);
                GetRecordsetResults(searchParameters, searchResults, resource, variableDefinitions);
                GetObjectResults(searchParameters, searchResults, resource, variableDefinitions);

            }
            return searchResults;
        }

        private static void GetObjectResults(ISearchValue searchParameters, List<ISearchResult> searchResults, IResource resource, IList<IDev2Definition> variableDefinitions)
        {
            if (searchParameters.SearchOptions.IsObjectNameSelected)
            {
                var matchingObjects = variableDefinitions.Where(v => v.IsObject && SearchUtils.FilterText(v.Name, searchParameters));
                foreach (var matchingObject in matchingObjects)
                {
                    var searchResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), SearchItemType.Object, matchingObject.Name);
                    searchResults.Add(searchResult);
                }
            }
        }

        private static void GetRecordsetResults(ISearchValue searchParameters, List<ISearchResult> searchResults, IResource resource, IList<IDev2Definition> variableDefinitions)
        {
            if (searchParameters.SearchOptions.IsRecSetNameSelected)
            {
                var matchingRecordsets = variableDefinitions.Where(v => v.IsRecordSet && SearchUtils.FilterText(v.RecordSetName, searchParameters));
                foreach (var recordSet in matchingRecordsets)
                {
                    var searchResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), SearchItemType.RecordSet, recordSet.RecordSetName);
                    searchResults.Add(searchResult);
                }
            }
        }

        private static void GetScalarResults(ISearchValue searchParameters, List<ISearchResult> searchResults, IResource resource, IList<IDev2Definition> variableDefinitions)
        {
            if (searchParameters.SearchOptions.IsScalarNameSelected)
            {
                var matchingScalars = variableDefinitions.Where(v => !v.IsObject && !v.IsRecordSet && SearchUtils.FilterText(v.Name, searchParameters));
                foreach (var scalar in matchingScalars)
                {
                    var searchResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), SearchItemType.Scalar, scalar.Name);
                    searchResults.Add(searchResult);
                }
            }
        }

        private static enDev2ColumnArgumentDirection GetIoType(ISearchValue searchParameters)
        {
            enDev2ColumnArgumentDirection ioType;
            if (searchParameters.SearchOptions.IsInputVariableSelected && searchParameters.SearchOptions.IsOutputVariableSelected)
            {
                ioType = enDev2ColumnArgumentDirection.Both;
            }
            else if (searchParameters.SearchOptions.IsOutputVariableSelected)
            {
                ioType = enDev2ColumnArgumentDirection.Output;
            }
            else if (searchParameters.SearchOptions.IsInputVariableSelected)
            {
                ioType = enDev2ColumnArgumentDirection.Input;
            }
            else
            {
                ioType = enDev2ColumnArgumentDirection.None;
            }

            return ioType;
        }
    }
}

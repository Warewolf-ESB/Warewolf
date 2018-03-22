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

        public List<ISearchResult> GetSearchResults(ISearch searchParameters)
        {
            var searchResults = new List<ISearchResult>();
            if (searchParameters.SearchOptions.IsVariableSelected)
            {
                var ioType = GetIoType(searchParameters);
                var allResources = _resourceCatalog.GetResources(GlobalConstants.ServerWorkspaceID);
                foreach (var resource in allResources)
                {
                    if (resource.DataList != null && resource.DataList.Length > 0)
                    {
                        var variableList = resource.DataList.Replace(GlobalConstants.SerializableResourceQuote, "\"").Replace(GlobalConstants.SerializableResourceSingleQuote, "\'").ToString();
                        var variableDefinitions = DataListUtil.GenerateDefsFromDataList(variableList, ioType, true);
                        GetScalarResults(searchParameters, searchResults, resource, variableDefinitions);
                        GetRecordsetResults(searchParameters, searchResults, resource, variableDefinitions);
                        GetObjectResults(searchParameters, searchResults, resource, variableDefinitions);
                    }
                }
            }
            return searchResults;
        }

        private static void GetObjectResults(ISearch searchParameters, List<ISearchResult> searchResults, IResource resource, IList<IDev2Definition> variableDefinitions)
        {
            if (searchParameters.SearchOptions.IsObjectNameSelected || searchParameters.SearchOptions.IsInputVariableSelected || searchParameters.SearchOptions.IsOutputVariableSelected)
            {
                var matchingObjects = variableDefinitions.Where(v => v.IsObject && SearchUtils.FilterText(v.Name, searchParameters));
                foreach (var matchingObject in matchingObjects)
                {
                    var searchResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), SearchItemType.Object, matchingObject.Name);
                    searchResults.Add(searchResult);

                    if (searchParameters.SearchOptions.IsInputVariableSelected)
                    {
                        var searchInputResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), SearchItemType.ObjectInput, matchingObject.Name);
                        searchResults.Add(searchInputResult);
                    }
                    if (searchParameters.SearchOptions.IsOutputVariableSelected)
                    {
                        var searchOutputResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), SearchItemType.ObjectOutput, matchingObject.Name);
                        searchResults.Add(searchOutputResult);
                    }
                }
            }
        }

        private static void GetRecordsetResults(ISearch searchParameters, List<ISearchResult> searchResults, IResource resource, IList<IDev2Definition> variableDefinitions)
        {
            if (searchParameters.SearchOptions.IsRecSetNameSelected || searchParameters.SearchOptions.IsInputVariableSelected || searchParameters.SearchOptions.IsOutputVariableSelected)
            {
                var matchingRecordsets = variableDefinitions.Where(v => v.IsRecordSet && SearchUtils.FilterText(v.RecordSetName, searchParameters));
                foreach (var recordSet in matchingRecordsets)
                {
                    var searchResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), SearchItemType.RecordSet, recordSet.RecordSetName);
                    searchResults.Add(searchResult);

                    if (searchParameters.SearchOptions.IsInputVariableSelected)
                    {
                        var searchInputResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), SearchItemType.RecordSetInput, recordSet.RecordSetName);
                        searchResults.Add(searchInputResult);
                    }
                    if (searchParameters.SearchOptions.IsOutputVariableSelected)
                    {
                        var searchOutputResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), SearchItemType.RecordSetOutput, recordSet.RecordSetName);
                        searchResults.Add(searchOutputResult);
                    }
                }
            }
        }

        private static void GetScalarResults(ISearch searchParameters, List<ISearchResult> searchResults, IResource resource, IList<IDev2Definition> variableDefinitions)
        {
            if (searchParameters.SearchOptions.IsScalarNameSelected || searchParameters.SearchOptions.IsInputVariableSelected || searchParameters.SearchOptions.IsOutputVariableSelected)
            {
                var matchingScalars = variableDefinitions.Where(v => !v.IsObject && !v.IsRecordSet && SearchUtils.FilterText(v.Name, searchParameters));
                foreach (var scalar in matchingScalars)
                {
                    var searchResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), SearchItemType.Scalar, scalar.Name);
                    searchResults.Add(searchResult);

                    if (searchParameters.SearchOptions.IsInputVariableSelected)
                    {
                        var searchInputResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), SearchItemType.ScalarInput, scalar.Name);
                        searchResults.Add(searchInputResult);
                    }
                    if (searchParameters.SearchOptions.IsOutputVariableSelected)
                    {
                        var searchOutputResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), SearchItemType.ScalarOutput, scalar.Name);
                        searchResults.Add(searchOutputResult);
                    }
                }
            }
        }

        private static enDev2ColumnArgumentDirection GetIoType(ISearch searchParameters)
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

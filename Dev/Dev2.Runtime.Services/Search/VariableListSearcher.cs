#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Search;
using Dev2.Common.Search;
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
                var allResources = _resourceCatalog.GetResources(GlobalConstants.ServerWorkspaceID).Where(res => res.ResourceType != "ReservedService");
                foreach (var resource in allResources)
                {
                    if (resource.DataList != null && resource.DataList.Length > 0)
                    {
                        GetScalarResults(searchParameters, searchResults, resource);
                        GetRecordsetResults(searchParameters, searchResults, resource);
                        GetObjectResults(searchParameters, searchResults, resource);
                    }
                }
            }
            return searchResults;
        }

        private static void GetObjectResults(ISearch searchParameters, List<ISearchResult> searchResults, IResource resource)
        {
            if (searchParameters.SearchOptions.IsObjectNameSelected)
            {
                AddObjectResult(searchParameters, searchResults, resource, enDev2ColumnArgumentDirection.None, SearchItemType.Object);
            }
            if (searchParameters.SearchOptions.IsInputVariableSelected)
            {
                AddObjectResult(searchParameters, searchResults, resource, enDev2ColumnArgumentDirection.Input, SearchItemType.ObjectInput);
            }
            if (searchParameters.SearchOptions.IsOutputVariableSelected)
            {
                AddObjectResult(searchParameters, searchResults, resource, enDev2ColumnArgumentDirection.Output, SearchItemType.ObjectOutput);
            }
        }

        private static void AddObjectResult(ISearch searchParameters, List<ISearchResult> searchResults, IResource resource, enDev2ColumnArgumentDirection type, SearchItemType option)
        {
            var variableList = resource.DataList.Replace(GlobalConstants.SerializableResourceQuote, "\"").Replace(GlobalConstants.SerializableResourceSingleQuote, "\'").ToString();
            var variableDefinitions = DataListUtil.GenerateDefsFromDataList(variableList, type, true, searchParameters);
            var matchingObjects = variableDefinitions.Where(v => v.IsObject);

            foreach (var scalar in matchingObjects)
            {
                var searchResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), option, scalar.Name);
                searchResults.Add(searchResult);
            }
        }

        private static void GetRecordsetResults(ISearch searchParameters, List<ISearchResult> searchResults, IResource resource)
        {
            if (searchParameters.SearchOptions.IsRecSetNameSelected)
            {
                AddRecordsetResult(searchParameters, searchResults, resource, enDev2ColumnArgumentDirection.None, SearchItemType.RecordSet);
            }
            if (searchParameters.SearchOptions.IsInputVariableSelected)
            {
                AddRecordsetResult(searchParameters, searchResults, resource, enDev2ColumnArgumentDirection.Input, SearchItemType.RecordSetInput);
            }
            if (searchParameters.SearchOptions.IsOutputVariableSelected)
            {
                AddRecordsetResult(searchParameters, searchResults, resource, enDev2ColumnArgumentDirection.Output, SearchItemType.RecordSetOutput);
            }
        }

        private static void AddRecordsetResult(ISearch searchParameters, List<ISearchResult> searchResults, IResource resource, enDev2ColumnArgumentDirection type, SearchItemType option)
        {
            var variableList = resource.DataList.Replace(GlobalConstants.SerializableResourceQuote, "\"").Replace(GlobalConstants.SerializableResourceSingleQuote, "\'").ToString();
            var variableDefinitions = DataListUtil.GenerateDefsFromDataList(variableList, type, true, searchParameters);
            var matchingRecordsets = variableDefinitions.Where(v => v.IsRecordSet);

            foreach (var recordset in matchingRecordsets)
            {
                var searchResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), option, recordset.RecordSetName);
                searchResults.Add(searchResult);
            }
        }

        private static void GetScalarResults(ISearch searchParameters, List<ISearchResult> searchResults, IResource resource)
        {
            if (searchParameters.SearchOptions.IsScalarNameSelected)
            {
                AddScalarResult(searchParameters, searchResults, resource, enDev2ColumnArgumentDirection.None, SearchItemType.Scalar);
            }
            if (searchParameters.SearchOptions.IsInputVariableSelected)
            {
                AddScalarResult(searchParameters, searchResults, resource, enDev2ColumnArgumentDirection.Input, SearchItemType.ScalarInput);
            }
            if (searchParameters.SearchOptions.IsOutputVariableSelected)
            {
                AddScalarResult(searchParameters, searchResults, resource, enDev2ColumnArgumentDirection.Output, SearchItemType.ScalarOutput);
            }
        }

        private static void AddScalarResult(ISearch searchParameters, List<ISearchResult> searchResults, IResource resource, enDev2ColumnArgumentDirection type, SearchItemType option)
        {
            var variableList = resource.DataList.Replace(GlobalConstants.SerializableResourceQuote, "\"").Replace(GlobalConstants.SerializableResourceSingleQuote, "\'").ToString();
            var nvariableDefinitions = DataListUtil.GenerateDefsFromDataList(variableList, type, true, searchParameters);
            var matchingScalars = nvariableDefinitions.Where(v => !v.IsObject && !v.IsRecordSet);

            foreach (var scalar in matchingScalars)
            {
                var searchResult = new SearchResult(resource.ResourceID, resource.ResourceName, resource.GetResourcePath(GlobalConstants.ServerWorkspaceID), option, scalar.Name);
                searchResults.Add(searchResult);
            }
        }
    }
}

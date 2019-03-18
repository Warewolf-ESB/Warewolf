#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Search;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Search;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetFilterListService : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                if (values == null)
                {
                    throw new InvalidDataContractException(ErrorResource.NoParameter);
                }
                string serializedSource = null;
                values.TryGetValue("Search", out StringBuilder searchValueSB);
                values.TryGetValue("SearchInput", out StringBuilder searchInputSB);
                if (searchValueSB != null)
                {
                    serializedSource = searchValueSB.ToString();
                }

                if (string.IsNullOrEmpty(serializedSource))
                {
                    var message = new ExecuteMessage();
                    message.HasError = true;
                    message.SetMessage("No Search found");
                    Dev2Logger.Debug("No Search found", GlobalConstants.WarewolfDebug);
                    return serializer.SerializeToBuilder(message);
                }

                var searchResults = new List<ISearchResult>();

                var searchValue = serializer.Deserialize<ISearch>(serializedSource);
                if (searchValue != null)
                {
                    var searchers = new List<ISearcher>
                    {
                        new ActivitySearcher(ResourceCatalog.Instance),
                        new TestSearcher(ResourceCatalog.Instance,TestCatalog.Instance),
                        new VariableListSearcher(ResourceCatalog.Instance),
                        new ResourceSearcher(ResourceCatalog.Instance)
                    };
                    searchResults = searchValue.GetSearchResults(searchers);                    
                }

                return serializer.SerializeToBuilder(searchResults);
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                var res = new CompressedExecuteMessage { HasError = true, Message = new StringBuilder(err.Message) };
                return serializer.SerializeToBuilder(res);
            }
        }
        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Database ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "GetFilterListService";
    }
}

#pragma warning disable
 using System;
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

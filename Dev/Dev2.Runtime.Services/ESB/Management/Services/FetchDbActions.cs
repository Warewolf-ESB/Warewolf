using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Warewolf.Core;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchDbActions : IEsbManagementEndpoint
    {


        public string HandlesType()
        {
            return "FetchDbActions";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {




                var dbSource = serializer.Deserialize<IDbSource>(values["source"]);
                // ReSharper disable MaximumChainedReferences
                ServiceModel.Services services = new ServiceModel.Services();
                var src = ResourceCatalog.Instance.GetResource<DbSource>(GlobalConstants.ServerWorkspaceID, dbSource.Id);
                var methods = services.FetchMethods(src).Select(CreateDbAction);
                return serializer.SerializeToBuilder(new ExecuteMessage()
                {
                    HasError = false,
                    Message = serializer.SerializeToBuilder(methods)
                });
                // ReSharper restore MaximumChainedReferences
            }
            catch (Exception e)
            {

                return serializer.SerializeToBuilder(new ExecuteMessage()
                {
                    HasError = true,
                    Message = new StringBuilder(e.Message)
                });
            }
        }

        DbAction CreateDbAction(ServiceMethod a)
        {
            // ReSharper disable MaximumChainedReferences
            var Inputs = a.Parameters.Select(b => new DbInput(b.Name, b.DefaultValue ?? "") as IDbInput).ToList();
            // ReSharper restore MaximumChainedReferences
            return new DbAction()
            {
                Name = a.Name, 
                Inputs = Inputs
            };
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }

        public ResourceCatalog Resources
        {
            get
            {
                return ResourceCatalog.Instance;
            }

        }
    }
}
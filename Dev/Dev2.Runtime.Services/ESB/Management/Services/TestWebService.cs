using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Unlimited.Framework.Converters.Graph.Ouput;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class TestWebService : IEsbManagementEndpoint
        // ReSharper restore UnusedMember.Global
    {
        IResourceCatalog _rescat;
        IWebServices _webServices;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {

                Dev2Logger.Log.Info("Test DB Connection Service");
                StringBuilder resourceDefinition;

                values.TryGetValue("WebService", out resourceDefinition);

                IWebService src = serializer.Deserialize<IWebService>(resourceDefinition);
                // ReSharper disable MaximumChainedReferences
                var parameters = src.Inputs==null?new List<MethodParameter>(): src.Inputs.Select(a => new MethodParameter { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value }).ToList();
                // ReSharper restore MaximumChainedReferences

                var res = new WebService
                {
                    Method = new ServiceMethod(src.Name, src.Name, parameters, new OutputDescription(), new List<MethodOutput>(),"test"),
                    RequestUrl = string.Concat(src.SourceUrl,src.RequestUrl),
                    ResourceName = src.Name,
                    ResourcePath = src.Path,
                    ResourceID = src.Id,
                    RequestBody = src.PostData,
                    Headers = src.Headers,
                    RequestResponse =  src.Response
                   
                };

                WebServices.TestWebService(res);
                msg.HasError = false;
                msg.Message = serializer.SerializeToBuilder(res);
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Log.Error(err);

            }

            return serializer.SerializeToBuilder(msg);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><WebService ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public IResourceCatalog ResourceCatalogue
        {
            get
            {
                return _rescat ?? ResourceCatalog.Instance;
            }
            set
            {
                _rescat = value;
            }
        }
        public IWebServices WebServices
        {
            get
            {
                return _webServices?? new WebServices();
            }
            set
            {
                _webServices = value;
            }
        }

        public string HandlesType()
        {
            return "TestWebService";
        }
    }
}
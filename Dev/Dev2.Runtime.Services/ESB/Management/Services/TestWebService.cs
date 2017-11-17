using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Unlimited.Framework.Converters.Graph.Ouput;


namespace Dev2.Runtime.ESB.Management.Services
{
    public class TestWebService : IEsbManagementEndpoint    
    {
        IResourceCatalog _rescat;
        IWebServices _webServices;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Test Web Connection Service", GlobalConstants.WarewolfInfo);

                values.TryGetValue("WebService", out StringBuilder resourceDefinition);

                IWebService src = serializer.Deserialize<IWebService>(resourceDefinition);
                
                var parameters = src.Inputs?.Select(a => new MethodParameter { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value }).ToList() ?? new List<MethodParameter>();
                
                var requestHeaders = src.Headers.Select(nameValue => nameValue.Name + ":" + nameValue.Value).ToList();
                var requestHeader = string.Join(";", requestHeaders).TrimEnd(':',';');
                var res = new WebService
                {
                    Method = new ServiceMethod(src.Name, src.Name, parameters, new OutputDescription(), new List<MethodOutput>(), "test"),
                    RequestUrl = string.Concat(src.RequestUrl, src.QueryString),
                    ResourceName = src.Name,
                    ResourceID = src.Id,
                    RequestBody = src.PostData,
                    Headers = src.Headers,
                    RequestHeaders = requestHeader,
                    RequestMethod = src.Method,
                    RequestResponse = src.Response,
                    Source = new WebSource
                    {
                        Address = src.Source.HostName,
                        DefaultQuery = src.Source.DefaultQuery,
                        AuthenticationType = src.Source.AuthenticationType,
                        UserName = src.Source.UserName,
                        Password = src.Source.Password
                    }
                };

                WebServices.TestWebService(res);
                msg.HasError = false;
                msg.Message = serializer.SerializeToBuilder(res);
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }

            return serializer.SerializeToBuilder(msg);
        }

        public IResourceCatalog ResourceCatalogue
        {
            get => _rescat ?? ResourceCatalog.Instance;
            set => _rescat = value;
        }

        public IWebServices WebServices
        {
            get => _webServices ?? new WebServices();
            set => _webServices = value;
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><WebService ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "TestWebService";
    }
}

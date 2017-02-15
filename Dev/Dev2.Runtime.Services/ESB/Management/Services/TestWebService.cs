using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Unlimited.Framework.Converters.Graph.Ouput;
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Runtime.ESB.Management.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class TestWebService : IEsbManagementEndpoint
    // ReSharper restore UnusedMember.Global
    {
        IResourceCatalog _rescat;
        IWebServices _webServices;



        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Contribute;
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {

                Dev2Logger.Info("Test Web Connection Service");
                StringBuilder resourceDefinition;

                values.TryGetValue("WebService", out resourceDefinition);

                IWebService src = serializer.Deserialize<IWebService>(resourceDefinition);
                // ReSharper disable MaximumChainedReferences
                var parameters = src.Inputs?.Select(a => new MethodParameter { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value }).ToList() ?? new List<MethodParameter>();
                // ReSharper restore MaximumChainedReferences
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
                Dev2Logger.Error(err);

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
                return _webServices ?? new WebServices();
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

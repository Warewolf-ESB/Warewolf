using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Unlimited.Framework.Converters.Graph.Ouput;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class TestWebService : IEsbManagementEndpoint
        // ReSharper restore UnusedMember.Global
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {

                Dev2Logger.Log.Info("Test DB Connection Service");
                StringBuilder resourceDefinition;

                values.TryGetValue("DbService", out resourceDefinition);

                IWebService src = serializer.Deserialize<IWebService>(resourceDefinition);
                // ReSharper disable MaximumChainedReferences
                var parameters = src.Inputs==null?new List<MethodParameter>(): src.Inputs.Select(a => new MethodParameter() { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value }).ToList();
                // ReSharper restore MaximumChainedReferences
                var source = ResourceCatalog.Instance.GetResource<DbSource>(GlobalConstants.ServerWorkspaceID, src.Source.Id);
                var res = new WebService
                {
                    Method = new ServiceMethod(src.Name, src.Name, parameters, new OutputDescription(), new List<MethodOutput>(), src.Action.Name),
                    ResourceName = src.Name,
                    ResourcePath = src.Path,
                    ResourceID = src.Id,
                    Source = source


                };
                ServiceModel.WebServices services = new ServiceModel.WebServices();
                services.TestWebService(res);
                var result =res.Recordsets;
                msg.HasError = false;
                msg.Message = serializer.SerializeToBuilder(result);
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
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><WebSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "TestWebService";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveWebserviceSource : IEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {

                Dev2Logger.Log.Info("Save Webservice Source");
                StringBuilder resourceDefinition;

                values.TryGetValue("WebserviceSource", out resourceDefinition);

                var src = serializer.Deserialize<WebServiceSourceDefinition>(resourceDefinition);
                if (src.Path.EndsWith("\\"))
                    src.Path = src.Path.Substring(0, src.Path.LastIndexOf("\\", StringComparison.Ordinal));
                var res = new WebSource
                {
                    AuthenticationType = src.AuthenticationType,
                    Address = src.HostName,
                    Password = src.Password,
                    UserName = src.UserName,
                    ResourceID = src.Id,
                    DefaultQuery = src.DefaultQuery,
                    ResourceName = src.Name,
                    ResourcePath = src.Path
                };
                var con = new WebSources();
                var result = con.Test(res);

                if (result.IsValid)
                {
                    ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, res);
                    ServerExplorerRepo.UpdateItem(res);
                 
                    msg.HasError = false;
                }
                else
                {
                    msg.HasError = false;
                    msg.Message = new StringBuilder(res.IsValid ? "" : result.ErrorMessage); 
                }
              


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
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><WebserviceSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }
        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get { return _serverExplorerRepository ?? ServerExplorerRepository.Instance; }
            set { _serverExplorerRepository = value; }
        }
        public string HandlesType()
        {
            return "SaveWebserviceSource";
        }
    }
}
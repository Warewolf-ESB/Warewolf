using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Unlimited.Framework.Converters.Graph.Ouput;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveDbService : IEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {

                Dev2Logger.Log.Info("Save Resource Service");
                StringBuilder resourceDefinition;

                values.TryGetValue("DbService", out resourceDefinition);

                IDatabaseService src = serializer.Deserialize<IDatabaseService>(resourceDefinition);
                if (src.Path.EndsWith("\\"))
                    src.Path = src.Path.Substring(0, src.Path.LastIndexOf("\\", System.StringComparison.Ordinal));
                var res = new DbService
                {
                    Method = new ServiceMethod(src.Name,src.Name,new List<MethodParameter>(),new OutputDescription(), new List<MethodOutput>(),src.Action.Name),
                    ResourceName = src.Name,
                    ResourcePath = src.Path,
                    ResourceID = src.Id

                   
                };
                ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, res);
                var explorerItem = ServerExplorerRepo.UpdateItem(res);

                msg.HasError = false;
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
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><DbService ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
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
            return "SaveDbServiceService";
        }
    }
}
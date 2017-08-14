using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{

    public class SaveSharePointServiceSource : IEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;
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
                Dev2Logger.Info("Save Sharepoint Source");
                StringBuilder resourceDefinition;

                values.TryGetValue("SharepointServer", out resourceDefinition);

                var src = serializer.Deserialize<SharePointServiceSourceDefinition>(resourceDefinition);
                if (src.Path.EndsWith("\\"))
                    src.Path = src.Path.Substring(0, src.Path.LastIndexOf("\\", StringComparison.Ordinal));
                var res = new SharepointSource
                {
                    AuthenticationType = src.AuthenticationType,
                    Server = src.Server,
                    Password = src.Password,
                    UserName = src.UserName,
                    ResourceID = src.Id,
                    ResourceName = src.Name,
                    IsSharepointOnline = src.IsSharepointOnline,
                };
                ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, res, src.Path);
                msg.HasError = false;
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
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><SharepointServer ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
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
            return "SaveSharepointServerService";
        }
    }
}
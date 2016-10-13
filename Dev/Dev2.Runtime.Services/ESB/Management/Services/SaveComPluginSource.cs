using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SaveComPluginSource : IEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;
       
        private IAuthorizer _authorizer;
        private IAuthorizer Authorizer => _authorizer ?? (_authorizer = new SecuredCreateEndpoint());

        public SaveComPluginSource(IAuthorizer authorizer)
        {
            _authorizer = authorizer;
        }

        // ReSharper disable once MemberCanBeInternal
        public SaveComPluginSource()
        {

        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {
                Authorizer.RunPermissions(GlobalConstants.ServerWorkspaceID);
                Dev2Logger.Info("Save Com Plugin Source");
                StringBuilder resourceDefinition;

                values.TryGetValue("ComPluginSource", out resourceDefinition);

                var src = serializer.Deserialize<ComPluginSourceDefinition>(resourceDefinition);
                if(src.ResourcePath == null) 
                    src.ResourcePath = string.Empty;
                if (src.ResourcePath.EndsWith("\\"))
                    src.ResourcePath = src.ResourcePath.Substring(0, src.ResourcePath.LastIndexOf("\\", StringComparison.Ordinal));
                var res = new ComPluginSource
                {
                    ResourceID = src.Id,
                    ClsId = src.ClsId,
                    Is32Bit = src.Is32Bit,
                    ComName = src.SelectedDll.Name,
                    ResourceName = src.ResourceName
                };
                ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, res, src.ResourcePath);
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
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><ComPluginSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
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
            return "SaveComPluginSource";
        }
    }
}
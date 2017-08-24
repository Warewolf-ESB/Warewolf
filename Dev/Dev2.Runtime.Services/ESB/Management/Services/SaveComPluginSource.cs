using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{

    public class SaveComPluginSource : IEsbManagementEndpoint
    {

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Contribute;
        }

        private IResourceCatalog _resourceCatalog;

        public SaveComPluginSource()
        {
            
        }
        public SaveComPluginSource(IResourceCatalog resourceCatalog)
        {
            _resourceCatalog = resourceCatalog;
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Save Com Plugin Source", GlobalConstants.WarewolfInfo);
                StringBuilder resourceDefinition;

                values.TryGetValue("ComPluginSource", out resourceDefinition);

                var src = serializer.Deserialize<ComPluginSourceDefinition>(resourceDefinition);
                if (src.ResourcePath == null)
                    src.ResourcePath = string.Empty;
                if (src.ResourcePath.EndsWith("\\"))
                    src.ResourcePath = src.ResourcePath.Substring(0, src.ResourcePath.LastIndexOf("\\", StringComparison.Ordinal));

                ComPluginSource res1;
                var existingSource = ResourceCat.GetResource(GlobalConstants.ServerWorkspaceID, src.Name);
                if (existingSource != null)
                {
                    res1 = existingSource as ComPluginSource;
                }
                else
                {
                    res1 = new ComPluginSource
                    {
                        ResourceID = src.Id,
                        ClsId = src.ClsId,
                        Is32Bit = src.Is32Bit,
                        ComName = src.SelectedDll.Name,
                        ResourceName = src.ResourceName
                    };
                }



                ResourceCat.SaveResource(GlobalConstants.ServerWorkspaceID, res1, src.ResourcePath);
                msg.HasError = false;

            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);

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
        public IResourceCatalog ResourceCat
        {
            get { return _resourceCatalog ?? ResourceCatalog.Instance; }
            set { _resourceCatalog = value; }
        }
        public string HandlesType()
        {
            return "SaveComPluginSource";
        }
    }
}
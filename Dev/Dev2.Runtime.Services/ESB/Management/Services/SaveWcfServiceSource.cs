﻿using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.Interfaces;


namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveWcfServiceSource : IEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;
        IResourceCatalog _resourceCatalogue;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Save Wcf Service Source", GlobalConstants.WarewolfInfo);

                values.TryGetValue("WcfSource", out StringBuilder resourceDefinition);

                var src = serializer.Deserialize<WcfServiceSourceDefinition>(resourceDefinition);
                var con = new WcfSource
                {
                    EndpointUrl = src.EndpointUrl,
                    ResourceName = src.Name,
                    Name = src.Name,
                    ResourceID = src.Id,
                    Type = enSourceType.WcfSource,
                    ResourceType = "WcfSource"
                };
                ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, con, src.Path);
                ServerExplorerRepo.UpdateItem(con);

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

        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get => _serverExplorerRepository ?? ServerExplorerRepository.Instance;
            set => _serverExplorerRepository = value;
        }

        public IResourceCatalog ResourceCatalogue
        {
            get => _resourceCatalogue ?? ResourceCatalog.Instance;
            set => _resourceCatalogue = value;
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><EmailServiceSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "SaveWcfServiceSource";
    }
}
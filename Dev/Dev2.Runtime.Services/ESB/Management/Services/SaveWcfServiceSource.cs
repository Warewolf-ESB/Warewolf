﻿using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Runtime.Interfaces;
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Runtime.ESB.Management.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SaveWcfServiceSource : IEsbManagementEndpoint
    {
        private IExplorerServerResourceRepository _serverExplorerRepository;
        private IResourceCatalog _resourceCatalogue;
        private IAuthorizer _authorizer;
        private IAuthorizer Authorizer => _authorizer ?? (_authorizer = new SecuredCreateEndpoint());

        public SaveWcfServiceSource(IAuthorizer authorizer)
        {
            _authorizer = authorizer;
        }

        // ReSharper disable once MemberCanBeInternal
        public SaveWcfServiceSource()
        {

        }
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {
                Authorizer.RunPermissions(GlobalConstants.ServerWorkspaceID);
                Dev2Logger.Info("Save Wcf Service Source");
                StringBuilder resourceDefinition;

                values.TryGetValue("WcfSource", out resourceDefinition);

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
                Dev2Logger.Error(err);
            }

            return serializer.SerializeToBuilder(msg);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><EmailServiceSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get { return _serverExplorerRepository ?? ServerExplorerRepository.Instance; }
            set { _serverExplorerRepository = value; }
        }

        public IResourceCatalog ResourceCatalogue
        {
            get
            {
                return _resourceCatalogue ?? ResourceCatalog.Instance;
            }
            set
            {
                _resourceCatalogue = value;
            }
        }

        public string HandlesType()
        {
            return "SaveWcfServiceSource";
        }
    }
}
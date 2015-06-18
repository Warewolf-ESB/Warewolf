using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Email;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveEmailServiceSource : IEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;
        IResourceCatalog _resourceCatalogue;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Log.Info("Save Email Service Source");
                StringBuilder resourceDefinition;

                values.TryGetValue("EmailServiceSource", out resourceDefinition);

                IEmailServiceSource src = serializer.Deserialize<IEmailServiceSource>(resourceDefinition);
                EmailSource con = new EmailSource();
                con.Host = src.HostName;
                con.UserName = src.UserName;
                con.Password = src.Password;
                con.Port = src.Port;
                con.EnableSsl = src.EnableSsl;
                con.Timeout = src.Timeout;

                // ReSharper disable MaximumChainedReferences
                // ReSharper restore MaximumChainedReferences
                var emailsrc = ResourceCatalog.Instance.GetResource<EmailSource>(GlobalConstants.ServerWorkspaceID, src.Id);

                ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, con);
                var explorerItem = ServerExplorerRepo.UpdateItem(con);

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
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><EmailServiceSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = Common.Interfaces.Core.DynamicServices.enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
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
                return _resourceCatalogue?? ResourceCatalog.Instance;
            }
            set
            {
                _resourceCatalogue = value;
            }
        }

        public string HandlesType()
        {
            return "EmailServiceSource";
        }
    }
}
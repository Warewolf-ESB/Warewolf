using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveDbSourceSource : IEsbManagementEndpoint
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

                values.TryGetValue("DbSource", out resourceDefinition);

                IDbSource src = serializer.Deserialize<DbSourceDefinition>(resourceDefinition);
                if (src.Path.EndsWith("\\"))
                    src.Path = src.Path.Substring(0, src.Path.LastIndexOf("\\", System.StringComparison.Ordinal));
                var res = new DbSource
                {
                    AuthenticationType = src.AuthenticationType,
                    Server = src.ServerName,
                    Password = src.Password,
                    UserID = src.UserName,
                    ResourceID = src.Id,
                    DatabaseName = src.DbName,
                    ResourceName = src.Name,
                    ResourcePath = src.Path
                };
                var con = new DbSources();
                var result = con.DoDatabaseValidation(res);

                if (result.IsValid)
                {
                    ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, res);
                    var explorerItem = ServerExplorerRepo.UpdateItem(res);
                 
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
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><ServerSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
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
            return "SaveDbSourceService";
        }
    }
}
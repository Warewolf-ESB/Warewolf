using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Explorer;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class RenameItemService : IEsbManagementEndpoint
    {
        private IExplorerServerResourceRepository _serverExplorerRepository;

        public string HandlesType()
        {
            return "RenameItemService";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            IExplorerRepositoryResult item;
            var serializer = new Dev2JsonSerializer();
            try
            {
                if(values == null)
                {
                    throw new ArgumentNullException("values");
                }
                StringBuilder itemToBeRenamed;
                StringBuilder newName;
                if(!values.TryGetValue("itemToRename", out itemToBeRenamed))
                {
                    throw new ArgumentException("itemToRename value not supplied.");
                }
                if(!values.TryGetValue("newName", out newName))
                {
                    throw new ArgumentException("newName value not supplied.");
                }
                
                var itemToRename = serializer.Deserialize<ServerExplorerItem>(itemToBeRenamed);
                Dev2Logger.Log.Info(String.Format("Rename Item. Path:{0} NewPath:{1}", itemToBeRenamed, newName));
                item = ServerExplorerRepo.RenameItem(itemToRename, newName.ToString(), GlobalConstants.ServerWorkspaceID);
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error(e);
                item = new ExplorerRepositoryResult(ExecStatus.Fail, e.Message);
            }
            return serializer.SerializeToBuilder(item);
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><itemToRename ColumnIODirection=\"Input\"/><newName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }

        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get { return _serverExplorerRepository ?? ServerExplorerRepository.Instance; }
            set { _serverExplorerRepository = value; }
        }
    }
}

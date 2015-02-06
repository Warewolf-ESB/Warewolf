
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Explorer;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class AddFolderService : IEsbManagementEndpoint
    {
        private IExplorerServerResourceRepository _serverExplorerRepository;

        public string HandlesType()
        {
            return "AddFolderService";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
           
            var serializer = new Dev2JsonSerializer();
            var name = (values["name"].ToString());
            var parentGuid = Guid.Parse(values["parentGuid"].ToString());
            var id = Guid.Parse((values["id"].ToString()));
            var parent = ServerExplorerRepo.Find(parentGuid);
            var itemToAdd = new ServerExplorerItem(name, id, ResourceType.Folder, new List<IExplorerItem>(), Permissions.Contribute, parent.ResourcePath + "\\" + name,"","");
            parent.Children.Add(itemToAdd);
            Dev2Logger.Log.Info("Add Folder Service." +itemToAdd);
            itemToAdd.Permissions = Permissions.Contribute;
            if(itemToAdd.ResourcePath.ToLower().StartsWith("root\\"))
            {
                itemToAdd.ResourcePath = itemToAdd.ResourcePath.Remove(0, 5);
            }

            var item = ServerExplorerRepo.AddItem(itemToAdd, theWorkspace.ID);
            return serializer.SerializeToBuilder(item);
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><name ColumnIODirection=\"Input\"/><parentGuid ColumnIODirection=\"Input\"/><id ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

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

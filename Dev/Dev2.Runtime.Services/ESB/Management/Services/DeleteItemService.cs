#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Explorer;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;


namespace Dev2.Runtime.ESB.Management.Services
{
    public class DeleteItemService : IEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {

            if (requestArgs != null && requestArgs.TryGetValue("itemToDelete", out StringBuilder itemBeingDeleted) && itemBeingDeleted != null)
            {
                var itemToDelete = ServerExplorerRepo.Find(a => a.ResourceId.ToString() == itemBeingDeleted.ToString());
                if (itemToDelete != null)
                {
                    return itemToDelete.ResourceId;
                }
            }


            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            IExplorerRepositoryResult item = null;
            var serializer = new Dev2JsonSerializer();
            try
            {
                if (values == null)
                {
                    throw new ArgumentNullException(nameof(values));
                }
                StringBuilder pathBeingDeleted = null;
                if (!values.TryGetValue("itemToDelete", out StringBuilder itemBeingDeleted) && !values.TryGetValue("folderToDelete", out pathBeingDeleted))
                {
                    throw new ArgumentException(string.Format(ErrorResource.IsBlank, "itemToDelete"));
                }


                IExplorerItem itemToDelete;
                if (itemBeingDeleted != null)
                {
                    itemToDelete = ServerExplorerRepo.Find(a => a.ResourceId.ToString() == itemBeingDeleted.ToString());
                    Dev2Logger.Info("Delete Item Service." + itemToDelete, GlobalConstants.WarewolfInfo);
                    item = ServerExplorerRepo.DeleteItem(itemToDelete, GlobalConstants.ServerWorkspaceID);
                }
                else
                {
                    if (pathBeingDeleted != null)
                    {
                        itemToDelete = new ServerExplorerItem
                        {
                            ResourceType = "Folder",
                            ResourcePath = pathBeingDeleted.ToString()
                        };
                        item = ServerExplorerRepo.DeleteItem(itemToDelete, GlobalConstants.ServerWorkspaceID);
                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Delete Item Error", e, GlobalConstants.WarewolfError);
                item = new ExplorerRepositoryResult(ExecStatus.Fail, e.Message);
            }
            return serializer.SerializeToBuilder(item);
        }

        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get => _serverExplorerRepository ?? ServerExplorerRepository.Instance;
            set => _serverExplorerRepository = value;
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><itemToAdd ColumnIODirection=\"itemToDelete\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "DeleteItemService";
    }
}

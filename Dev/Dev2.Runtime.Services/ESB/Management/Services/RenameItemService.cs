/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Explorer;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;

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
            IExplorerRepositoryResult item=null;
            var serializer = new Dev2JsonSerializer();
            try
            {
                if(values == null)
                {
                    throw new ArgumentNullException("values");
                }
                StringBuilder itemToBeRenamed;
                StringBuilder newName;
                StringBuilder folderToBeRenamed=null;
                if(!values.TryGetValue("itemToRename", out itemToBeRenamed))
                {
                    if (!values.TryGetValue("folderToRename", out folderToBeRenamed))
                    {
                        throw new ArgumentException(string.Format(ErrorResource.ValueNotSupplied, "itemToRename"));
                    }
                }
                if(!values.TryGetValue("newName", out newName))
                {
                    throw new ArgumentException(string.Format(ErrorResource.ValueNotSupplied, "newName"));
                }
                IExplorerItem explorerItem;
                if (itemToBeRenamed != null)
                {
                    explorerItem = ServerExplorerRepo.Find(Guid.Parse(itemToBeRenamed.ToString()));
                    Dev2Logger.Info(String.Format("Rename Item. Path:{0} NewPath:{1}", explorerItem.ResourcePath, newName));
                    item = ServerExplorerRepo.RenameItem(explorerItem, newName.ToString(), GlobalConstants.ServerWorkspaceID);
                }
                else if (folderToBeRenamed != null)
                {
                    explorerItem = new ServerExplorerItem()
                    {
                        ResourceType = "Folder",
                        ResourcePath = folderToBeRenamed.ToString()

                    };
                    item = ServerExplorerRepo.RenameItem(explorerItem, newName.ToString(), GlobalConstants.ServerWorkspaceID);
                    
                }
                if (item.Status == ExecStatus.Success)
                {
                    ResourceCatalog.Instance.Reload();
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Error(e);
                item = new ExplorerRepositoryResult(ExecStatus.Fail, e.Message);
            }
            return serializer.SerializeToBuilder(item);
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><itemToRename ColumnIODirection=\"Input\"/><newName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

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

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
    public class RenameItemService : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            requestArgs.TryGetValue("itemToRename", out StringBuilder tmp);
            if (tmp != null && Guid.TryParse(tmp.ToString(), out Guid resourceId))
            {
                return resourceId;
            }


            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;
        IExplorerServerResourceRepository _serverExplorerRepository;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            IExplorerRepositoryResult item=null;
            var serializer = new Dev2JsonSerializer();
            try
            {
                if(values == null)
                {
                    throw new ArgumentNullException(nameof(values));
                }
                StringBuilder folderToBeRenamed = null;
                if (!values.TryGetValue("itemToRename", out StringBuilder itemToBeRenamed) && !values.TryGetValue("folderToRename", out folderToBeRenamed))
                {
                    throw new ArgumentException(string.Format(ErrorResource.ValueNotSupplied, "itemToRename"));
                }

                if (!values.TryGetValue("newName", out StringBuilder newName))
                {
                    throw new ArgumentException(string.Format(ErrorResource.ValueNotSupplied, "newName"));
                }
                IExplorerItem explorerItem;
                if (itemToBeRenamed != null)
                {
                    explorerItem = ServerExplorerRepo.Find(Guid.Parse(itemToBeRenamed.ToString()));
                    if (explorerItem == null)
                    {
                        throw new ArgumentException(string.Format(ErrorResource.FailedToFindResource, "newName"));
                    }
                    Dev2Logger.Info($"Rename Item. Path:{explorerItem.ResourcePath} NewPath:{newName}", GlobalConstants.WarewolfInfo);
                    item = ServerExplorerRepo.RenameItem(explorerItem, newName.ToString(), GlobalConstants.ServerWorkspaceID);
                }
                else
                {
                    if (folderToBeRenamed != null)
                    {
                        explorerItem = new ServerExplorerItem()
                        {
                            ResourceType = "Folder",
                            ResourcePath = folderToBeRenamed.ToString()

                        };
                        item = ServerExplorerRepo.RenameItem(explorerItem, newName.ToString(), GlobalConstants.ServerWorkspaceID);
                    }
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                item = new ExplorerRepositoryResult(ExecStatus.Fail, e.Message);
            }
            return serializer.SerializeToBuilder(item);
        }

        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get => _serverExplorerRepository ?? ServerExplorerRepository.Instance;
            set => _serverExplorerRepository = value;
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><itemToRename ColumnIODirection=\"Input\"/><newName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "RenameItemService";
    }
}

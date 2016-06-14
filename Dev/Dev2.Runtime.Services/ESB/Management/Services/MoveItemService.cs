
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class MoveItemService : IEsbManagementEndpoint
    {
        private IExplorerServerResourceRepository _serverExplorerRepository;

        public string HandlesType()
        {
            return "MoveItemService";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            IExplorerRepositoryResult item;
            var serializer = new Dev2JsonSerializer();
            try
            {
                StringBuilder itemToBeRenamedPath;
                if (values == null)
                {
                    throw new ArgumentNullException("values");
                }
                StringBuilder itemToBeRenamed;
                StringBuilder newPath;
                if (!values.TryGetValue("itemToMove", out itemToBeRenamed))
                {
                    throw new ArgumentException(string.Format(ErrorResource.ValueNotSupplied, "itemToMove"));
                }
                if (!values.TryGetValue("newPath", out newPath))
                {
                    throw new ArgumentException(string.Format(ErrorResource.ValueNotSupplied, "newName"));
                }
                if (!values.TryGetValue("itemToBeRenamedPath", out itemToBeRenamedPath))
                {
                    throw new ArgumentException(string.Format(ErrorResource.ValueNotSupplied, "newName"));
                }


                var itemToMove = ServerExplorerRepo.Find(Guid.Parse(itemToBeRenamed.ToString())) ?? ServerExplorerRepo.Find(a => a.ResourcePath == itemToBeRenamedPath.ToString());
                Dev2Logger.Info(String.Format("Move Item. Path:{0} NewPath:{1}", itemToBeRenamed, newPath));
                item = ServerExplorerRepo.MoveItem(itemToMove, newPath.ToString(), GlobalConstants.ServerWorkspaceID);
                if(item.Status==ExecStatus.Success && itemToMove.ResourceType=="Folder")
                {
                    if (Directory.Exists(ServerExplorerRepository.DirectoryStructureFromPath(itemToMove.ResourcePath)))
                          Directory.Delete(ServerExplorerRepository.DirectoryStructureFromPath(itemToMove.ResourcePath),true);
                    //ServerExplorerRepo.Reload(Guid.Empty);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
                item = new ExplorerRepositoryResult(ExecStatus.Fail, e.Message);
            }
            return serializer.SerializeToBuilder(item);
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><itemToMove ColumnIODirection=\"Input\"/><newPath ColumnIODirection=\"Input\"/><itemToBeRenamedPath ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

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

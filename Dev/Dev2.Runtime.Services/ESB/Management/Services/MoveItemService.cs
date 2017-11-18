/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;


namespace Dev2.Runtime.ESB.Management.Services
{
    public class MoveItemService : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            requestArgs.TryGetValue("itemToMove", out StringBuilder tmp);
            if (tmp != null)
            {
                if (Guid.TryParse(tmp.ToString(), out Guid resourceId))
                {
                    return resourceId;
                }
            }

            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Contribute;
        }

        private IExplorerServerResourceRepository _serverExplorerRepository;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            IExplorerRepositoryResult item;
            var serializer = new Dev2JsonSerializer();
            try
            {
                if (values == null)
                {
                    throw new ArgumentNullException("values");
                }
                if (!values.TryGetValue("itemToMove", out StringBuilder itemToBeRenamed))
                {
                    throw new ArgumentException(string.Format(ErrorResource.ValueNotSupplied, "itemToMove"));
                }
                if (!values.TryGetValue("newPath", out StringBuilder newPath))
                {
                    throw new ArgumentException(string.Format(ErrorResource.ValueNotSupplied, "newName"));
                }
                if (!values.TryGetValue("itemToBeRenamedPath", out StringBuilder itemToBeRenamedPath))
                {
                    throw new ArgumentException(string.Format(ErrorResource.ValueNotSupplied, "newName"));
                }


                var itemToMove = ServerExplorerRepo.Find(Guid.Parse(itemToBeRenamed.ToString())) ?? ServerExplorerRepo.Find(a => a.ResourcePath == itemToBeRenamedPath.ToString());
                Dev2Logger.Info($"Move Item. Path:{itemToBeRenamed} NewPath:{newPath}", GlobalConstants.WarewolfInfo);
                item = ServerExplorerRepo.MoveItem(itemToMove, newPath.ToString(), GlobalConstants.ServerWorkspaceID);               
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                item = new ExplorerRepositoryResult(ExecStatus.Fail, e.Message);
            }
            return serializer.SerializeToBuilder(item);
        }

        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get { return _serverExplorerRepository ?? ServerExplorerRepository.Instance; }
            set { _serverExplorerRepository = value; }
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><itemToMove ColumnIODirection=\"Input\"/><newPath ColumnIODirection=\"Input\"/><itemToBeRenamedPath ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "MoveItemService";
    }
}

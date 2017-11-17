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
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
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

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
           
            var serializer = new Dev2JsonSerializer();
            var itemToAdd = serializer.Deserialize<ServerExplorerItem>(values["itemToAdd"]);
            Dev2Logger.Info("Add Folder Service." +itemToAdd, GlobalConstants.WarewolfInfo);
            itemToAdd.Permissions = Permissions.Contribute;
            if(itemToAdd.ResourcePath.ToLower().StartsWith("root\\"))
            {
                itemToAdd.ResourcePath = itemToAdd.ResourcePath.Remove(0, 5);
            }            
            var item = ServerExplorerRepo.AddItem(itemToAdd, theWorkspace.ID);
            return serializer.SerializeToBuilder(item);
        }

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Contribute;
        }

        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get => _serverExplorerRepository ?? ServerExplorerRepository.Instance;
            set => _serverExplorerRepository = value;
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><itemToAdd ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "AddFolderService";
    }
}

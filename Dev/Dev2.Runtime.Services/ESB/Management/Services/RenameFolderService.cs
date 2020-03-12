#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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

    public class RenameFolderService : EsbManagementEndpointBase
    {
        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public override AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public override  StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            IExplorerRepositoryResult item;
            try
            {
                if(values == null)
                {
                    throw new ArgumentNullException(nameof(values));
                }
                if(theWorkspace == null)
                {
                    throw new ArgumentNullException(nameof(theWorkspace));
                }
                if (!values.TryGetValue("path", out StringBuilder path))
                {
                    throw new ArgumentException(string.Format(ErrorResource.ValueNotSupplied, "path"));
                }
                if (!values.TryGetValue("newPath", out StringBuilder newPath))
                {
                    throw new ArgumentException(string.Format(ErrorResource.ValueNotSupplied, "newPath"));
                }
                Dev2Logger.Info($"Reanme Folder. Path:{path} NewPath:{newPath}", GlobalConstants.WarewolfInfo);
                var explorerRepository = ServerExplorerRepository.Instance;
                item = explorerRepository.RenameFolder(path.ToString(), newPath.ToString(), theWorkspace.ID);
            }
            catch(Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                item = new ExplorerRepositoryResult(ExecStatus.Fail, e.Message);
            }
            var serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(item);
        }

        public override  DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override  string HandlesType() => "RenameFolderService";
    }
}

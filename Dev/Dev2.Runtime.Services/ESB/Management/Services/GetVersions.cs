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
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Common.Wrappers;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetVersions : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }

        private IServerVersionRepository _serverExplorerRepository;

        public string HandlesType()
        {
            return "GetVersions";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                if (values == null)
                {
                    throw new ArgumentNullException(nameof(values));
                }
                if (!values.ContainsKey("resourceId"))
                {
                    // ReSharper disable NotResolvedInText
                    throw new ArgumentNullException(ErrorResource.NoResourceIdInTheIncomingData);
                    // ReSharper restore NotResolvedInText
                }
                var id = Guid.Parse(values["resourceId"].ToString());
                Dev2Logger.Info("Get Versions. " + id);
                var item = ServerVersionRepo.GetVersions(id);
                return serializer.SerializeToBuilder(item);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
                IExplorerRepositoryResult error = new ExplorerRepositoryResult(ExecStatus.Fail, e.Message);
                return serializer.SerializeToBuilder(error);
            }
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceId ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }

        public IServerVersionRepository ServerVersionRepo
        {
            get { return _serverExplorerRepository ?? new ServerVersionRepository(new VersionStrategy(), ResourceCatalog.Instance, new DirectoryWrapper(), EnvironmentVariables.GetWorkspacePath(GlobalConstants.ServerWorkspaceID), new FileWrapper()); }
            set { _serverExplorerRepository = value; }
        }
    }
}

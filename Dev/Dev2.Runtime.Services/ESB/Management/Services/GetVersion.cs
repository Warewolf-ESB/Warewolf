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
using Dev2.Runtime.Interfaces;
using Dev2.Services.Security;
using Dev2.Util;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetVersion : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }

        #region Implementation of ISpookyLoadable<string>
        private IServerVersionRepository _serverExplorerRepository;
        IResourceCatalog _resourceCatalog   ;

        public string HandlesType()
        {
            return "GetVersion";
        }

        #endregion

        #region Implementation of IEsbManagementEndpoint

        /// <summary>
        /// Executes the service
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="theWorkspace">The workspace.</param>
        /// <returns></returns>
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                var res = new ExecuteMessage { HasError = false };
                if (values == null)
                {
                    throw new ArgumentNullException("values");
                }
                if (!values.ContainsKey("versionInfo"))
                {
// ReSharper disable NotResolvedInText
                    throw new ArgumentNullException(ErrorResource.NoResourceIdInTheIncomingData);
// ReSharper restore NotResolvedInText
                }
               
                var version = serializer.Deserialize<IVersionInfo>(values["versionInfo"]);
                Dev2Logger.Info("Get Version. " + version);
                StringBuilder tmp;
                Guid resourceId = Guid.Empty;
                values.TryGetValue("resourceId", out tmp);
                if (tmp != null)
                {
                    resourceId = Guid.Parse(tmp.ToString());
                }
                var resourcePath = ResourceCatalog.GetResourcePath(theWorkspace.ID, resourceId);
                var result = ServerVersionRepo.GetVersion(version, resourcePath);
                res.Message.Append(result);
                Dev2XamlCleaner dev2XamlCleaner = new Dev2XamlCleaner();
                res.Message = dev2XamlCleaner.StripNaughtyNamespaces(res.Message);


                return serializer.SerializeToBuilder(res);

            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
                IExplorerRepositoryResult error = new ExplorerRepositoryResult(ExecStatus.Fail, e.Message);
                return serializer.SerializeToBuilder(error);
            }
        }

        /// <summary>
        /// Creates the service entry.
        /// </summary>
        /// <returns></returns>
        public DynamicService CreateServiceEntry()
        {
            var serviceAction = new ServiceAction { Name = HandlesType(), SourceMethod = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService };

            var serviceEntry = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><ResourceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            serviceEntry.Actions.Add(serviceAction);

            return serviceEntry;
        }

        #endregion
        public IServerVersionRepository ServerVersionRepo
        {
            get { return _serverExplorerRepository ?? new ServerVersionRepository(new VersionStrategy(), Hosting.ResourceCatalog.Instance, new DirectoryWrapper(), EnvironmentVariables.GetWorkspacePath(GlobalConstants.ServerWorkspaceID), new FileWrapper()); }
            set { _serverExplorerRepository = value; }
        }

        public IResourceCatalog ResourceCatalog
        {
            get { return _resourceCatalog ?? Hosting.ResourceCatalog.Instance; }
            set { _resourceCatalog = value; }
        }


    }
}

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
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Common.Wrappers;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using Warewolf.Data;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetVersion : DefaultEsbManagementEndpoint
    {
        IServerVersionRepository _serverExplorerRepository;
        IResourceCatalog _resourceCatalog;

        #region Implementation of IEsbManagementEndpoint

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
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
                    throw new ArgumentNullException(ErrorResource.NoResourceIdInTheIncomingData);
                }
               
                var version = serializer.Deserialize<IVersionInfo>(values["versionInfo"]);
                Dev2Logger.Info("Get Version. " + version, GlobalConstants.WarewolfInfo);
                var resourceId = Guid.Empty;
                values.TryGetValue("resourceId", out StringBuilder tmp);
                if (tmp != null)
                {
                    resourceId = Guid.Parse(tmp.ToString());
                }
                var resourcePath = ResourceCatalog.GetResourcePath(theWorkspace.ID, resourceId);
                var result = ServerVersionRepo.GetVersion(version, resourcePath);
                res.Message.Append(result);
                var dev2XamlCleaner = new Dev2XamlCleaner();
                res.Message = dev2XamlCleaner.StripNaughtyNamespaces(res.Message);
                
                return serializer.SerializeToBuilder(res);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                IExplorerRepositoryResult error = new ExplorerRepositoryResult(ExecStatus.Fail, e.Message);
                return serializer.SerializeToBuilder(error);
            }
        }

        #endregion

        public IServerVersionRepository ServerVersionRepo
        {
            get { return _serverExplorerRepository ?? new ServerVersionRepository(new VersionStrategy(), Hosting.ResourceCatalog.Instance, new DirectoryWrapper(), EnvironmentVariables.GetWorkspacePath(GlobalConstants.ServerWorkspaceID), new FileWrapper(), new FilePathWrapper()); }
            set { _serverExplorerRepository = value; }
        }

        public IResourceCatalog ResourceCatalog
        {
            get => _resourceCatalog ?? Hosting.ResourceCatalog.Instance;
            set => _resourceCatalog = value;
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "GetVersion";
    }
}

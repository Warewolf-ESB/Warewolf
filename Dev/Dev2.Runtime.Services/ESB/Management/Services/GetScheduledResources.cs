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
using System.Collections.ObjectModel;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Scheduler;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetScheduledResources : DefaultEsbManagementEndpoint
    {
        private IServerSchedulerFactory _schedulerFactory;
        ISecurityWrapper _securityWrapper;
        private IResourceCatalog _catalog;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {

                Dev2Logger.Info("Get Scheduled Resources", GlobalConstants.WarewolfInfo);
                ObservableCollection<IScheduledResource> resources;
                using(var model = SchedulerFactory.CreateModel(GlobalConstants.SchedulerFolderId, SecurityWrapper))
                {
                    resources = model.GetScheduledResources();
                }

                var sb = new StringBuilder(JsonConvert.SerializeObject(resources, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                }));
                return sb;
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                throw;
            }
        }
        public IServerSchedulerFactory SchedulerFactory
        {
            get => _schedulerFactory ?? new ServerSchedulerFactory(a => ResourceCatalogue.GetResourcePath(GlobalConstants.ServerWorkspaceID, a.ResourceId));
            set => _schedulerFactory = value;
        }

        public IResourceCatalog ResourceCatalogue
        {
            get => _catalog ?? ResourceCatalog.Instance;
            set => _catalog = value;
        }

        public ISecurityWrapper SecurityWrapper
        {
            get => _securityWrapper ?? new SecurityWrapper(ServerAuthorizationService.Instance);
            set => _securityWrapper = value;
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList></DataList>");

        public override string HandlesType() => "GetScheduledResources";
    }
}

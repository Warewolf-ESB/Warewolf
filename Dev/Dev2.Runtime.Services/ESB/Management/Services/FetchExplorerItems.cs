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
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using KGySoft.CoreLibraries;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchExplorerItems : DefaultEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2Logger.Info("Fetch Explorer Items", GlobalConstants.WarewolfInfo);

            var serializer = new Dev2JsonSerializer();
            try
            {
                if (values == null)
                {
                    throw new ArgumentNullException(nameof(values));
                }
                values.TryGetValue("ReloadResourceCatalogue", out StringBuilder tmp);
                var reloadResourceCatalogueString = "";
                if (tmp != null)
                {
                    reloadResourceCatalogueString = tmp.ToString();
                }
                var reloadResourceCatalogue = false;

                if (!string.IsNullOrEmpty(reloadResourceCatalogueString) && !bool.TryParse(reloadResourceCatalogueString, out reloadResourceCatalogue))
                {
                    reloadResourceCatalogue = false;
                }

                if (reloadResourceCatalogue)
                {
                    var exeManager = CustomContainer.Get<IExecutionManager>();
                    if (exeManager != null && !exeManager.IsRefreshing)
                    {
                        exeManager.StartRefresh();
                        ResourceCatalog.Instance.Reload();
                        exeManager.StopRefresh();
                    }
                }

                // ResourceType
                values.TryGetValue("resourceTypeFilter", out StringBuilder tmp_resourceType);
                var resourceTypeFilter = "all";
                if (tmp_resourceType != null)
                {
                    resourceTypeFilter = tmp_resourceType.ToString();
                }

                return serializer.SerializeToBuilder(GetExplorerItems(serializer, reloadResourceCatalogue, resourceTypeFilter));
            }
            catch (Exception e)
            {
                Dev2Logger.Info("Fetch Explorer Items Error", e, GlobalConstants.WarewolfInfo);
                IExplorerRepositoryResult error = new ExplorerRepositoryResult(ExecStatus.Fail, e.Message);
                return serializer.SerializeToBuilder(error);
            }
            finally
            {
                var exeManager = CustomContainer.Get<IExecutionManager>();
                exeManager?.StopRefresh();
            }
        }

        CompressedExecuteMessage GetExplorerItems(Dev2JsonSerializer serializer, bool reloadResourceCatalogue, string resourceType)
        {
            IExplorerItem item = ServerExplorerRepo.Load(GlobalConstants.ServerWorkspaceID, reloadResourceCatalogue);

            if (!resourceType.Equals("all"))
                item = FilterResourcesByType(item, resourceType.ToLower(), true);

            var message = new CompressedExecuteMessage();
            message.SetMessage(serializer.Serialize(item));
            return message;
        }

        public static IExplorerItem FilterResourcesByType(IExplorerItem root, string filterType, bool isRoot = false)
        {
            if (root == null) return null;

            // Recursively filter the children
            List<IExplorerItem> filteredChildren = new List<IExplorerItem>();
            if (root.Children != null)
            {
                foreach (var child in root.Children)
                {
                    var filteredChild = FilterResourcesByType(child, filterType);
                    if (filteredChild != null)
                        filteredChildren.Add(filteredChild);
                }
            }
            // If the current item is of the filter type or a folder with filtered children, keep it
            if (isRoot || root.ResourceType.ToLower() == filterType || (root.ResourceType.ToLower() == "folder" && filteredChildren.Count > 0))
            {
                root.Children = filteredChildren;
                return root;
            }

            // Otherwise, return null to exclude this item
            return null;
        }

        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get { return _serverExplorerRepository ?? ServerExplorerRepository.Instance; }
            set { _serverExplorerRepository = value; }
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FetchExplorerItemsService";
    }
}

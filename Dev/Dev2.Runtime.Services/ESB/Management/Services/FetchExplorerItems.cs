/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

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
                return serializer.SerializeToBuilder(GetExplorerItems(serializer, reloadResourceCatalogue));
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

        CompressedExecuteMessage GetExplorerItems(Dev2JsonSerializer serializer, bool reloadResourceCatalogue)
        {
            var item = ServerExplorerRepo.Load(GlobalConstants.ServerWorkspaceID, reloadResourceCatalogue);
            var message = new CompressedExecuteMessage();
            message.SetMessage(serializer.Serialize(item));
            return message;
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

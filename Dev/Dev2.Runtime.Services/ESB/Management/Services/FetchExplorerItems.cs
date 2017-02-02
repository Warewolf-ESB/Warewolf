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
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Services.Security;
using Dev2.Workspaces;
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchExplorerItems : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }

        private IExplorerServerResourceRepository _serverExplorerRepository;
      
        public string HandlesType()
        {
            return "FetchExplorerItemsService";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2Logger.Info("Fetch Explorer Items");

            var serializer = new Dev2JsonSerializer();
            try
            {
                if (values == null)
                {
                    throw new ArgumentNullException(nameof(values));
                }
                StringBuilder tmp;
                values.TryGetValue("ReloadResourceCatalogue", out tmp);
                string reloadResourceCatalogueString = "";
                if (tmp != null)
                {
                    reloadResourceCatalogueString = tmp.ToString();
                }
                bool reloadResourceCatalogue = false;
                if (!string.IsNullOrEmpty(reloadResourceCatalogueString))
                {

                    if (!bool.TryParse(reloadResourceCatalogueString, out reloadResourceCatalogue))
                    {
                        reloadResourceCatalogue = false;
                    }
                }
                if (reloadResourceCatalogue)
                {
                    ResourceCatalog.Instance.Reload();
                }
                var item = ServerExplorerRepo.Load(GlobalConstants.ServerWorkspaceID, reloadResourceCatalogue);
                CompressedExecuteMessage message = new CompressedExecuteMessage();
                message.SetMessage(serializer.Serialize(item));
                return serializer.SerializeToBuilder(message);
            }
            catch (Exception e)
            {
                Dev2Logger.Info("Fetch Explorer Items Error", e);
                IExplorerRepositoryResult error = new ExplorerRepositoryResult(ExecStatus.Fail, e.Message);
                return serializer.SerializeToBuilder(error);
            }
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

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

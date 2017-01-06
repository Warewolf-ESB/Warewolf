/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Services.Security;
using Warewolf.Core;

// ReSharper disable UnusedMember.Global

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchDbActions : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }

        public string HandlesType()
        {
            return "FetchDbActions";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                var dbSource = serializer.Deserialize<IDbSource>(values["source"]);
                // ReSharper disable MaximumChainedReferences
                ServiceModel.Services services = new ServiceModel.Services();

                var src = ResourceCatalog.Instance.GetResource<DbSource>(GlobalConstants.ServerWorkspaceID, dbSource.Id);
                src.ReloadActions = dbSource.ReloadActions;
                if (dbSource.Type == enSourceType.ODBC)
                {
                    DbSource db = new DbSource();
                    db.DatabaseName = dbSource.DbName;
                    db.ResourceID = dbSource.Id;
                    db.ServerType = dbSource.Type;
                    db.ResourceName = dbSource.Name;

                    var methods = services.FetchMethods(src).Select(method => CreateDbAction(method, src)).OrderBy(a => a.Name);
                    return serializer.SerializeToBuilder(new ExecuteMessage
                    {
                        HasError = false,
                        Message = serializer.SerializeToBuilder(methods)
                    });
                }
                else
                {
                    var methods = services.FetchMethods(src).Select(method => CreateDbAction(method, src)).OrderBy(a => a.Name);
                    return serializer.SerializeToBuilder(new ExecuteMessage
                    {
                        HasError = false,
                        Message = serializer.SerializeToBuilder(methods)
                    });
                }

                // ReSharper restore MaximumChainedReferences
            }
            catch (Exception e)
            {
                return serializer.SerializeToBuilder(new ExecuteMessage
                {
                    HasError = true,
                    Message = new StringBuilder(e.Message)
                });
            }
        }

        private DbAction CreateDbAction(ServiceMethod a, DbSource src)
        {
            // ReSharper disable MaximumChainedReferences
            var inputs = a.Parameters.Select(b => new ServiceInput(b.Name, b.DefaultValue ?? "") as IServiceInput).ToList();
            // ReSharper restore MaximumChainedReferencse
            return new DbAction
            {
                Name = a.Name,
                Inputs = inputs,
                SourceId = src.ResourceID
            };
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }

        public ResourceCatalog Resources => ResourceCatalog.Instance;
    }
}
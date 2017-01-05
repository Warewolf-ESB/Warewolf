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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SaveDbSourceSource : IEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Contribute;
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Save Resource Service");
                StringBuilder resourceDefinition;

                values.TryGetValue("DbSource", out resourceDefinition);

                IDbSource src = serializer.Deserialize<DbSourceDefinition>(resourceDefinition);
                if (src.Path.EndsWith("\\"))
                    src.Path = src.Path.Substring(0, src.Path.LastIndexOf("\\", StringComparison.Ordinal));
                var res = new DbSource
                {
                    AuthenticationType = src.AuthenticationType,
                    Server = src.ServerName,
                    Password = src.Password,
                    ServerType = src.Type,
                    UserID = src.UserName,
                    ResourceID = src.Id,
                    DatabaseName = src.DbName,
                    ResourceName = src.Name,
                    ResourceType = src.Type.ToString()
                };
                var con = new DbSources();
                var result = con.DoDatabaseValidation(res);

                if (result.IsValid)
                {
                    ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, res, src.Path);
                    ServerExplorerRepo.UpdateItem(res);

                    msg.HasError = false;
                }
                else
                {
                    msg.HasError = false;
                    msg.Message = new StringBuilder(res.IsValid ? "" : result.ErrorMessage);
                }



            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err);

            }

            return serializer.SerializeToBuilder(msg);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><ServerSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }
        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get { return _serverExplorerRepository ?? ServerExplorerRepository.Instance; }
            set { _serverExplorerRepository = value; }
        }
        public string HandlesType()
        {
            return "SaveDbSourceService";
        }
    }
}

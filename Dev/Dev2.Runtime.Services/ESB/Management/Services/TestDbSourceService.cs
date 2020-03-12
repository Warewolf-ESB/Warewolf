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
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{

    public class TestDbSourceService : EsbManagementEndpointBase
    {
        readonly IDbSources _dbSources;

        public TestDbSourceService()
            : this(new DbSources())
        {

        }

        public TestDbSourceService(IDbSources dbSources) => _dbSources = dbSources;

        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public override AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Test DB Connection Service", GlobalConstants.WarewolfInfo);

                values.TryGetValue("DbSource", out StringBuilder resourceDefinition);

                IDbSource src = serializer.Deserialize<DbSourceDefinition>(resourceDefinition);
                DatabaseValidationResult result = null;
                Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.OrginalExecutingUser, () =>
                {
                    result = _dbSources.DoDatabaseValidation(new DbSource
                    {
                        AuthenticationType = src.AuthenticationType,
                        Server = src.ServerName,
                        Password = src.Password,
                        ServerType = src.Type,
                        ConnectionTimeout = src.ConnectionTimeout,
                        UserID = src.UserName
                    });

                });
                if (result == null)
                {
                    result = new DatabaseValidationResult { ErrorMessage = "Problem testing connection.", IsValid = false };
                }

                msg.Message = new StringBuilder(result.IsValid ? serializer.Serialize(result.DatabaseList) : result.ErrorMessage);
                msg.HasError = !result.IsValid;

            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);

            }

            return serializer.SerializeToBuilder(msg);
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><DbSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "TestDbSourceService";
    }
}

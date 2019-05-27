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
using System.Text;
using System.Threading;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchServerPermissions : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                Dev2Logger.Info("Find Server User Name", GlobalConstants.WarewolfInfo);
                var user = Thread.CurrentPrincipal;
                var permissionsMemo = new PermissionsModifiedMemo
                {
                    ModifiedPermissions = ServerAuthorizationService.Instance.GetPermissions(user),
                    ServerID = HostSecurityProvider.Instance.ServerID
                };
                var serializer = new Dev2JsonSerializer();
                return serializer.SerializeToBuilder(permissionsMemo);
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FetchServerPermissions";
    }
}
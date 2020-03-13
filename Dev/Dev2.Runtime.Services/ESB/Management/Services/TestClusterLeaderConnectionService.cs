/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Text;
using Warewolf.Service;
using WarewolfCOMIPC.Client;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class TestClusterLeaderConnectionService : EsbManagementEndpointBase
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {

            var serializer = new Dev2JsonSerializer();
            var clusterResult = Execute(values);
            return serializer.SerializeToBuilder(clusterResult);
        }
        static TestClusterResult Execute(Dictionary<string, StringBuilder> values)
        {
            try
            {
                Dev2Logger.Info("Test Cluster Leader Connection Service", GlobalConstants.WarewolfInfo);
                values.TryGetValue("LeaderServerKey", out StringBuilder submittedKey);

                var clusterSettings = Config.Cluster.Get();
                var isValidKey = clusterSettings.Key == submittedKey?.ToString();
                if (isValidKey)
                {
                    return new TestClusterResult
                    {
                        HasError = false,
                        Success = true,
                    };
                }

                return new TestClusterResult
                {
                    HasError = true,
                    Message = new StringBuilder("the cluster key provided does not match"),
                };
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                return new TestClusterResult
                {
                    HasError = true,
                    Message = new StringBuilder(err.Message),
                };
            }
        }

        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public override AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => Cluster.TestClusterLeaderConnection;
    }
}

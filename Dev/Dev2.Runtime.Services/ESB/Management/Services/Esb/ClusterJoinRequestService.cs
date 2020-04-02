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
using Dev2.Common.Serializers;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Warewolf.Client;

namespace Dev2.Runtime.ESB.Management.Services.Esb
{
    public class ClusterJoinRequestService : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var response = new ClusterJoinResponse();
            if (values.TryGetValue(Warewolf.Service.Cluster.ClusterJoinRequest.Key, out var keySb))
            {
                var key = keySb.ToString();
                if (key == Config.Cluster.Key)
                {
                    response.Token = Guid.NewGuid();
                }
            }
            var serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(response);
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Type ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => nameof(Warewolf.Service.Cluster.ClusterJoinRequest);
    }
}
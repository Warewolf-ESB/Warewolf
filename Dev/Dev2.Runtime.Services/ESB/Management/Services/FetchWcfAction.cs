﻿#pragma warning disable
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
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchWcfAction : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                var source = serializer.Deserialize<IWcfServerSource>(values["WcfSource"]);

                var src = ResourceCatalog.Instance.GetResource<WcfSource>(GlobalConstants.ServerWorkspaceID, source.Id);

                var methods = src.GetServiceActions(source);

                return serializer.SerializeToBuilder(new ExecuteMessage
                {
                    HasError = false,
                    Message = serializer.SerializeToBuilder(methods)
                });
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

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FetchWcfAction";
    }
}

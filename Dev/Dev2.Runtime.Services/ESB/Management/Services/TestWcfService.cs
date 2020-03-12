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
﻿using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.Interfaces;
using Unlimited.Framework.Converters.Graph.Ouput;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class TestWcfService : EsbManagementEndpointBase
    {
        IResourceCatalog _rescat;

        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public override AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Test Wcf Service", GlobalConstants.WarewolfInfo);

                values.TryGetValue("wcfService", out StringBuilder resourceDefinition);
                var service = serializer.Deserialize<IWcfService>(resourceDefinition);

                var source = ResourceCatalog.Instance.GetResource<WcfSource>(GlobalConstants.ServerWorkspaceID, service.Source.Id);
                var parameters = service.Inputs?.Select(a => new MethodParameter { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value, TypeName = a.TypeName }).ToList() ?? new List<MethodParameter>();

                var res = new WcfService
                {
                    Method = new ServiceMethod(service.Action.Method, service.Name, parameters, new OutputDescription(), new List<MethodOutput>(), service.Action.Method),
                    ResourceName = service.Name,
                    ResourceID = service.Id,
                    Source = source
                };

                var services = new ServiceModel.Services();

                var output = services.WcfTest(res, GlobalConstants.ServerWorkspaceID, Guid.Empty);

                msg.HasError = false;
                msg.Message = serializer.SerializeToBuilder(new RecordsetListWrapper() { Description = output.Description, RecordsetList = output });
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }

            return serializer.SerializeToBuilder(msg);
        }

        public IResourceCatalog ResourceCatalogue
        {
            get => _rescat ?? ResourceCatalog.Instance;
            set => _rescat = value;
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><PluginService ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "TestWcfService";
    }
}
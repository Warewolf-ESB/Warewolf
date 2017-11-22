﻿using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class TestWebserviceSourceService : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            try
            {

                Dev2Logger.Info("Test WebserviceSource", GlobalConstants.WarewolfInfo);

                values.TryGetValue("WebserviceSource", out StringBuilder resourceDefinition);

                var src = serializer.Deserialize<WebServiceSourceDefinition>(resourceDefinition);
                var con = new WebSources();
                var result = con.Test(new WebSource
                {
                    Address = src.HostName,
                    DefaultQuery = src.DefaultQuery,
                    AuthenticationType = src.AuthenticationType,
                    UserName = src.UserName,
                    Password = src.Password
                });


                msg.HasError = false;
                msg.Message = new StringBuilder(result.IsValid ? serializer.Serialize(result.Result) : result.ErrorMessage);
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

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><WebserviceSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "TestWebserviceSource";
    }
}

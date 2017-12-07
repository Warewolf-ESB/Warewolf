﻿using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class TestEmailServiceSource : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Save Resource Service", GlobalConstants.WarewolfInfo);

                values.TryGetValue("EmailServiceSource", out StringBuilder resourceDefinition);

                IEmailServiceSource src = serializer.Deserialize<EmailServiceSourceDefinition>(resourceDefinition);
                var con = new EmailSource
                {
                    Host = src.HostName,
                    UserName = src.UserName,
                    Password = src.Password,
                    Port = src.Port,
                    EnableSsl = src.EnableSsl,
                    Timeout = src.Timeout
                };
                try
                {
                    con.Send(new MailMessage(src.EmailFrom,src.EmailTo,"Test Email Service Source","Test message from Warewolf for Email Service Source"));
                }
                catch (SmtpException e)
                {
                    msg.HasError = false;
                    msg.Message = new StringBuilder( e.Message);
                    return serializer.SerializeToBuilder(msg);
                }
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }

            return serializer.SerializeToBuilder(msg);
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><EmailServiceSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "TestEmailServiceSource";
    }
}
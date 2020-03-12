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
using Warewolf.Resource.Messages;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class TestEmailServiceSource : EsbManagementEndpointBase
    {
        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public override AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
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
                    var mailMessage = new MailMessage(src.EmailFrom, src.EmailTo, Messages.Test_EmailServerSource_Header, Messages.Test_EmailServerSource_EmailBody);
                    con.Send(mailMessage);
                }
                catch (SmtpException e)
                {
                    msg.HasError = true;
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

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><EmailServiceSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "TestEmailServiceSource";
    }
}
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Email;
using Dev2.Common.Interfaces.ServerDialogue;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Adds a resource
    /// </summary>
    public class TestEmailServiceSource : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Log.Info("Save Resource Service");
                StringBuilder resourceDefinition;

                values.TryGetValue("EmailServiceSource", out resourceDefinition);

                IEmailServiceSource src = serializer.Deserialize<EmailServiceSourceDefinition>(resourceDefinition);
                EmailSource con = new EmailSource();
                con.Host = src.HostName;
                con.UserName = src.UserName;
                con.Password = src.Password;
                con.Port = src.Port;
                con.EnableSsl = src.EnableSsl;
                con.Timeout = src.Timeout;
                try
                {
                    con.Send(new MailMessage(src.EmailFrom,src.EmailTo,"Test Email Service Source","Test message from Warewolf for Email Service Source"));
                }
                catch (SmtpException e)
                {
                    msg.HasError = true;
                    msg.Message = new StringBuilder(e.Message);
                }
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Log.Error(err);
            }

            return serializer.SerializeToBuilder(msg);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><EmailServiceSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "TestEmailServiceSource";
        }
    }
}
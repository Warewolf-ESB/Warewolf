using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel
{
    // PBI 953 - 2013.05.16 - TWR - Created
    public class EmailSources : ExceptionManager
    {
        readonly IResourceCatalog _resourceCatalog;

        #region CTOR

        public EmailSources()
            : this(ResourceCatalog.Instance)
        {
        }

        public EmailSources(IResourceCatalog resourceCatalog)
        {
            if(resourceCatalog == null)
            {
                throw new ArgumentNullException("resourceCatalog");
            }
            _resourceCatalog = resourceCatalog;
        }

        #endregion

        #region Get

        // POST: Service/EmailSources/Get
        public EmailSource Get(string resourceId, Guid workspaceId, Guid dataListId)
        {
            var result = new EmailSource();
            try
            {
                var xmlStr = Resources.ReadXml(workspaceId, ResourceType.EmailSource, resourceId);
                if(!string.IsNullOrEmpty(xmlStr))
                {
                    var xml = XElement.Parse(xmlStr);
                    result = new EmailSource(xml);
                }
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        #endregion

        #region Save

        // POST: Service/EmailSources/Save
        public string Save(string args, Guid workspaceId, Guid dataListId)
        {
            try
            {
                var source = JsonConvert.DeserializeObject<EmailSource>(args);

                _resourceCatalog.SaveResource(workspaceId, source);
                if(workspaceId != GlobalConstants.ServerWorkspaceID)
                {
                    _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, source);
                }

                return source.ToString();
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                return new ValidationResult { IsValid = false, ErrorMessage = ex.Message }.ToString();
            }
        }

        #endregion

        #region Test

        // POST: Service/EmailSources/Test
        public ValidationResult Test(string args, Guid workspaceId, Guid dataListId)
        {
            try
            {
                var source = JsonConvert.DeserializeObject<EmailSource>(args);
                return CanConnectServer(source);
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                return new ValidationResult { IsValid = false, ErrorMessage = ex.Message };
            }
        }

        #endregion

        #region CanConnectServer

        ValidationResult CanConnectServer(EmailSource emailSource)
        {
            try
            {
                var userParts = emailSource.UserName.Split(new[] { '@' });

                var smtp = new SmtpClient(emailSource.Host, emailSource.Port)
                {
                    Credentials = new NetworkCredential(userParts[0], emailSource.Password),
                    EnableSsl = emailSource.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = emailSource.Timeout
                };

                try
                {
                    smtp.Send(emailSource.TestFromAddress, emailSource.TestToAddress, "Test Message", "This is a test message");
                    return new ValidationResult();
                }
                finally
                {
                    smtp.Dispose();
                }
            }
            catch(SmtpException sex)
            {
                RaiseError(sex);
                var message = sex.Message;
                if(sex.StatusCode == SmtpStatusCode.MustIssueStartTlsFirst && sex.Message.Contains("Learn more at"))
                {
                    message = message.Replace(" Learn more at", "");
                }
                var errors = new StringBuilder();
                errors.AppendFormat("{0} ", message);
                Exception ex = sex.InnerException;
                while(ex != null)
                {
                    errors.AppendFormat("{0} ", ex.Message);
                    ex = ex.InnerException;
                }
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = errors.ToString()
                };
            }
        }

        #endregion
    }
}
#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Xml.Linq;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
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
                throw new ArgumentNullException(nameof(resourceCatalog));
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
                var xmlStr = ResourceCatalog.Instance.GetResourceContents(workspaceId, Guid.Parse(resourceId)).ToString();
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
                var userParts = emailSource.UserName.Split('@');

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
                var ex = sex.InnerException;
                while (ex != null)
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

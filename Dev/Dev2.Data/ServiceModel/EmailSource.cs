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
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Warewolf.Security.Encryption;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class EmailSource : Resource, IResourceSource, IEmailSource
    {
        public static readonly int DefaultTimeout = 100000; // (100 seconds)
        public static readonly int DefaultPort = 25;
        public static readonly int SslPort = 465;
        public static readonly int TlsPort = 587;

        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public int Timeout { get; set; }

        /// <summary>
        /// This must NEVER be persisted - here for JSON transport only!
        /// </summary>
        public string TestFromAddress { get; set; }

        /// <summary>
        /// This must NEVER be persisted - here for JSON transport only!
        /// </summary>
        public string TestToAddress { get; set; }

        public new string DataList
        {
            get
            {
                var stringBuilder = base.DataList;
                return stringBuilder?.ToString();
            }
            set
            {
                base.DataList = value.ToStringBuilder();
            }
        }

        public EmailSource()
        {
            ResourceID = Guid.Empty;
            ResourceType = "EmailSource";
            Timeout = DefaultTimeout;
            Port = DefaultPort;
        }

        public EmailSource(XElement xml)
            : base(xml)
        {
            ResourceType = "EmailSource";
            Timeout = DefaultTimeout;
            Port = DefaultPort;

            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Host", string.Empty },
                { "UserName", string.Empty },
                { "Password", string.Empty },
                { "Port", string.Empty },
                { "EnableSsl", string.Empty },
                { "Timeout", string.Empty },
            };

            var conString = xml.AttributeSafe("ConnectionString");
            var connectionString = conString.CanBeDecrypted() ? DpapiWrapper.Decrypt(conString) : conString;
            ParseProperties(connectionString, properties);

            Host = properties["Host"];
            UserName = properties["UserName"];
            Password = properties["Password"];

            Port = Int32.TryParse(properties["Port"], out int port) ? port : DefaultPort;

            EnableSsl = bool.TryParse(properties["EnableSsl"], out bool enableSsl) && enableSsl;

            Timeout = Int32.TryParse(properties["Timeout"], out int timeout) ? timeout : DefaultTimeout;
        }
        //TODO: Add SmtpClientFactory so that we can test it without failing
        public void Send(MailMessage mailMessage)
        {
            var userParts = UserName.Split('@');
            using (var smtp = new SmtpClient(Host, Port)
            {
                Credentials = new NetworkCredential(userParts[0], Password),
                EnableSsl = EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = Timeout
            })
            {
                smtp.Send(mailMessage);
            }
        }

        public override XElement ToXml()
        {
            var result = base.ToXml();
            var connectionString = string.Join(";",
                $"Host={Host}",
                $"UserName={UserName}",
                $"Password={Password}",
                $"Port={Port}",
                $"EnableSsl={EnableSsl}",
                $"Timeout={Timeout}"
                );

            result.Add(
                new XAttribute("ConnectionString", DpapiWrapper.Encrypt(connectionString)),
                new XAttribute("Type", GetType().Name),
                new XElement("TypeOf", ResourceType)
                );

            return result;
        }

        public override bool IsSource => true;
        public override bool IsService => false;
        public override bool IsFolder => false;
        public override bool IsReservedService => false;
        public override bool IsServer => false;
        public override bool IsResourceVersion => false;
    }
}

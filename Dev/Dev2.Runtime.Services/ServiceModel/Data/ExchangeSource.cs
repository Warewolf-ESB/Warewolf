#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using Dev2.Common.Common;
using Dev2.Common.Exchange;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Warewolf.Security.Encryption;
using ExchangeService = Microsoft.Exchange.WebServices.Data.ExchangeService;

namespace Dev2.Runtime.ServiceModel.Data
{
    [Serializable]
    public class ExchangeSource : Resource, IExchange, IResourceSource
    {
        ExchangeService _exchangeService;

        IExchangeEmailSender _emailSender;

        public static readonly int DefaultTimeout = 100000;
        public static readonly int DefaultPort = 25;
        public static readonly int SslPort = 465;
        public static readonly int TlsPort = 587;

        public override bool IsSource => true;

        public override bool IsService => false;
        public override bool IsFolder => false;
        public override bool IsReservedService => false;
        public override bool IsServer => false;
        public override bool IsResourceVersion => false;

        #region Properties

        public string AutoDiscoverUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public enSourceType Type { get; set; }
        public string Path { get; set; }
        public int Timeout { get; set; }
        public string EmailFrom { get; set; }
        public string EmailTo { get; set; }

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

        #endregion Properties

        #region CTOR

        public ExchangeSource(IExchangeEmailSender sender)
        {
            _emailSender = sender;
            ResourceType = "ExchangeSource";
        }

        public ExchangeSource()
        {
            ResourceID = Guid.Empty;
            ResourceType = "ExchangeSource";
            Timeout = DefaultTimeout;
        }

        public ExchangeSource(XElement xml)
            : base(xml)
        {
            ResourceType = "ExchangeSource";
            Timeout = DefaultTimeout;

            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "AutoDiscoverUrl", string.Empty },
                { "UserName", string.Empty },
                { "Password", string.Empty },
                { "Timeout", string.Empty },
            };

            var conString = xml.AttributeSafe("ConnectionString");
            var connectionString = conString.CanBeDecrypted() ? DpapiWrapper.Decrypt(conString) : conString;
            ParseProperties(connectionString, properties);

            AutoDiscoverUrl = properties["AutoDiscoverUrl"];
            UserName = properties["UserName"];
            Password = properties["Password"];

            Timeout = Int32.TryParse(properties["Timeout"], out int timeout) ? timeout : DefaultTimeout;
        }

        public void Send(IExchangeEmailSender emailSender, ExchangeTestMessage testMessage) => Send(emailSender, testMessage, true);
        public void Send(IExchangeEmailSender emailSender, ExchangeTestMessage testMessage, bool isHtml)
        {
            InitializeService();

            _emailSender = emailSender;

            var emailMessage = new EmailMessage(_exchangeService)
            {
                Body = testMessage.Body,
                Subject = testMessage.Subject
            };

            if (isHtml)
            {
                emailMessage.Body.BodyType = BodyType.HTML;
            }
            else
            {
                emailMessage.Body.BodyType = BodyType.Text;
            }

            foreach (var to in testMessage.Tos)
            {
                if (!string.IsNullOrEmpty(to))
                {
                    emailMessage.ToRecipients.Add(to);
                }
            }

            foreach (var cC in testMessage.CCs)
            {
                if (!string.IsNullOrEmpty(cC))
                {
                    emailMessage.CcRecipients.Add(cC);
                }
            }

            foreach (var bcC in testMessage.BcCs)
            {
                if (!string.IsNullOrEmpty(bcC))
                {
                    emailMessage.BccRecipients.Add(bcC);
                }
            }

            foreach (var att in testMessage.Attachments)
            {
                if (!string.IsNullOrEmpty(att))
                {
                    emailMessage.Attachments.AddFileAttachment(att);
                }
            }

            _emailSender.Send(_exchangeService, emailMessage);
        }

        #endregion CTOR

        void InitializeService()
        {
            _exchangeService = new ExchangeServiceFactory().Create();
        }

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            var connectionString = string.Join(";",
                string.Format("AutoDiscoverUrl={0}", AutoDiscoverUrl),
                string.Format("UserName={0}", UserName),
                string.Format("Password={0}", Password),
                string.Format("Timeout={0}", Timeout)
                );

            result.Add(
                new XAttribute("ConnectionString", DpapiWrapper.Encrypt(connectionString)),
                new XAttribute("Type", GetType().Name),
                new XElement("TypeOf", ResourceType)
                );

            return result;
        }

        #endregion ToXml

        public bool Equals(IExchangeSource other) => true;
    }

    public class ExchangeTestMessage
    {
        public ExchangeTestMessage()
        {
            Tos = new List<string>();
            CCs = new List<string>();
            BcCs = new List<string>();
            Attachments = new List<string>();
        }

        public List<string> Tos { get; set; }
        public List<string> CCs { get; set; }
        public List<string> BcCs { get; set; }
        public List<string> Attachments { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
    }
}
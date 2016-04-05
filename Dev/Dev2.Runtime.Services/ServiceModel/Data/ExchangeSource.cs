﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Exchange;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Microsoft.Exchange.WebServices.Data;
using Warewolf.Security.Encryption;
using ExchangeService = Microsoft.Exchange.WebServices.Data.ExchangeService;

namespace Dev2.Runtime.ServiceModel.Data
{
    [Serializable]
    public class ExchangeSource : Resource, IExchangeSource
    {
        private ExchangeService _exchangeService;
        // ReSharper disable once NotAccessedField.Local
        private IExchangeEmailSender _emailSender;

        public static int DefaultTimeout = 100000; // (100 seconds)
        public static int DefaultPort = 25;
        public static int SslPort = 465;
        public static int TlsPort = 587;

        #region Properties

        public string AutoDiscoverUrl { get; set; }
        public string Name { get; set; }
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
                return stringBuilder != null ? stringBuilder.ToString() : null;
            }
            set
            {
                base.DataList = value.ToStringBuilder();
            }
        }

        #endregion

        #region CTOR

        public ExchangeSource()
        {
            ResourceID = Guid.Empty;
            ResourceType = ResourceType.ExchangeSource;
            Timeout = DefaultTimeout;
        }

        public ExchangeSource(XElement xml)
            : base(xml)
        {
            ResourceType = ResourceType.ExchangeSource;
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

            int timeout;
            Timeout = Int32.TryParse(properties["Timeout"], out timeout) ? timeout : DefaultTimeout;
        }

        public void Send(IExchangeSource source, string message, string from, string to)
        {
            InitializeService();

            _emailSender = new ExchangeEmailSender(source);

            var emailMessage = new EmailMessage(_exchangeService) {Body = message};
            emailMessage.ToRecipients.Add(to);
           // _emailSender.Send(_exchangeService,emailMessage);
        }
        #endregion

        private void InitializeService()
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
                new XAttribute("Type", ResourceType),
                new XElement("TypeOf", ResourceType)
                );

            return result;
        }

        #endregion

        public bool Equals(IExchangeSource other)
        {
            return true;
        }
    }
}

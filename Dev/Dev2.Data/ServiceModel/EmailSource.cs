using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;

namespace Dev2.Runtime.ServiceModel.Data
{
    // PBI 953 - 2013.05.16 - TWR - Created
    public class EmailSource : Resource
    {
        public static int DefaultTimeout = 100000; // (100 seconds)
        public static int DefaultPort = 25;
        public static int SslPort = 465;
        public static int TlsPort = 587;

        #region Properties

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

        #endregion

        #region CTOR

        public EmailSource()
        {
            ResourceID = Guid.Empty;
            ResourceType = ResourceType.EmailSource;
            Timeout = DefaultTimeout;
            Port = DefaultPort;
        }

        public EmailSource(XElement xml)
            : base(xml)
        {
            ResourceType = ResourceType.EmailSource;
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

            ParseProperties(xml.AttributeSafe("ConnectionString"), properties);

            Host = properties["Host"];
            UserName = properties["UserName"];
            Password = properties["Password"];

            int port;
            Port = Int32.TryParse(properties["Port"], out port) ? port : DefaultPort;

            bool enableSsl;
            EnableSsl = bool.TryParse(properties["EnableSsl"], out enableSsl) && enableSsl;

            int timeout;
            Timeout = Int32.TryParse(properties["Timeout"], out timeout) ? timeout : DefaultTimeout;
        }

        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            var connectionString = string.Join(";",
                string.Format("Host={0}", Host),
                string.Format("UserName={0}", UserName),
                string.Format("Password={0}", Password),
                string.Format("Port={0}", Port),
                string.Format("EnableSsl={0}", EnableSsl),
                string.Format("Timeout={0}", Timeout)
                );

            result.Add(
                new XAttribute("ConnectionString", connectionString),
                new XAttribute("Type", enSourceType.EmailSource),
                new XElement("TypeOf", enSourceType.EmailSource)
                );

            return result;
        }

        #endregion

    }
}

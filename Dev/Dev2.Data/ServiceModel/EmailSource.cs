using System;
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

            var connectionString = xml.AttributeSafe("ConnectionString");
            var props = connectionString.Split(';');
            foreach(var p in props.Select(prop => prop.Split('=')).Where(p => p.Length >= 1))
            {
                switch(p[0].ToLowerInvariant())
                {
                    case "host":
                        Host = p[1];
                        break;
                    case "username":
                        UserName = p[1];
                        break;
                    case "password":
                        Password = p[1];
                        break;
                    case "port":
                        int port;
                        Port = Int32.TryParse(p[1], out port) ? port : DefaultPort;
                        break;
                    case "enablessl":
                        bool enableSsl;
                        EnableSsl = bool.TryParse(p[1], out enableSsl) && enableSsl;
                        break;
                    case "timeout":
                        int timeout;
                        Timeout = Int32.TryParse(p[1], out timeout) ? timeout : DefaultTimeout;
                        break;
                }
            }
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

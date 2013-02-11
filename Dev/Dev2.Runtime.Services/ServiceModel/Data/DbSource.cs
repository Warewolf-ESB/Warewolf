using System;
using System.Xml.Linq;
using Dev2.DynamicServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbSource : Resource
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public int Port { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationType AuthenticationType { get; set; }

        public string UserID { get; set; }
        public string Password { get; set; }

        #region ConnectionString - calculated

        public string ConnectionString
        {
            get
            {
                switch(ResourceType)
                {
                    case enSourceType.SqlDatabase:
                        return string.Format("Data Source={0}{2};Initial Catalog={1};{3}", Server, Database,
                            (Port > 0 ? "," + Port : string.Empty),
                            AuthenticationType == AuthenticationType.Windows
                                ? "Integrated Security=SSPI;"
                                : string.Format("User ID={0};Password={1};", UserID, Password));

                    case enSourceType.MySqlDatabase:
                        return string.Format("Server={0};{4}Database={1};Uid={2};Pwd={3};",
                            Server, Database, UserID, Password,
                            (Port > 0 ? string.Format("Port={0};", Port) : string.Empty));
                }
                return string.Empty;
            }
        }

        #endregion

        #region CTOR

        public DbSource()
        {
        }

        public DbSource(XElement xml)
            : base(xml)
        {
            Server = xml.AttributeSafe("Server");
            Database = xml.AttributeSafe("Database");

            int port;
            Port = Int32.TryParse(xml.AttributeSafe("Port"), out port) ? port : 0;

            AuthenticationType authType;
            AuthenticationType = Enum.TryParse(xml.AttributeSafe("AuthenticationType"), true, out authType) ? authType : AuthenticationType.User;

            UserID = xml.AttributeSafe("UserID");
            Password = xml.AttributeSafe("Password");
        }

        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            result.Add(new XAttribute("ConnectionString", ConnectionString));
            result.Add(new XAttribute("Server", Server));
            result.Add(new XAttribute("Database", Database));
            result.Add(new XAttribute("Port", Port));
            result.Add(new XAttribute("AuthenticationType", AuthenticationType));
            result.Add(new XAttribute("UserID", UserID));
            result.Add(new XAttribute("Password", Password));

            return result;
        }

        #endregion
    }
}

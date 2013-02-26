using System;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common.ServiceModel;
using Dev2.DynamicServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbSource : Resource
    {
        #region CTOR

        public DbSource()
        {
            ResourceType = ResourceType.DbSource;
        }

        public DbSource(XElement xml)
            : base(xml)
        {
            ResourceType = ResourceType.DbSource;
            Server = xml.AttributeSafe("Server");

            // Setup type include default port
            switch(xml.AttributeSafe("ServerType"))
            {
                case "SqlDatabase":
                    ServerType = enSourceType.SqlDatabase;
                    Port = 1433;
                    break;
                case "MySqlDatabase":
                    ServerType = enSourceType.MySqlDatabase;
                    break;
                default:
                    ServerType = enSourceType.Unknown;
                    break;
            }

            SourceDatabase = xml.AttributeSafe("SourceDatabase");
            ConnectionString = xml.AttributeSafe("ConnectionString");
            Port = int.Parse(xml.AttributeSafe("Port"));
            switch(xml.AttributeSafe("AuthenticationType"))
            {
                case "User":
                    AuthenticationType = AuthenticationType.User;
                    break;
                case "Windows":
                    AuthenticationType = AuthenticationType.Windows;
                    break;
                default:
                    AuthenticationType = AuthenticationType.Windows;
                    break;
            }
            UserName = xml.AttributeSafe("UserName");
            Password = xml.AttributeSafe("Password");

        }

        #endregion

        #region Properties

        [JsonConverter(typeof(StringEnumConverter))]
        public enSourceType ServerType { get; set; }

        public string Server { get; set; }

        public string SourceDatabase { get; set; }

        public int Port { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationType AuthenticationType { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        #endregion

        #region ConnectionString

        public string ConnectionString
        {
            //
            // PBI 8720: TODO: Make ConnectionString readonly 
            //
            get
            {
                switch(ServerType)
                {
                    case enSourceType.SqlDatabase:
                        return string.Format("Data Source={0};{1}{2}", Server, SourceDatabase==null?null:"Initial Catalog="+SourceDatabase+";",
                            //(Port > 0 ? ":" + Port : string.Empty),
                            AuthenticationType == AuthenticationType.Windows
                                ? "Integrated Security=SSPI;"
                                : string.Format("User ID={0};Password={1};", UserName, Password));

                    case enSourceType.MySqlDatabase:
                        return string.Format("Server={0};{4}Database={1};Uid={2};Pwd={3};",
                            Server, SourceDatabase, UserName, Password,
                            (Port > 0 ? string.Format("Port={0};", Port) : string.Empty));
                }
                return string.Empty;
            }
            set
            {
            } 
        }

        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            result.Add(new XAttribute("ConnectionString", ConnectionString ?? string.Empty));
            result.Add(new XAttribute("Server", Server ?? string.Empty));
            result.Add(new XAttribute("ServerType", ServerType));
            result.Add(new XAttribute("SourceDatabase", SourceDatabase ?? string.Empty));
            result.Add(new XAttribute("Port", Port));
            result.Add(new XAttribute("AuthenticationType", AuthenticationType));
            result.Add(new XAttribute("UserName", UserName ?? string.Empty));
            result.Add(new XAttribute("Password", Password ?? string.Empty));

            return result;
        }

        #endregion
    }
}

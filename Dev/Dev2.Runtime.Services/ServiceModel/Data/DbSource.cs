#pragma warning disable
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
using System.Linq;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Warewolf.Security.Encryption;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbSource : Resource, IResourceSource, IDb, IPersistenceSource
    {
        #region CTOR

        public DbSource()
        {
            ResourceType = "DbSource";
        }

        public DbSource(enSourceType sourceType)
        {
            ResourceType = "DbSource";
            ServerType = sourceType;
        }

        public DbSource(XElement xml)
            : base(xml)
        {
            // Setup type include default port
            var attributeSafe = xml.AttributeSafe("ServerType");
            switch (attributeSafe.ToLowerInvariant())
            {
                case "sqldatabase":
                    ServerType = enSourceType.SqlDatabase;
                    Port = 1433;
                    break;
                case "mysqldatabase":
                    ServerType = enSourceType.MySqlDatabase;
                    break;
                case "oracle":
                    ServerType = enSourceType.Oracle;
                    Port = 1521;
                    break;
                case "odbc":
                    ServerType = enSourceType.ODBC;
                    break;
                case "postgresql":
                    ServerType = enSourceType.PostgreSQL;
                    break;
                case "sqlite":
                    ServerType = enSourceType.SQLiteDatabase;
                    break;
                default:
                    ResourceType = "DbSource";
                    ServerType = enSourceType.Unknown;
                    break;
            }

            var conString = xml.AttributeSafe("ConnectionString");
            var connectionString = conString.CanBeDecrypted() ? DpapiWrapper.Decrypt(conString) : conString;
            ResourceType = ServerType.ToString();
            ConnectionString = connectionString;
        }

        #endregion

        #region Properties

        [JsonConverter(typeof(StringEnumConverter))]
        public enSourceType ServerType { get; set; }

        public string Server { get; set; }

        public string DatabaseName { get; set; }

        public int Port { get; set; }
        public int ConnectionTimeout { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationType AuthenticationType { get; set; }


        public string UserID { get; set; }


        public string Password { get; set; }

        public new string DataList
        {
            get
            {
                var stringBuilder = base.DataList;
                return stringBuilder?.ToString();
            }
            set { base.DataList = value.ToStringBuilder(); }
        }

        #endregion

        #region ConnectionString

        public string ConnectionString
        {
            //
            // PBI 8720: TODO: Make ConnectionString readonly 
            //
            get
            {
                var portString = string.Empty;
                switch (ServerType)
                {
                    case enSourceType.SqlDatabase:
                        var isNamedInstance = Server != null && Server.Contains('\\');
                        if (isNamedInstance && Port == 1433)
                        {
                            Port = 0;
                        }

                        portString = Port > 0 ? "," + Port : string.Empty;
                        var authString = AuthenticationType == AuthenticationType.Windows
                            ? "Integrated Security=SSPI;"
                            : $"User ID={UserID};Password={Password};";
                        return $"Data Source={Server}{portString};Initial Catalog={DatabaseName};{authString};Connection Timeout={ConnectionTimeout}";

                    case enSourceType.MySqlDatabase:
                        portString = Port > 0 ? $"Port={Port};" : string.Empty;
                        return $"Server={Server};{portString}Database={DatabaseName};Uid={UserID};Pwd={Password};Connect Timeout={ConnectionTimeout};SslMode=none;";

                    case enSourceType.Oracle:
                        portString = Port > 0 ? $":{Port}" : string.Empty;
                        var dbString = DatabaseName != null ? $"Database={DatabaseName};" : string.Empty;
                        return $"User Id={UserID};Password={Password};Data Source={Server}{portString};{dbString};Connection Timeout={ConnectionTimeout};";

                    case enSourceType.ODBC:
                        return $"DSN={DatabaseName};";
                    case enSourceType.SQLiteDatabase:
                        return ":memory:";

                    case enSourceType.PostgreSQL:

                        if (string.IsNullOrEmpty(DatabaseName))
                        {
                            DatabaseName = string.Empty;
                        }

                        portString = Port > 0 ? $"Port={Port};" : string.Empty;
                        return $"Host={Server};{portString}Username={UserID};Password={Password};Database={DatabaseName};Timeout={ConnectionTimeout}";
                    case enSourceType.WebService:
                        break;
                    case enSourceType.DynamicService:
                        break;
                    case enSourceType.ManagementDynamicService:
                        break;
                    case enSourceType.PluginSource:
                        break;
                    case enSourceType.Unknown:
                        break;
                    case enSourceType.Dev2Server:
                        break;
                    case enSourceType.EmailSource:
                        break;
                    case enSourceType.WebSource:
                        break;
                    case enSourceType.OauthSource:
                        break;
                    case enSourceType.SharepointServerSource:
                        break;
                    case enSourceType.RabbitMQSource:
                        break;
                    case enSourceType.ExchangeSource:
                        break;
                    case enSourceType.WcfSource:
                        break;
                    case enSourceType.ComPluginSource:
                        break;
                    default:
                        break;
                }

                return string.Empty;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                AuthenticationType = AuthenticationType.Windows;
                bool containsTimeout = false;
                int defaultTimeout = 30;
                foreach (var prm in value.Split(';').Select(p => p.Split('=')))
                {
                    int port;
                    switch (prm[0].ToLowerInvariant())
                    {
                        case "server":
                        case "host":
                        case "data source":
                            var arr = prm[1].Split(','); // may include port number after comma
                            Server = arr[0];
                            if (arr.Length > 1 && Int32.TryParse(arr[1], out port))
                            {
                                Port = port;
                            }

                            break;
                        case "port":
                            if (Int32.TryParse(prm[1], out port))
                            {
                                Port = port;
                            }

                            break;
                        case "database":
                        case "dsn":
                        case "initial catalog":
                            DatabaseName = prm[1];
                            break;
                        case "integrated security":
                            AuthenticationType = AuthenticationType.Windows;
                            break;
                        case "user id":
                        case "uid":
                        case "username":
                            AuthenticationType = AuthenticationType.User;
                            UserID = prm[1];
                            break;
                        case "password":
                        case "pwd":
                            Password = prm[1];
                            break;
                        case "timeout":
                        case "connect timeout":
                        case "connection timeout":
                            containsTimeout = true;
                            ConnectionTimeout = int.TryParse(prm[1], out int timeout) ? timeout : defaultTimeout;
                            break;
                        default:
                            break;
                    }
                }

                if (!containsTimeout)
                {
                    ConnectionTimeout = defaultTimeout;
                }
            }
        }

        public string GetConnectionStringWithTimeout(int timeout)
        {
            var oldTimeout = ConnectionTimeout;
            ConnectionTimeout = timeout;
            var result = ConnectionString;
            ConnectionTimeout = oldTimeout;

            return result;
        }

        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            result.Add(new XAttribute("ServerType", ServerType));
            result.Add(new XAttribute("Type", GetType().Name));
            result.Add(new XAttribute("ConnectionString", DpapiWrapper.Encrypt(ConnectionString) ?? string.Empty));

            result.Add(new XElement("AuthorRoles", string.Empty));
            result.Add(new XElement("Comment", string.Empty));
            result.Add(new XElement("HelpLink", string.Empty));
            result.Add(new XElement("Tags", string.Empty));
            result.Add(new XElement("UnitTestTargetWorkflowService", string.Empty));
            result.Add(new XElement("BizRule", string.Empty));
            result.Add(new XElement("WorkflowActivityDef", string.Empty));
            return result;
        }

        public override bool IsSource => true;

        public override bool IsService => false;
        public override bool IsFolder => false;
        public override bool IsReservedService => false;
        public override bool IsServer => false;
        public override bool IsResourceVersion => false;

        #endregion
    }
}
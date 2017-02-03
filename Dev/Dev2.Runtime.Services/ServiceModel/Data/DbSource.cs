/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Warewolf.Security.Encryption;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbSource : Resource, IResourceSource, IDb
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
            switch(attributeSafe.ToLowerInvariant())
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

        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationType AuthenticationType { get; set; }

// ReSharper disable InconsistentNaming
        public string UserID { get; set; }
// ReSharper restore InconsistentNaming

        public string Password { get; set; }

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
                        var isNamedInstance = Server != null && Server.Contains('\\');
                        if (isNamedInstance)
                        {
                            if (Port == 1433)
                            {
                                Port = 0;
                            }
                        }
                        return string.Format("Data Source={0}{2};Initial Catalog={1};{3}", Server, DatabaseName,
                            Port > 0 ? "," + Port : string.Empty,
                            AuthenticationType == AuthenticationType.Windows
                                ? "Integrated Security=SSPI;"
                                : string.Format("User ID={0};Password={1};", UserID, Password));

                    case enSourceType.MySqlDatabase:
                        return string.Format("Server={0};{4}Database={1};Uid={2};Pwd={3};",
                            Server, DatabaseName, UserID, Password,
                            Port > 0 ? string.Format("Port={0};", Port) : string.Empty);

                    case enSourceType.Oracle:
                     //database refers to owner/schema in oracle
                        return string.Format("User Id={2};Password={3};Data Source={0};{1}",
                          Server,( DatabaseName  !=null ? string.Format("Database={0};", DatabaseName) : string.Empty) , UserID, Password,
                         Port > 0 ? string.Format(":{0}", Port) : string.Empty);

                    case enSourceType.ODBC:
                        return string.Format("DSN={0};", DatabaseName);

                    case enSourceType.PostgreSQL:

                        if (string.IsNullOrEmpty(DatabaseName))
                            DatabaseName = string.Empty;

                        return string.Format(@"Host={0};Username={1};Password={2};Database={3}", Server, UserID, Password,
                            DatabaseName);

                }
                return string.Empty;
            }

            set
            {
                if(string.IsNullOrEmpty(value))
                {
                    return;
                }

                AuthenticationType = AuthenticationType.Windows;
                
                foreach (var prm in value.Split(';').Select(p => p.Split('=')))
                {
                    int port;
                    switch(prm[0].ToLowerInvariant())
                    {
                        case "server":
                        case "host":
                        case "data source":
                            var arr = prm[1].Split(','); // may include port number after comma
                            Server = arr[0];
                            if(arr.Length > 1)
                            {
                                if(Int32.TryParse(arr[1], out port))
                                {
                                    Port = port;
                                }

                            }
                            break;
                        case "port":
                            if(Int32.TryParse(prm[1], out port))
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
                    }
                }
            }
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

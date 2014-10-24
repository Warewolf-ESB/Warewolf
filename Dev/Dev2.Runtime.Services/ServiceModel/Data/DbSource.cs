
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbSource : Resource
    {
        #region CTOR

        public DbSource()
        {
            ResourceType = Common.Interfaces.Data.ResourceType.DbSource;
        }

        public DbSource(XElement xml)
            : base(xml)
        {
            ResourceType = Common.Interfaces.Data.ResourceType.DbSource;

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

            ConnectionString = xml.AttributeSafe("ConnectionString");
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
                return stringBuilder != null ? stringBuilder.ToString() : null;
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
                        return string.Format("Data Source={0}{2};Initial Catalog={1};{3}", Server, DatabaseName,
                            (Port > 0 ? "," + Port : string.Empty),
                            AuthenticationType == AuthenticationType.Windows
                                ? "Integrated Security=SSPI;"
                                : string.Format("User ID={0};Password={1};", UserID, Password));

                    case enSourceType.MySqlDatabase:
                        return string.Format("Server={0};{4}Database={1};Uid={2};Pwd={3};",
                            Server, DatabaseName, UserID, Password,
                            (Port > 0 ? string.Format("Port={0};", Port) : string.Empty));
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

                foreach(var prm in value.Split(';').Select(p => p.Split('=')))
                {
                    int port;
                    switch(prm[0].ToLowerInvariant())
                    {
                        case "server":
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
                        case "initial catalog":
                            DatabaseName = prm[1];
                            break;
                        case "integrated security":
                            AuthenticationType = AuthenticationType.Windows;
                            break;
                        case "user id":
                        case "uid":
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
            result.Add(new XAttribute("Type", ServerType));
            result.Add(new XAttribute("ConnectionString", ConnectionString ?? string.Empty));

            result.Add(new XElement("AuthorRoles", string.Empty));
            result.Add(new XElement("Comment", string.Empty));
            result.Add(new XElement("HelpLink", string.Empty));
            result.Add(new XElement("Tags", string.Empty));
            result.Add(new XElement("UnitTestTargetWorkflowService", string.Empty));
            result.Add(new XElement("BizRule", string.Empty));
            result.Add(new XElement("WorkflowActivityDef", string.Empty));
            return result;
        }

        #endregion
    }
}

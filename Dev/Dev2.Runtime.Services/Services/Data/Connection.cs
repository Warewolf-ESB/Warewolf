﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Dev2.Runtime.Services.Data
{
    public class Connection : Resource
    {
        public const int DefaultWebServerPort = 1234;

        public string Address { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationType AuthenticationType { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }
        public int WebServerPort { get; set; }

        #region CTOR

        public Connection()
        {
        }

        public Connection(XElement xml)
            : base(xml)
        {
            var connectionString = xml.AttributeSafe("ConnectionString");
            var props = connectionString.Split(';');
            foreach(var p in props.Select(prop => prop.Split('=')).Where(p => p.Length >= 1))
            {
                switch(p[0].ToLowerInvariant())
                {
                    case "appserveruri":
                        Address = p[1];
                        break;
                    case "webserverport":
                        int port;
                        WebServerPort = Int32.TryParse(p[1], out port) ? port : DefaultWebServerPort;
                        break;
                    case "authenticationtype":
                        AuthenticationType authType;
                        AuthenticationType = Enum.TryParse(p[1], true, out authType) ? authType : AuthenticationType.Windows;
                        break;
                    case "username":
                        UserName = p[1];
                        break;
                    case "password":
                        Password = p[1];
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
                string.Format("AppServerUri={0}", Address),
                string.Format("WebServerPort={0}", WebServerPort),
                string.Format("AuthenticationType={0}", AuthenticationType)
                );
            if(AuthenticationType == AuthenticationType.User)
            {
                connectionString = string.Join(";",
                    connectionString,
                    string.Format("UserName={0}", UserName),
                    string.Format("Password={0}", Password)
                    );
            }

            result.Add(new XAttribute("ConnectionString", connectionString));

            return result;
        }

        #endregion

    }
}

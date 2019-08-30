/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Resources;
using Dev2.Runtime.ServiceModel.Data;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Warewolf.Driver.RabbitMQ;
using Warewolf.Security.Encryption;
using Warewolf.Triggers;

namespace Dev2.Data.ServiceModel
{
    //TODO: This class should be moved to Warewolf.Driver.RabbitMQ, hence the current class name duplicate
    public class RabbitMQSource : Resource, IResourceSource, IRabbitMQ, IQueueSource
    {
        const int DefaultPort = 5672;
        const string DefaultVirtualHost = "/";

        public string HostName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }

        IConnectionFactory _factory; 
        private IConnectionFactory GetConnectionFactory()
        {
            if (_factory is null)
            {
                _factory = new ConnectionFactory
                {
                    HostName = HostName,
                    Port = Port,
                    UserName = UserName,
                    Password = Password,
                    VirtualHost = VirtualHost
                };
            }
            return _factory;
        }

        public RabbitMQSource()
        {
            ResourceID = Guid.Empty;
            ResourceType = "RabbitMQSource";
            Port = DefaultPort;
            VirtualHost = DefaultVirtualHost;
        }

        public RabbitMQSource(XElement xml)
            : base(xml)
        {
            ResourceType = "RabbitMQSource";
            Port = DefaultPort;

            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "HostName", string.Empty },
                { "Port", string.Empty },
                { "UserName", string.Empty },
                { "Password", string.Empty },
                { "VirtualHost", string.Empty },
            };

            var conString = xml.AttributeSafe("ConnectionString");
            var connectionString = conString.CanBeDecrypted() ? DpapiWrapper.Decrypt(conString) : conString;
            ParseProperties(connectionString, properties);

            HostName = properties["HostName"];
            UserName = properties["UserName"];
            Password = properties["Password"];

            Port = Int32.TryParse(properties["Port"], out int port) ? port : DefaultPort;
            VirtualHost = !string.IsNullOrWhiteSpace(properties["VirtualHost"]) ? properties["VirtualHost"] : DefaultVirtualHost;
        }

        public override XElement ToXml()
        {
            var result = base.ToXml();
            var connectionString = string.Join(";",
                $"HostName={HostName}",
                $"Port={Port}",
                $"UserName={UserName}",
                $"Password={Password}",
                $"VirtualHost={VirtualHost}"
                );

            result.Add(
                new XAttribute("ConnectionString", DpapiWrapper.Encrypt(connectionString)),
                new XAttribute("Type", ResourceType),
                new XElement("TypeOf", ResourceType)
                );

            return result;
        }

        public IQueueConnection NewConnection()
        {
            return new RabbitConnection(GetConnectionFactory().CreateConnection());
        }

        public override bool IsSource => true;

        public override bool IsService => false;

        public override bool IsFolder => false;

        public override bool IsReservedService => false;

        public override bool IsServer => false;

        public override bool IsResourceVersion => false;
    }
}

/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Warewolf.Security.Encryption;
using RabbitMQ.Client;
using System.Text;

// ReSharper disable CheckNamespace
namespace Dev2.Runtime.ServiceModel.Data
{
    // PBI 953 - 2013.05.16 - TWR - Created
    public class RabbitMQSource : Resource
    {
        //public static int DefaultTimeout = 100000; // (100 seconds)
        public static int DefaultPort = 5672;
        //public static int SslPort = 465;
        //public static int TlsPort = 587;
        public static string DefaultVirtualHost = "/";

        #region Properties

        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        //public bool EnableSsl { get; set; }
        //public int Timeout { get; set; }

        /// <summary>
        /// This must NEVER be persisted - here for JSON transport only!
        /// </summary>
        //public string TestFromAddress { get; set; }

        /// <summary>
        /// This must NEVER be persisted - here for JSON transport only!
        /// </summary>
        //public string TestToAddress { get; set; }

        //public new string DataList
        //{
        //    get
        //    {
        //        var stringBuilder = base.DataList;
        //        return stringBuilder != null ? stringBuilder.ToString() : null;
        //    }
        //    set
        //    {
        //        base.DataList = value.ToStringBuilder();
        //    }
        //}

        #endregion

        #region CTOR

        public RabbitMQSource()
        {
            ResourceID = Guid.Empty;
            ResourceType = ResourceType.RabbitMQSource;
            //Timeout = DefaultTimeout;
            Port = DefaultPort;
        }

        public RabbitMQSource(XElement xml)
            : base(xml)
        {
            ResourceType = ResourceType.RabbitMQSource;
            //Timeout = DefaultTimeout;
            Port = DefaultPort;

            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Host", string.Empty },
                { "Port", string.Empty },
                { "UserName", string.Empty },
                { "Password", string.Empty },
                { "VirtualHost", string.Empty },
            };

            var conString = xml.AttributeSafe("ConnectionString");
            var connectionString = conString.CanBeDecrypted() ? DpapiWrapper.Decrypt(conString) : conString;
            ParseProperties(connectionString, properties);

            Host = properties["Host"];
            UserName = properties["UserName"];
            Password = properties["Password"];

            int port;
            Port = Int32.TryParse(properties["Port"], out port) ? port : DefaultPort;
        }

        public void Send(string message)
        {
            var factory = new ConnectionFactory()
            {
                HostName = Host,
                Port = Port,
                UserName = UserName,
                Password = Password,
                VirtualHost = VirtualHost
            };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "greeting_queue",
                                            durable: false,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                        routingKey: "greeting_queue",
                        basicProperties: null,
                        body: body);
                }
            }




            //var userParts = UserName.Split('@');
            //using(var smtp = new SmtpClient(Host, Port)
            //{
            //    Credentials = new NetworkCredential(userParts[0], Password),
            //    EnableSsl = EnableSsl,
            //    DeliveryMethod = SmtpDeliveryMethod.Network,
            //    Timeout = Timeout
            //})
            //{
            //    smtp.Send(mailMessage);
            //}
        }
        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            var connectionString = string.Join(";",
                string.Format("Host={0}", Host),
                string.Format("Port={0}", Port),
                string.Format("UserName={0}", UserName),
                string.Format("Password={0}", Password),
                string.Format("VirtualHost={0}", VirtualHost)
                );

            result.Add(
                new XAttribute("ConnectionString", DpapiWrapper.Encrypt(connectionString)),
                new XAttribute("Type", ResourceType),
                new XElement("TypeOf", ResourceType)
                );

            return result;
        }

        #endregion
    }
}

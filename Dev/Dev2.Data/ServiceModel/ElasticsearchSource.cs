/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Warewolf.Security.Encryption;

namespace Dev2.Data.ServiceModel
{
    public class ElasticsearchSource : Resource, IDisposable, IResourceSource, IElasticsearchSource
    { 
        const string DefaultPort = "9200";
        private const string DefaultHostname = "http://localhost";
        public string HostName { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationType AuthenticationType { get; set; }
        public string Port { get; set; }
        
        public ElasticsearchSource()
        {
            ResourceID = Guid.Empty;
            ResourceType = "ElasticsearchSource";
            AuthenticationType = AuthenticationType.Anonymous;
            Port = DefaultPort;
            HostName = DefaultHostname;
        }
        
        public ElasticsearchSource(XElement xml) : base(xml)
        {
            ResourceType = "ElasticsearchSource";
            AuthenticationType = AuthenticationType.Anonymous;
            Port = DefaultPort;
            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "HostName", string.Empty },
                { "Port", string.Empty },
                { "AuthenticationType", string.Empty },
                { "Password", string.Empty },
                { "Username", string.Empty}
            }; 
            
            var conString = xml.AttributeSafe("ConnectionString");
            var connectionString = conString.CanBeDecrypted() ? DpapiWrapper.Decrypt(conString) : conString;
            connectionString = connectionString.UnescapeString();
            ParseProperties(connectionString, properties);
            HostName = properties["HostName"];
            Port = properties["Port"];
            Username = properties["Username"];
            Password = properties["Password"];
            AuthenticationType = Enum.TryParse(properties["AuthenticationType"], true, out AuthenticationType authType) ? authType : AuthenticationType.Windows;
        }
        
        public override XElement ToXml()
        {
            var result = base.ToXml();
            var connectionString = string.Join(";",
                $"HostName={HostName}",
                $"Port={Port}",
                $"AuthenticationType={AuthenticationType}"
            );

            if (AuthenticationType == AuthenticationType.Password)
            {
                connectionString = string.Join(";",
                    connectionString,
                    $"Username={Username}",
                    $"Password={Password}"
                );
            }

            result.Add(
                new XAttribute("ConnectionString", DpapiWrapper.Encrypt(connectionString.EscapeString())),
                new XAttribute("Type", GetType().Name),
                new XElement("TypeOf", ResourceType)
            );

            return result;
        }
        
        public override bool IsSource => true;
        public override bool IsService => false;

        public override bool IsFolder => false;

        public override bool IsReservedService => false;

        public override bool IsServer => false;

        public override bool IsResourceVersion => false;

        bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios. 
        // If disposing equals true, the method has been called directly 
        // or indirectly by a user's code. Managed and unmanaged resources 
        // can be disposed. 
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed. 
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!_disposed)
            {
                // Call the appropriate methods to clean up 
                // unmanaged resources here. 

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}
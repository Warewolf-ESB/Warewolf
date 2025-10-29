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
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Security.Encryption;

namespace Dev2.Data.ServiceModel
{
    public class ChatbotSource : Resource, IDisposable, IResourceSource
    {
        public string ApiKey { get; set; }
        public string CompletionsEndpoint { get; set; }

        public ChatbotSource()
        {
            ResourceID = Guid.Empty;
            ResourceType = "ChatbotSource";
        }

        public ChatbotSource(XElement xml)
            : base(xml)
        {
            ResourceType = "ChatbotSource";
            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "ApiKey", string.Empty },
                { "CompletionsEndpoint", string.Empty }
            };

            var conString = xml.AttributeSafe("ConnectionString");
            var connectionString = conString.CanBeDecrypted() ? DpapiWrapper.Decrypt(conString) : conString;
            connectionString = connectionString.UnescapeString();
            ParseProperties(connectionString, properties);
            ApiKey = properties["ApiKey"];
            CompletionsEndpoint = properties["CompletionsEndpoint"];
        }

        public override XElement ToXml()
        {
            var result = base.ToXml();
            var connectionString = string.Join(";",
                $"ApiKey={ApiKey}",
                $"CompletionsEndpoint={CompletionsEndpoint}"
                );

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
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}

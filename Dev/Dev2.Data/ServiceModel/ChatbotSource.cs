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
using Dev2.Common.Interfaces.Resources;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Security.Encryption;

namespace Dev2.Data.ServiceModel
{
    public class ChatbotSource : Resource, IDisposable, IResourceSource, IChatbotSourceResource
    {
        public string ApiKey { get; set; }
        public string CompletionsEndpoint { get; set; }
        public string ModelsEndpoint { get; set; }
        public string SelectedModel { get; set; }
        public string Provider { get; set; }

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
                { "CompletionsEndpoint", string.Empty },
                { "ModelsEndpoint", string.Empty },
                { "SelectedModel", string.Empty },
                { "Provider", string.Empty }
            };

            var conString = xml.AttributeSafe("ConnectionString");
            var connectionString = conString.CanBeDecrypted() ? DpapiWrapper.Decrypt(conString) : conString;
            connectionString = connectionString.UnescapeString();
            ParseProperties(connectionString, properties);
            ApiKey = properties["ApiKey"];
            CompletionsEndpoint = properties["CompletionsEndpoint"];
            ModelsEndpoint = properties["ModelsEndpoint"];
            SelectedModel = properties["SelectedModel"];
            Provider = properties["Provider"];
        }

        public override XElement ToXml()
        {
            var result = base.ToXml();
            var connectionString = string.Join(";",
                $"ApiKey={ApiKey}",
                $"CompletionsEndpoint={CompletionsEndpoint}",
                $"ModelsEndpoint={ModelsEndpoint}",
                $"SelectedModel={SelectedModel}",
                $"Provider={Provider}"
                );

            result.Add(
#if WINDOWS
                new XAttribute("ConnectionString", DpapiWrapper.Encrypt(connectionString.EscapeString())),
#else
				new XAttribute("ConnectionString", connectionString.EscapeString()),
#endif
				new XAttribute("Type", GetType().Name),
                new XElement("TypeOf", ResourceType)
                );

            if (VersionInfo != null)
            {
                result.Add(
                    new XElement("VersionInfo",
                        new XAttribute("DateTimeStamp", VersionInfo.DateTimeStamp),
                        new XAttribute("Reason", VersionInfo.Reason ?? string.Empty),
                        new XAttribute("User", VersionInfo.User ?? string.Empty),
                        new XAttribute("VersionNumber", VersionInfo.VersionNumber ?? string.Empty),
                        new XAttribute("ResourceId", VersionInfo.ResourceId),
                        new XAttribute("VersionId", VersionInfo.VersionId)
                    )
                );
            }

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

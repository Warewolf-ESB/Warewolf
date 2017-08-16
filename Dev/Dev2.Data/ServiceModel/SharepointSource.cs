using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Warewolf.Security.Encryption;
using Warewolf.Sharepoint;



namespace Dev2.Data.ServiceModel
{
    public class SharepointSource : Resource, ISharepointSource, IResourceSource
    {
        public string Server { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationType AuthenticationType { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }

        public SharepointSource()
        {
            ResourceID = Guid.Empty;
            ResourceType = "SharepointServerSource";
            AuthenticationType = AuthenticationType.Windows;
        }

        public SharepointSource(XElement xml)
            : base(xml)
        {
            ResourceType = "SharepointServerSource";
            AuthenticationType = AuthenticationType.Windows;

            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Server", string.Empty },
                { "AuthenticationType", string.Empty },
                { "UserName", string.Empty },
                { "Password", string.Empty }
            };

            var conString = xml.AttributeSafe("ConnectionString");
            var connectionString = conString.CanBeDecrypted() ? DpapiWrapper.Decrypt(conString) : conString;
            ParseProperties(connectionString, properties);
            Server = properties["Server"];
            UserName = properties["UserName"];
            Password = properties["Password"];
            var isSharepointSourceValue = xml.AttributeSafe("IsSharepointOnline");
            bool isSharepointSource;
            if (bool.TryParse(isSharepointSourceValue, out isSharepointSource))
            {
                IsSharepointOnline = isSharepointSource;
            }
            AuthenticationType authType;
            AuthenticationType = Enum.TryParse(properties["AuthenticationType"], true, out authType) ? authType : AuthenticationType.Windows;
        }

        /// <summary>
        /// Gets the XML representation of this resource.
        /// </summary>
        /// <returns>
        /// The XML representation of this resource.
        /// </returns>
        public override XElement ToXml()
        {
            var result = base.ToXml();
            var connectionString = string.Join(";",
                $"Server={Server}",
                $"AuthenticationType={AuthenticationType}"
                );

            if (AuthenticationType == AuthenticationType.User)
            {
                connectionString = string.Join(";",
                    connectionString,
                    $"UserName={UserName}",
                    $"Password={Password}"
                    );
            }

            result.Add(
                new XAttribute("ConnectionString", DpapiWrapper.Encrypt(connectionString)),
                new XAttribute("IsSharepointOnline", IsSharepointOnline),
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

        public List<SharepointListTo> LoadLists()
        {
            var sharepointHelper = CreateSharepointHelper();
            return sharepointHelper.LoadLists();
        }

        public List<ISharepointFieldTo> LoadFieldsForList(string listName, bool editableFieldsOnly = false)
        {
            var sharepointHelper = CreateSharepointHelper();
            return sharepointHelper.LoadFieldsForList(listName, editableFieldsOnly);
        }

        public SharepointHelper CreateSharepointHelper()
        {
            string userName = null;
            string password = null;

            if (AuthenticationType == AuthenticationType.User)
            {
                userName = UserName;
                password = Password;
            }
            var sharepointHelper = new SharepointHelper(Server, userName, password, IsSharepointOnline);
            return sharepointHelper;
        }

        public string TestConnection()
        {
            var helper = CreateSharepointHelper();
            bool isSharepointOnline;
            var testConnection = helper.TestConnection(out isSharepointOnline);
            IsSharepointOnline = isSharepointOnline;
            return testConnection;
        }

        public bool IsSharepointOnline { get; set; }
    }
}
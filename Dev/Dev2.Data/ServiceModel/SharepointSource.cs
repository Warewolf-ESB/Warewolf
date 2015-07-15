using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Warewolf.Sharepoint;

namespace Dev2.Data.ServiceModel
{
    public class SharepointSource : Resource,ISharepointSource
    {
        public string Server { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationType AuthenticationType { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public SharepointSource()
        {
            ResourceID = Guid.Empty;
            ResourceType = ResourceType.SharepointServerSource;
            AuthenticationType = AuthenticationType.Windows;
        }

        public SharepointSource(XElement xml)
            : base(xml)
        {
            ResourceType = ResourceType.SharepointServerSource;
            AuthenticationType = AuthenticationType.Windows;

            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Server", string.Empty },
                { "AuthenticationType", string.Empty },
                { "UserName", string.Empty },
                { "Password", string.Empty }
            };

            ParseProperties(xml.AttributeSafe("ConnectionString"), properties);

            Server = properties["Server"];
            UserName = properties["UserName"];
            Password = properties["Password"];

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
                string.Format("Server={0}", Server),
                string.Format("AuthenticationType={0}", AuthenticationType)
                );

            if (AuthenticationType == AuthenticationType.User)
            {
                connectionString = string.Join(";",
                    connectionString,
                    string.Format("UserName={0}", UserName),
                    string.Format("Password={0}", Password)
                    );
            }

            result.Add(
                new XAttribute("ConnectionString", connectionString),
                new XAttribute("Type", ResourceType),
                new XElement("TypeOf", ResourceType)
                );

            return result;
        }

        public List<SharepointListTo> LoadLists()
        {
            var sharepointHelper = CreateSharepointHelper();
            return sharepointHelper.LoadLists();
        }

        public List<ISharepointFieldTo> LoadFieldsForList(string listName,bool editableFieldsOnly = false)
        {
            var sharepointHelper = CreateSharepointHelper();
            return sharepointHelper.LoadFieldsForList(listName, editableFieldsOnly);
        }

        public SharepointHelper CreateSharepointHelper()
        {
            string userName = null;
            string password = null;

            if(AuthenticationType == AuthenticationType.User)
            {
                userName = UserName;
                password = Password;
            }
            var sharepointHelper = new SharepointHelper(Server, userName, password);
            return sharepointHelper;
        }

        public string TestConnection()
        {
            var helper = CreateSharepointHelper();
            return helper.TestConnection();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Runtime.ServiceModel.Data
{
    // PBI 5656 - 2013.05.20 - TWR - Created
    public class WebSource : Resource
    {
        #region Properties

        public string Address { get; set; }
        public string DefaultQuery { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationType AuthenticationType { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// This must NEVER be persisted - here for JSON transport only!
        /// </summary>
        public string Response { get; set; }

        #endregion

        #region CTOR

        public WebSource()
        {
            ResourceID = Guid.Empty;
            ResourceType = ResourceType.WebSource;
            AuthenticationType = AuthenticationType.Anonymous;
        }

        public WebSource(XElement xml)
            : base(xml)
        {
            ResourceType = ResourceType.WebSource;
            AuthenticationType = AuthenticationType.Anonymous;

            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Address", string.Empty },
                { "DefaultQuery", string.Empty },
                { "AuthenticationType", string.Empty },
                { "UserName", string.Empty },
                { "Password", string.Empty }
            };

            ParseProperties(xml.AttributeSafe("ConnectionString"), properties);

            Address = properties["Address"];
            DefaultQuery = properties["DefaultQuery"];
            UserName = properties["UserName"];
            Password = properties["Password"];

            AuthenticationType authType;
            AuthenticationType = Enum.TryParse(properties["AuthenticationType"], true, out authType) ? authType : AuthenticationType.Windows;
        }

        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            var connectionString = string.Join(";",
                string.Format("Address={0}", Address),
                string.Format("DefaultQuery={0}", DefaultQuery),
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

            result.Add(
                new XAttribute("ConnectionString", connectionString),
                new XAttribute("Type", ResourceType),
                new XElement("TypeOf", ResourceType)
                );

            return result;
        }

        #endregion

    }
}

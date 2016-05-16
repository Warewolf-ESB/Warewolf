using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Security.Encryption;

namespace Dev2.Data.ServiceModel
{
    // ReSharper disable once UnusedMember.Global
    public class OauthSource : Resource, IResourceSource, IOAuthSource
    {
        #region Properties

        // ReSharper disable once MemberCanBePrivate.Global
        public string AccessToken { get; set; }
        // ReSharper disable once MemberCanBePrivate.Global
        public string AppKey { get; set; }


        #endregion

        #region CTOR

        public OauthSource()
        {
            ResourceID = Guid.Empty;
            ResourceType = "OauthSource";
            AccessToken = string.Empty;
            AppKey = string.Empty;
        }

        public OauthSource(XElement xml)
            : base(xml)
        {
            ResourceType = "OauthSource";


            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "AccessToken", string.Empty },
                { "AppKey", string.Empty }
            };

            var conString = xml.AttributeSafe("ConnectionString");
            var connectionString = conString.CanBeDecrypted() ? DpapiWrapper.Decrypt(conString) : conString;
            ParseProperties(connectionString, properties);
            AccessToken = properties["AccessToken"];
            AppKey = properties["AppKey"];
   

        }

       
        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            var connectionString = string.Join(";",
                string.Format("AccessToken={0}", AccessToken),
                string.Format("AppKey={0}", AppKey)
                );

            result.Add(
                new XAttribute("ConnectionString", DpapiWrapper.Encrypt(connectionString)),
                new XAttribute("Type", GetType().Name),
                new XElement("TypeOf", ResourceType)
                );

            return result;
        }

        public override bool IsSource
        {
            get
            {
                return true;
            }
        }
        public override bool IsService
        {
            get
            {
                return false;
            }
        }
        public override bool IsFolder
        {
            get
            {
                return false;
            }
        }
        public override bool IsReservedService
        {
            get
            {
                return false;
            }
        }
        public override bool IsServer
        {
            get
            {
                return false;
            }
        }
        public override bool IsResourceVersion
        {
            get
            {
                return false;
            }
        }

        #endregion

    }
}
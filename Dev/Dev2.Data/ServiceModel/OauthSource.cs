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
    public abstract class OauthSource : Resource, IResourceSource, IOAuthSource
    {

        #region CTOR

        protected OauthSource()
        {
            ResourceID = Guid.Empty;            
        }

        protected OauthSource(XElement xml)
            : base(xml)
        {
        }
       
        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            var connectionString = GetConnectionString();

            result.Add(
                new XAttribute("ConnectionString", DpapiWrapper.Encrypt(connectionString)),
                new XAttribute("Type", GetType().Name),
                new XElement("TypeOf", ResourceType)
                );

            return result;
        }

        protected abstract string GetConnectionString();

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

        public abstract string AppKey { get; set; }
        public abstract string AccessToken { get; set; }

        #region Implementation of IEquatable<IOAuthSource>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public abstract bool Equals(IOAuthSource other);

        #endregion
    }

    public class DropBoxSource : OauthSource
    {
        public DropBoxSource()
        {
            ResourceType = "DropBoxSource";
        }

        public DropBoxSource(XElement xml)
            : base(xml)
        {
            ResourceType = "DropBoxSource";

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

        public sealed override string AccessToken { get; set; }
        public sealed override string AppKey { get; set; }



        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public override bool Equals(IOAuthSource other)
        {
            return AppKey == other.AppKey && AccessToken == other.AccessToken;
        }

        protected override string GetConnectionString()
        {
            var connectionString = string.Join(";",
                string.Format("AccessToken={0}", AccessToken),
                string.Format("AppKey={0}", AppKey)
                );
            return connectionString;
        }
    }
}
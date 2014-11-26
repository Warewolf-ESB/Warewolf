
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Xml.Linq;
using Dev2.Common.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable CheckNamespace
namespace Dev2.Runtime.ServiceModel.Data
// ReSharper restore CheckNamespace
{
    // PBI 5656 - 2013.05.20 - TWR - Created
    public class WebSource : Resource, IDisposable
    {
        bool _disposed;

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

        /// <summary>
        /// This must NEVER be persisted - here so that we only instantiate once!
        /// </summary>
        public WebClient Client { get; set; }

        #endregion

        #region CTOR

        public WebSource()
        {
            ResourceID = Guid.Empty;
            ResourceType = Common.Interfaces.Data.ResourceType.WebSource;
            AuthenticationType = AuthenticationType.Anonymous;
        }

        public WebSource(XElement xml)
            : base(xml)
        {
            ResourceType = Common.Interfaces.Data.ResourceType.WebSource;
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

        #region DisposeClient

        public void DisposeClient()
        {
            if(Client != null)
            {
                Client.Dispose();
                Client = null;
            }
        }

        #endregion

        #region Implementation of IDisposable

        // This destructor will run only if the Dispose method does not get called. 
        ~WebSource()
        {
            // Do not re-create Dispose clean-up code here. 
            // Calling Dispose(false) is optimal in terms of 
            // readability and maintainability.
            Dispose(false);
        }

        // Implement IDisposable. 
        // Do not make this method virtual. 
        // A derived class should not be able to override this method. 
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
            if(!_disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if(disposing)
                {
                    // Dispose managed resources.
                    DisposeClient();
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here. 

                // Note disposing has been done.
                _disposed = true;
            }
        }

        #endregion
    }
}


/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Common;

namespace Dev2.Runtime.Security
{
    public class HostSecurityProvider : IHostSecurityProvider
    {
        public static readonly Guid InternalServerID = new Guid("51A58300-7E9D-4927-A57B-E5D700B11B55");
        public static readonly string InternalPublicKey = "BgIAAACkAABSU0ExAAQAAAEAAQBlsJw+ibEmPy3P93PV7a8QjuHqS4QR+yP/+6CVUUpqvUE3hguQUzZ4Fw28hz0LwMLK8Sc1qb0s0FFiH9Ju6O+fIXruGzC3CjzN8wZRGoV2IvfmJ/nMKQ/NVESx9virJA1xTIZa9Za3PQvGbPh1ce0me5YJd3VOHKUqqJCbVeE7pg==";

        readonly RSACryptoServiceProvider _serverKey;
        readonly RSACryptoServiceProvider _systemKey;
        public Guid ServerID { get; private set; }

        #region Singleton Instance

        //
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a syncRoot 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //
        static volatile IHostSecurityProvider TheInstance;
        static readonly object SyncRoot = new Object();

        /// <summary>
        /// Gets the repository instance.
        /// </summary>
        public static IHostSecurityProvider Instance
        {
            get
            {
                if (TheInstance == null)
                {
                    lock (SyncRoot)
                    {
                        if (TheInstance == null)
                        {
                            var config = new HostSecureConfig();
                            TheInstance = new HostSecurityProvider(config);
                        }
                    }
                }
                return TheInstance;
            }
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="HostSecurityProvider" /> class.
        /// </summary>
        /// <param name="config">The config to be used.</param>
        protected HostSecurityProvider(ISecureConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            ServerID = config.ServerID;
            _serverKey = config.ServerKey;
            _systemKey = config.SystemKey;
        }

        #endregion

        #region VerifyXml

        public bool VerifyXml(StringBuilder xml)
        {
            if (xml == null || xml.Length == 0)
            {
                throw new ArgumentNullException("xml");
            }

            using(Stream s = xml.EncodeForXmlDocument())
            {
                var doc = new XmlDocument();
                doc.Load(s);

                // Validate server ID, this is a check which can be done quickly in order to skip loading the whole file for verification        
                var serverID = GetServerID(doc);

                /*
                 * NOTE : 
                 * 
                 * This magical check is here for shipping resources
                 * It enables the server on first start to resign the resource such that
                 * the end user's install can view and execute them ;)
                 * 
                 * To ship a resource you need to do the following : 
                 * 
                 * 1) Set the type to Unknown
                 * 2) Give the resource a server ID of our InternalServerID
                 * 3) Remove any existing signing data 
                 * 
                 */
                if (serverID != ServerID && serverID != InternalServerID)
                {
                    return false;
                }

                // Find the "Signature" node and add it to the SignedXml object
                var signedXml = new SignedXml(doc);
                var nodeList = doc.GetElementsByTagName("Signature");

                // allow unsigned resources with our internal server ID
                if (nodeList.Count == 0 && serverID == InternalServerID)
                {
                    return true;
                }

                signedXml.LoadXml((XmlElement) nodeList[0]);


                var result = (serverID == ServerID && signedXml.CheckSignature(_serverKey)) ||
                             (serverID != InternalServerID == signedXml.CheckSignature(_systemKey));


                // Check if signed by the server or the system
                return result;
            }
        }

        #endregion

        #region SignXml

        public StringBuilder SignXml(StringBuilder xml)
        {
           
            if (xml == null || xml.Length == 0)
            {
                throw new ArgumentNullException("xml");
            }

            // remove the signature element here as it does not pick up correctly futher down ;(
            xml = RemoveSignature(xml);

            using(Stream s = xml.EncodeForXmlDocument())
            {
                var doc = new XmlDocument();
               
                doc.Load(s);

                SetServerID(doc);

                // Create a reference to be signed and add
                // an enveloped transformation to the reference.
                var reference = new Reference
                {
                    Uri = ""
                };

                reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());

                var signedXml = new SignedXml(doc)
                {
                    SigningKey = _serverKey
                };
                signedXml.AddReference(reference);
                signedXml.ComputeSignature();

                // Get the XML representation of the signature and save
                // it to an XmlElement object.
                var xmlDigitalSignature = signedXml.GetXml();

                // Append the element to the XML document.
                if (doc.DocumentElement != null)
                {
                    doc.DocumentElement.AppendChild(doc.ImportNode(xmlDigitalSignature, true));
                }

                StringBuilder result = new StringBuilder();
                using (StringWriter sw = new StringWriter(result))
                {
                    doc.Save(sw);
                }

                // remove the crapy encoding header
                result = result.CleanEncodingHeaderForXmlSave();

                return result;
            }
        }

        #endregion

        #region Remove Signature
        StringBuilder RemoveSignature(StringBuilder sb)
        {
            const string signatureStart = "<Signature xmlns";
            const string signatureEnd = "</Signature>";

            var startIdx = sb.IndexOf(signatureStart, 0,false);
            if (startIdx >= 0)
            {
                var endIdx = sb.IndexOf(signatureEnd, startIdx,false);

                if (endIdx >= 0)
                {
                    var len = (endIdx - startIdx) + signatureEnd.Length;
                    return sb.Remove(startIdx, len);
                }
            }

            return sb;
        }
        #endregion

        #region Get/Set ServerID

        void SetServerID(XmlDocument doc)
        {
            if (doc.DocumentElement != null)
            {
                doc.DocumentElement.SetAttribute("ServerID", ServerID.ToString());
            }
        }

        static Guid GetServerID(XmlDocument doc)
        {
            if (doc.DocumentElement != null)
            {
                var attr = doc.DocumentElement.Attributes["ServerID"];
                if (attr != null)
                {
                    if (!string.IsNullOrEmpty(attr.Value))
                    {
                        Guid id;
                        if (Guid.TryParse(attr.Value, out id))
                        {
                            return id;
                        }
                    }
                }
            }
            return Guid.Empty;
        }

        #endregion

        #region EnsureSSL
        public bool EnsureSsl(string certPath, IPEndPoint endPoint)
        {
            bool result = false;

            if (!File.Exists(certPath))
            {
                try
                {
                    var certificateBuilder = new SslCertificateBuilder();
                    result = certificateBuilder.EnsureSslCertificate(certPath, endPoint);

                    result = File.Exists(certPath);
                }
                catch (Exception e)
                {
                    Dev2Logger.Log.Error(e);
                }
            }
            else
            {
                result = File.Exists(certPath);
            }

            return result;
        }

        public void EnsureAccessToPort(string url)
        {
            var args = string.Format("http add urlacl url={0}/ user=\\Everyone", url);
            try
            {
                bool invoke = ProcessHost.Invoke(null, "netsh.exe", args);
                if (!invoke)
                {
                    Dev2Logger.Log.Error(string.Format("There was an error adding url: {0}", url));
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Log.Error(e);
            }
            
        }
        #endregion
    }
}

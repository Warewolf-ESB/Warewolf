/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
        static volatile IHostSecurityProvider _theInstance;
        static readonly object SyncRoot = new object();

        /// <summary>
        /// Gets the repository instance.
        /// </summary>
        public static IHostSecurityProvider Instance
        {
            get
            {
                if (_theInstance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_theInstance == null)
                        {
                            var config = new HostSecureConfig();
                            _theInstance = new HostSecurityProvider(config);
                        }
                    }
                }
                return _theInstance;
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
            return true;
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

            using(var s = xml.EncodeForXmlDocument())
            {
                var doc = new XmlDocument();
               
                doc.Load(s);

                SetServerID(doc);

                var result = new StringBuilder();
                using (var sw = new StringWriter(result))
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
            const string SignatureStart = "<Signature xmlns";
            const string SignatureEnd = "</Signature>";

            var startIdx = sb.IndexOf(SignatureStart, 0,false);
            if (startIdx >= 0)
            {
                var endIdx = sb.IndexOf(SignatureEnd, startIdx,false);

                if (endIdx >= 0)
                {
                    var len = endIdx - startIdx + SignatureEnd.Length;
                    return sb.Remove(startIdx, len);
                }
            }

            return sb;
        }
        #endregion

        #region Get/Set ServerID

        void SetServerID(XmlDocument doc)
        {
            doc.DocumentElement?.SetAttribute("ServerID", ServerID.ToString());
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
                    Dev2Logger.Error(e);
                }
            }
            else
            {
                result = SslCertificateBuilder.BindSslCertToPorts(endPoint, certPath);
            }

            return result;
        }

        #endregion
    }
}

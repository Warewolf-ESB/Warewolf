using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

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
                            TheInstance = new HostSecurityProvider(new HostSecureConfig());
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

        public bool VerifyXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentNullException("xml");
            }

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            // Validate server ID, this is a check which can be done quickly in order to skip loading the whole file for verification        
            var serverID = GetServerID(doc);
            if (serverID != ServerID && serverID != InternalServerID)
            {
                return false;
            }

            // Find the "Signature" node and add it to the SignedXml object
            var signedXml = new SignedXml(doc);
            var nodeList = doc.GetElementsByTagName("Signature");
            signedXml.LoadXml((XmlElement)nodeList[0]);

            // Check if signed by the server or the system
            return (serverID == ServerID && signedXml.CheckSignature(_serverKey)) ||
                (serverID != InternalServerID == signedXml.CheckSignature(_systemKey));
        }

        #endregion

        #region SignXml

        public string SignXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentNullException("xml");
            }

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            SetServerID(doc);

            var nodeList = doc.GetElementsByTagName("Signature");
            if (nodeList.Count > 0)
            {
                var child = nodeList[0];
                if (child.ParentNode != null)
                {
                    child.ParentNode.RemoveChild(child);
                }
            }

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

            if (doc.FirstChild is XmlDeclaration)
            {
                doc.RemoveChild(doc.FirstChild);
            }
            return doc.OuterXml.Trim();
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
    }
}
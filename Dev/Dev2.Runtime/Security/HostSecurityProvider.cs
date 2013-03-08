using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Dev2.DynamicServices.Security
{
    public class HostSecurityProvider : IHostSecurityProvider
    {
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
        static volatile IHostSecurityProvider _instance;
        static readonly object SyncRoot = new Object();

        /// <summary>
        /// Gets the repository instance.
        /// </summary>
        public static IHostSecurityProvider Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(SyncRoot)
                    {
                        if(_instance == null)
                        {
                            _instance = new HostSecurityProvider(new HostSecureConfig());
                        }
                    }
                }
                return _instance;
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
            if(config == null)
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
            if(string.IsNullOrEmpty(xml))
            {
                throw new ArgumentNullException("xml");
            }

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            // Validate server ID           
            var serverID = GetServerID(doc);
            if(serverID != ServerID)
            {
                return false;
            }

            // Find the "Signature" node and add it to the SignedXml object
            var signedXml = new SignedXml(doc);
            var nodeList = doc.GetElementsByTagName("Signature");
            signedXml.LoadXml((XmlElement)nodeList[0]);

            // Check if signed by the server or the system
            return signedXml.CheckSignature(_serverKey) || signedXml.CheckSignature(_systemKey);
        }

        #endregion

        #region SignXml

        public string SignXml(string xml)
        {
            if(string.IsNullOrEmpty(xml))
            {
                throw new ArgumentNullException("xml");
            }

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            SetServerID(doc);

            var nodeList = doc.GetElementsByTagName("Signature");
            if(nodeList.Count > 0)
            {
                var child = nodeList[0];
                if(child.ParentNode != null)
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
            if(doc.DocumentElement != null)
            {
                doc.DocumentElement.AppendChild(doc.ImportNode(xmlDigitalSignature, true));
            }

            if(doc.FirstChild is XmlDeclaration)
            {
                doc.RemoveChild(doc.FirstChild);
            }
            return doc.OuterXml.Trim();
        }

        #endregion

        #region Get/Set ServerID

        void SetServerID(XmlDocument doc)
        {
            if(doc.DocumentElement != null)
            {
                doc.DocumentElement.SetAttribute("ServerID", ServerID.ToString());
            }
        }

        static Guid GetServerID(XmlDocument doc)
        {
            if(doc.DocumentElement != null)
            {
                var attr = doc.DocumentElement.Attributes["ServerID"];
                if(attr != null)
                {
                    if(!string.IsNullOrEmpty(attr.Value))
                    {
                        Guid id;
                        if(Guid.TryParse(attr.Value, out id))
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
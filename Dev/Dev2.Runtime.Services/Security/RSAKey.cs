
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
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace Dev2.Runtime.Security
{
    /// <summary>
    /// A RSA key in base64.
    /// </summary>
    public class RSAKey
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RSAKey" /> class
        /// with a new <see cref="RSACryptoServiceProvider" />.
        /// </summary>
        public RSAKey()
            : this(new RSACryptoServiceProvider())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RSAKey" /> class.
        /// </summary>
        /// <param name="csp">The <see cref="RSACryptoServiceProvider"/> to be used.</param>
        /// <exception cref="System.ArgumentNullException">csp</exception>
        public RSAKey(ICspAsymmetricAlgorithm csp)
        {
            if(csp == null)
            {
                throw new ArgumentNullException("csp");
            }

            var publicKey = csp.ExportCspBlob(false);
            var privateKey = csp.ExportCspBlob(true);

            Public = Convert.ToBase64String(publicKey);
            Private = Convert.ToBase64String(privateKey);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RSAKey" /> class.
        /// </summary>
        /// <param name="xml">The xml represention to be loaded.</param>
        /// <exception cref="System.ArgumentNullException">xml</exception>
        public RSAKey(XContainer xml)
        {
            if(xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            XElement elem;
            if((elem = xml.Elements("Private").FirstOrDefault()) != null)
            {
                Private = elem.Value;
            }
            if((elem = xml.Elements("Public").FirstOrDefault()) != null)
            {
                Public = elem.Value;
            }
        }

        #endregion

        /// <summary>
        /// The public key in base64.
        /// </summary>
        public string Public { get; set; }

        /// <summary>
        /// The private key in base64.
        /// </summary>
        public string Private { get; set; }

        /// <summary>
        /// Creates an XML representation of this instance.
        /// </summary>
        /// <returns>An XML representation of this instance.</returns>
        public XElement ToXml()
        {
            return new XElement("RSAKey",
                                new XElement("Public", Public),
                                new XElement("Private", Private));
        }

        /// <summary>
        /// Creates an <see cref="RSACryptoServiceProvider"/> from this instance.
        /// </summary>
        /// <param name="usePrivate">If <c>true</c> uses the <see cref="Private"/> key; otherwise uses the <see cref="Public"/> key.</param>
        /// <returns>
        /// An <see cref="RSACryptoServiceProvider"/>.
        /// </returns>
        public RSACryptoServiceProvider ToCSP(bool usePrivate)
        {
            var cspBlob = Convert.FromBase64String(usePrivate ? Private : Public);
            var csp = new RSACryptoServiceProvider();
            csp.ImportCspBlob(cspBlob);
            return csp;
        }

        /// <summary>
        /// Saves this key to the specified file path.
        /// </summary>
        /// <param name="filePath">The file path to be used.</param>
        public void Save(string filePath)
        {
            var xml = ToXml();
            xml.Save(filePath);
        }

        /// <summary>
        /// Loads an <see cref="RSAKey"/> from a file.
        /// </summary>
        /// <param name="filePath">The file path referencing the file to load into a new <see cref="RSAKey"/>.</param>
        /// <returns>An <see cref="RSAKey"/> that contains the contents of the specified file.</returns>
        public static RSAKey Load(string filePath)
        {
            var xml = XElement.Load(filePath);
            return new RSAKey(xml);
        }
    }
}

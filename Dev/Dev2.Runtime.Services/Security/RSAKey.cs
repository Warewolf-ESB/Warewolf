/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
    }
}

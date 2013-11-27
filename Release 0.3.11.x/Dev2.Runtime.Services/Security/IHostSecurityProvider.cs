
using System;
using System.Net;

namespace Dev2.Runtime.Security
{
    /// <summary>
    /// Defines the requirements for a host security provider.
    /// </summary>
    public interface IHostSecurityProvider
    {
        /// <summary>
        /// Gets the server ID.
        /// </summary>
        Guid ServerID { get; }

        /// <summary>
        /// Verifies the given XML with this server.
        /// </summary>
        /// <param name="xml">The XML to be verified.</param>
        /// <returns><code>true</code> if the XML was sigend by this server; <code>false</code> otherwise.</returns>
        bool VerifyXml(string xml);

        /// <summary>
        /// Signs the given XML with this server's key.
        /// </summary>
        /// <param name="xml">The XML to be signed.</param>
        /// <returns>The signed XML.</returns>
        string SignXml(string xml);

        /// <summary>
        /// Ensures SSL support with self-signed cert.
        /// </summary>
        /// <param name="certPath">The cert path.</param>
        /// <returns></returns>
        bool EnsureSSL(string certPath, IPEndPoint endPoint);
    }
}

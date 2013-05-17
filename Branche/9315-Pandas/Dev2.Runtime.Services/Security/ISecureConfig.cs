using System;
using System.Security.Cryptography;

namespace Dev2.Runtime.Security
{
    /// <summary>
    /// Defines the requirements for a secure config.
    /// </summary>
    public interface ISecureConfig
    {
        /// <summary>
        /// Gets the server ID.
        /// </summary>
        Guid ServerID { get; }

        /// <summary>
        /// Gets the server key.
        /// </summary>
        RSACryptoServiceProvider ServerKey { get; }

        /// <summary>
        /// Gets the system key.
        /// </summary>
        RSACryptoServiceProvider SystemKey { get; }
    }
}

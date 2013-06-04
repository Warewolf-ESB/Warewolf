using System;
using System.Collections.Generic;

namespace Dev2.Studio.Core.Interfaces
{
    /// <summary>
    /// Defines the requirements for a server.
    /// <remarks>
    /// <strong>Note:</strong> <see cref="WebUri"/> and <see cref="AppUri"/> 
    /// must be strings otherwise serialization will fail!
    /// </remarks>
    /// </summary>
    public interface IServer : IComparable<IServer>
    {
        #region Servers

        /// <summary>
        /// Gets the list of servers stored on this server..
        /// </summary>
        IList<IServer> Servers
        {
            get;
        }

        #endregion

        #region Environment

        IEnvironmentModel Environment { get; set; }

        #endregion

        #region ID

        /// <summary>
        /// Gets the unique ID of this server.
        /// </summary>
        string ID
        {
            get;
            set;
        }

        #endregion

        #region Alias

        /// <summary>
        /// Gets the alias of this server.
        /// </summary>
        string Alias
        {
            get;
            set;
        }

        #endregion

        #region AppUri

        /// <summary>
        /// Gets the application server <see cref="Uri"/> including port number.
        /// </summary>
        string AppAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the application server <see cref="Uri"/> including port number.
        /// </summary>
        Uri AppUri
        {
            get;
        }

        #endregion

        #region WebUri

        /// <summary>
        /// Gets the web server <see cref="Uri"/> including port number.
        /// </summary>
        string WebAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the web server <see cref="Uri"/> including port number.
        /// </summary>
        Uri WebUri
        {
            get;
        }

        bool IsLocalHost { get; }
        #endregion

    }
}

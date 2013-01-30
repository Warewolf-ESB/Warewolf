using Dev2.Studio.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Dev2.Studio.Core.InterfaceImplementors
{
    /// <summary>
    /// A server definition.
    /// </summary>
    public class ServerDTO : IServer
    {
        #region Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDTO" /> class with the information from a <see cref="IEnvironmentModel" />.
        /// </summary>
        public ServerDTO(IEnvironmentModel environment)
        {
            Servers = new List<IServer>();

            Environment = environment;
            ID = environment.ID.ToString();
            Alias = environment.Name;
            AppAddress = environment.DsfAddress.AbsoluteUri;
            WebAddress = environment.WebServerAddress.AbsoluteUri;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDTO" /> class.
        /// </summary>
        public ServerDTO()
        {
            Servers = new List<IServer>();
        }

        #endregion

        #region Environment

        public IEnvironmentModel Environment { get; set; }

        #endregion

        #region Servers

        /// <summary>
        /// Gets the list of servers stored on this server.
        /// </summary>
        [XmlIgnore]
        public IList<IServer> Servers
        {
            get;
            private set;
        }

        #endregion

        #region ID

        /// <summary>
        /// Gets the unique ID of this server.
        /// </summary>
        public string ID
        {
            get;
            set;
        }

        #endregion

        #region Alias

        /// <summary>
        /// Gets the alias of this server.
        /// </summary>
        public string Alias
        {
            get;
            set;
        }

        #endregion

        #region AppAddress

        /// <summary>
        /// Gets the application server <see cref="Uri"/> including port number.
        /// </summary>
        public string AppAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the application server <see cref="Uri"/> including port number.
        /// </summary>
        [XmlIgnore]
        public Uri AppUri
        {
            get
            {
                Uri uri;
                if(!Uri.TryCreate(AppAddress, UriKind.Absolute, out uri))
                {
                    uri = new Uri(StringResources.Uri_Live_Environment);
                }
                return uri;
            }
        }

        #endregion

        #region WebAddress

        /// <summary>
        /// Gets the web server <see cref="Uri"/> including port number.
        /// </summary>
        public string WebAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the web server <see cref="Uri"/> including port number.
        /// </summary>
        [XmlIgnore]
        public Uri WebUri
        {
            get
            {
                Uri uri;
                if(!Uri.TryCreate(WebAddress, UriKind.Absolute, out uri))
                {
                    uri = new Uri(StringResources.Uri_WebServer);
                }
                return uri;
            }
        }

        #endregion

        #region Load

        public void Load()
        {
            Servers.Clear();

            // TODO: Load servers stored on this server 
        }

        #endregion

        #region CompareTo

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object..</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public int CompareTo(IServer other)
        {
            // If other is not a valid object reference, this instance is greater.
            if(other == null) return 1;

            // The comparison depends on the comparison of the underlying ID values. 
            return String.CompareOrdinal(ID, other.ID);
        }

        #endregion
    }
}

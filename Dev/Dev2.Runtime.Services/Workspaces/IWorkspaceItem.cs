using System;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Core.DynamicServices;

namespace Dev2.Workspaces
{
    /// <summary>
    /// Defines the requirements for a <see cref="IWorkspace"/> item.
    /// </summary>
    public interface IWorkspaceItem : ISerializable, IEquatable<IWorkspaceItem>
    {
        /// <summary>
        /// The unique ID of the item.
        /// </summary>
        Guid ID
        {
            get;
        }

        /// <summary>
        /// Gets or sets the workspace ID.
        /// </summary>
        Guid WorkspaceID
        {
            get;
        }

        /// <summary>
        /// Gets or sets the server ID.
        /// </summary>
        Guid ServerID
        {
            get;
        }
        /// <summary>
        /// Gets or sets the action to be taken on the item.
        /// </summary>
        WorkspaceItemAction Action
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        string ServiceName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the service.
        /// <remarks>Must map to a <see cref="enDynamicServiceObjectType"/> value</remarks>
        /// </summary>
        string ServiceType
        {
            get;
            set;
        }

        Guid EnvironmentID { get; }
        bool IsWorkflowSaved { get; set; }

        /// <summary>
        /// Gets the XML representation of this instance.
        /// </summary>
        XElement ToXml();
    }
}

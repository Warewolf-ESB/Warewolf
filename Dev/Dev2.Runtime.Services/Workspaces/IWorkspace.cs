using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dev2.Workspaces {
    /// <summary>
    /// Defines the requirements for a workspace.
    /// </summary>
    public partial interface IWorkspace : ISerializable, IEquatable<IWorkspace> {
        /// <summary>
        /// Gets or sets the unique ID.
        /// </summary>
        Guid ID {
            get;
        }


        /// <summary>
        /// Gets the items for this workspace.
        /// </summary>
        IList<IWorkspaceItem> Items {
            get;
        }
    }
}

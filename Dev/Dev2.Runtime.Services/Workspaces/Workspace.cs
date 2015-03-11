
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dev2.Workspaces
{
    /// <summary>
    /// A workspace.
    /// </summary>
    [Serializable]
    public partial class Workspace : IWorkspace
    {

        #region Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="Workspace" /> class.
        /// </summary>
        /// <param name="workspaceID">The id of the workspace.</param>
        public Workspace(Guid workspaceID)
        {
            ID = workspaceID;
            Items = new List<IWorkspaceItem>();
        }

        #endregion

        #region ID

        /// <summary>
        /// Gets or sets the unique ID.
        /// </summary>
        public Guid ID
        {
            get;
            private set;
        }

        #endregion

        #region Items

        /// <summary>
        /// Gets the items for this workspace.
        /// </summary>
        public IList<IWorkspaceItem> Items
        {
            get;
            private set;
        }

        #endregion

        #region ISerializable

        protected Workspace(SerializationInfo info, StreamingContext context)
        {
            if(info == null)
            {
                throw new ArgumentNullException("info");
            }
            ID = (Guid)info.GetValue("ID", typeof(Guid));
            Items = (IList<IWorkspaceItem>)info.GetValue("Items", typeof(IList<IWorkspaceItem>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if(info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("ID", ID);
            info.AddValue("Items", Items);
        }

        #endregion

        #region IEquatable

        public bool Equals(IWorkspace other)
        {
            if(other == null)
            {
                return false;
            }
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            var item = obj as IWorkspace;
            return item != null && Equals(item);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        #endregion

    }
}

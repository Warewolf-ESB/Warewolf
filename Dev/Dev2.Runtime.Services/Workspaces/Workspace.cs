/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
    [Serializable]
    public class Workspace : IWorkspace
    {
        public Workspace(Guid workspaceID)
        {
            ID = workspaceID;
            Items = new List<IWorkspaceItem>();
        }

        public Guid ID
        {
            get;
            private set;
        }

        public IList<IWorkspaceItem> Items
        {
            get;
            private set;
        }

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
    }
}

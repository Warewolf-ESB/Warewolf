
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
using System.Collections.Generic;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Versioning;

namespace Dev2.Explorer
{
    public class ServerExplorerItem : IExplorerItem
    {
        public ServerExplorerItem()
        {
            Children = new List<IExplorerItem>();
        }
        public ServerExplorerItem(string displayName, Guid resourceId, string resourceType,
                                  IList<IExplorerItem> children, Permissions permissions, string resourcePath, string inputs, string outputs)
        {
            DisplayName = displayName;
            ResourceId = resourceId;
            ResourceType = resourceType;
            Children = children;
            Permissions = permissions;
            ResourcePath = resourcePath;
            Inputs = inputs;
            Outputs = outputs;
        }
        public string DisplayName { get; set; }
        public Guid ResourceId { get; set; }
        public Guid ServerId { get; set; }
        public string WebserverUri { get; set; }
        public bool IsSource
        {
            get;
            set;
        }
        public bool IsService
        {
            get;
            set;
        }
        public bool IsFolder
        {
            get;
            set;
        }
        public bool IsReservedService
        {
            get;
            set;
        }
        public bool IsServer
        {
            get;
            set;
        }
        public bool IsResourceVersion
        {
            get;
            set;
        }

        public string Inputs
        {
            get;
            set;
        }

        public string Outputs
        {
            get;
            set;
        }

        public string ResourceType { get; set; }

        public IList<IExplorerItem> Children { get; set; }
        public Permissions Permissions { get; set; }
        public IVersionInfo VersionInfo { get; set; }
        public string ResourcePath { get; set; }
        public IExplorerItem Parent { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ServerExplorerItem)obj);
        }


        protected bool Equals(ServerExplorerItem other)
        {
            return ResourceId.Equals(other.ResourceId);
        }

        public override int GetHashCode()
        {
            return ResourceId.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("Name:{0} Path:{1} Id:{2}", DisplayName, ResourcePath, ResourceId);
        }
    }
}

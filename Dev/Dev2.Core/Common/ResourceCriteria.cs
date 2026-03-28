/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Runtime.Serialization;
using Dev2.Interfaces;
using Warewolf.Data;

namespace Dev2.Common
{
    [DataContract]
    public class ResourceCriteria : IResourceCriteria
    {
        [DataMember]
        public Guid ResourceID { get; set; }
        [DataMember]
        public Guid WorkspaceId { get; set; }
        [DataMember]
        public IVersionInfo VersionInfo { get; set; }
        [DataMember]
        public string ResourceName { get; set; }
        [DataMember]
        public string ResourceType { get; set; }
        [DataMember]
        public string ResourcePath { get; set; }
        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public string AuthorRoles { get; set; }
        [DataMember]
        public bool IsUpgraded { get; set; }
        [DataMember]
        public bool IsNewResource { get; set; }
        [DataMember]
        public bool FetchAll { get; set; }
        [DataMember]
        public string GuidCsv { get; set; }
    }
}

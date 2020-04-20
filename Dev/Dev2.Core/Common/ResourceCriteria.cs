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
using Dev2.Interfaces;
using Warewolf.Data;

namespace Dev2.Common
{
    [Serializable]
    public class ResourceCriteria : IResourceCriteria
    {
        public Guid ResourceID { get; set; }
        public Guid WorkspaceId { get; set; }
        public IVersionInfo VersionInfo { get; set; }
        public string ResourceName { get; set; }
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public string FilePath { get; set; }
        public string AuthorRoles { get; set; }
        public bool IsUpgraded { get; set; }
        public bool IsNewResource { get; set; }
        public bool FetchAll { get; set; }
        public string GuidCsv { get; set; }
    }
}

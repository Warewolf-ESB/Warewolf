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
using Warewolf.Data;

namespace Dev2.Interfaces
{
    public interface IResourceCriteria
    {
        string AuthorRoles { get; set; }
        bool FetchAll { get; set; }
        string FilePath { get; set; }
        string GuidCsv { get; set; }
        bool IsNewResource { get; set; }
        bool IsUpgraded { get; set; }
        Guid ResourceID { get; set; }
        string ResourceName { get; set; }
        string ResourcePath { get; set; }
        string ResourceType { get; set; }
        IVersionInfo VersionInfo { get; set; }
        Guid WorkspaceId { get; set; }
    }
}
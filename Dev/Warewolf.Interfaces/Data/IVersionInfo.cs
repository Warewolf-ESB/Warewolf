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

namespace Warewolf.Data
{
    public interface IVersionInfo
    {
        DateTime DateTimeStamp { get; set; }
        string Reason { get; set; }
        string User { get; set; }
        string VersionNumber { get; set; }
        Guid ResourceId { get; set; }
        Guid VersionId { get; set; }
    }
}
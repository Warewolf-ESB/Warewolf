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

namespace Dev2.Common.Interfaces.Scheduler.Interfaces
{
    public interface ISecurityWrapper : IDisposable
    {
        bool IsWindowsAuthorised(string privilege, string userName);        
        // bool IsWarewolfAuthorised(string privilege, string userName, string resourceGuid);
        bool IsWarewolfAuthorised(string privilege, string userName, Guid resourceId);
        bool IsWarewolfAuthorised(string privilege, string userName, IWarewolfResource resource);
    }
}

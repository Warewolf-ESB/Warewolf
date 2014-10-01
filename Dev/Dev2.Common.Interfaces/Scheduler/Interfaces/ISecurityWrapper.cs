
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;

namespace Dev2.Common.Interfaces.Scheduler.Interfaces
{
    public interface ISecurityWrapper : IDisposable
    {
        /// <summary>
        /// Reads the user accounts which have the specific privilege
        /// </summary>
        /// <param name="privilege">The name of the privilege for which the accounts with this right should be enumerated</param>
        /// <param name="userName"></param>
        bool IsWindowsAuthorised(string privilege, string userName);
        /// <summary>
        /// Checks if the user has warewolf permissions for a resource guid
        /// </summary>
        /// <param name="privilege"></param>
        /// <param name="userName"></param>
        /// <param name="resourceGuid"></param>
        /// <returns></returns>
        bool IsWarewolfAuthorised(string privilege, string userName, string resourceGuid);
    }
}

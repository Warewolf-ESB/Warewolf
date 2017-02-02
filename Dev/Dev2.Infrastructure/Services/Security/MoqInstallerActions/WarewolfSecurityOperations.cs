/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;

namespace Dev2.Services.Security.MoqInstallerActions
{
    public interface IWarewolfSecurityOperations
    {
        void AddWarewolfGroup();

        bool DoesWarewolfGroupExist();

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        bool IsUserInGroup(string username);

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void AddUserToWarewolf(string currentUser);

        void DeleteWarewolfGroup();

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        string FormatUserForInsert(string currentUser, string machineName);

        void AddAdministratorsGroupToWarewolf();

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        bool IsAdminMemberOfWarewolf();
    }
}

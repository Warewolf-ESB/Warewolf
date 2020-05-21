/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Warewolf;

namespace Dev2.Services.Security
{
    public interface ISecurityService : IDisposable
    {
        event EventHandler PermissionsChanged;
        event EventHandler AuthenticationChanged;
        IReadOnlyList<WindowsGroupPermission> Permissions { get; }
        INamedGuid OverrideResource { get; }
        string SecretKey { get; }
        TimeSpan TimeOutPeriod { get; set; }

        void Read();
        event EventHandler<PermissionsModifiedEventArgs> PermissionsModified;
        event EventHandler<AuthenticationModifiedEventArgs> AuthenticationModified;

        void Remove(Guid resourceId);
    }
}

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
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Common.Interfaces.ServerProxyLayer
{
    public interface IDbSource : IEquatable<IDbSource>
    {
        string ServerName { get; set; }
        enSourceType Type { get; set; }
        AuthenticationType AuthenticationType { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        string Name { get; set; }
        string Path { get; set; }
        Guid Id { get; set; }
        string DbName { get; set; }
        bool ReloadActions { get; set; }
    }
}

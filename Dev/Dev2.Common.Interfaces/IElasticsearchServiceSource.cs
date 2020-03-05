/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Runtime.ServiceModel.Data;
using System;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Dev2.Common.Interfaces
{
    public interface IElasticsearchServiceSource : IEquatable<IElasticsearchServiceSource>
    {
        string HostName { get; set; }
        string Password { get; set; }
        string Port { get; set; }
        string Name { get; set; }
        string Path { get; set; }
        Guid Id { get; set; }
    }
}

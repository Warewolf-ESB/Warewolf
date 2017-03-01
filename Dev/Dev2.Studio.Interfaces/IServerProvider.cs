/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;

namespace Dev2.Studio.Interfaces
{
    /// <summary>
    /// Defines the requirements for an <see cref="IServer"/> provider.
    /// </summary>
    public interface IEnvironmentModelProvider
    {
        List<IServer> Load();
        List<IServer> ReloadServers();
        List<IServer> Load(IServerRepository serverRepository);
        List<IServer> ReloadServers(IServerRepository serverRepository);
    }
}

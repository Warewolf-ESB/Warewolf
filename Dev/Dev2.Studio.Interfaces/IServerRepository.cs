/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Studio.Core;

namespace Dev2.Studio.Interfaces
{
    public interface IServerRepository : IFrameworkRepository<IServer>
    {
        IServer Source { get; }
        IServer ActiveServer { get; set; }
        event EventHandler<IEnvironmentEditedArgs> ItemEdited;
        bool IsLoaded { get; set; }

        ICollection<IServer> ReloadServers();

        IServer Get(Guid id);

        /// <summary>
        /// Lookups the environments.
        /// <remarks>
        /// If <paramref name="environmentGuids"/> is <code>null</code> or empty then this returns all <see cref="enSourceType.Dev2Server"/> sources.
        /// </remarks>
        /// </summary>
        /// <param name="defaultEnvironment">The default environment.</param>
        /// <param name="environmentGuids">The environment guids to be queried; may be null.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">defaultEnvironment</exception>
        IList<IServer> LookupEnvironments(IServer defaultEnvironment, IList<string> environmentGuids = null);

    
        void Clear();

        ICollection<IServer> ReloadAllServers();
    }
}

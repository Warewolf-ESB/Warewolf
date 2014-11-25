
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
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IEnvironmentRepository : IFrameworkRepository<IEnvironmentModel>
    {
        IEnvironmentModel Source { get; }
        IEnvironmentModel ActiveEnvironment { get; set; }
        event EventHandler<EnvironmentEditedArgs> ItemEdited;
        bool IsLoaded { get; set; }

        void Clear();

        IEnvironmentModel Fetch(IEnvironmentModel server);

        IList<Guid> ReadSession();

        void WriteSession(IEnumerable<Guid> environmentGuids);

        void ForceLoad();

        void Remove(Guid id);

        IEnvironmentModel Get(Guid id);

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
        IList<IEnvironmentModel> LookupEnvironments(IEnvironmentModel defaultEnvironment, IList<string> environmentGuids = null);
    }
}

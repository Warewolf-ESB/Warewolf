/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Threading;

namespace Dev2.Runtime.Interfaces
{
    /// <summary>
    /// Loads a single database source into <see cref="Dev2.Runtime.Hosting.ResourceCatalog"/>
    /// on demand, without pre-loading the entire resource directory.
    /// Implementations are registered via <see cref="AmbientSourceLoader"/> and called from
    /// <c>ServiceExecutionAbstract.GetSource(Guid)</c> when a catalog lookup misses.
    /// </summary>
    public interface IOnDemandSourceLoader
    {
        /// <summary>
        /// Ensures the source identified by <paramref name="sourceId"/> is present in
        /// <see cref="Dev2.Runtime.Hosting.ResourceCatalog"/>.
        /// Loads it from disk only if it has not already been loaded.
        /// Returns <c>true</c> when the source is now available in the catalog.
        /// </summary>
        bool EnsureSourceLoaded(Guid sourceId);
    }

    /// <summary>
    /// Ambient registry for <see cref="IOnDemandSourceLoader"/>.
    /// Set once by the Azure Function host before execution begins;
    /// remains <c>null</c> on the full Warewolf server where sources are
    /// pre-loaded by the server startup sequence.
    /// </summary>
    public static class AmbientSourceLoader
    {
        private static volatile IOnDemandSourceLoader _current;

        /// <summary>Current loader, or <c>null</c> when running on the full server.</summary>
        public static IOnDemandSourceLoader Current => _current;

        /// <summary>Registers <paramref name="loader"/> as the active on-demand loader.</summary>
        public static void Register(IOnDemandSourceLoader loader) =>
            Interlocked.Exchange(ref _current, loader);

        /// <summary>Removes the registered loader (useful in tests).</summary>
        public static void Clear() =>
            Interlocked.Exchange(ref _current, null);
    }
}

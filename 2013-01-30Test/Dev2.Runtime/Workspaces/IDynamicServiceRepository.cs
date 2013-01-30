
using System.Collections.Generic;
using Dev2.DynamicServices;

namespace Dev2.Workspaces
{
    /// <summary>
    /// Defines the requirements of a <see cref="IDynamicServiceObject" /> repository.
    /// </summary>
    public interface IDynamicServiceRepository
    {
        /// <summary>
        /// Gets the items in the repository.
        /// </summary>
        ICollection<IDynamicServiceObject> Items
        {
            get;
        }

        /// <summary>
        /// Gets the service with the given ID.
        /// </summary>
        /// <param name="serviceID">The service ID to be queried.</param>
        /// <returns>The service with the given ID or null if not found.</returns>
        IDynamicServiceObject Get(string serviceID);

        /// <summary>
        /// Adds the specified service to the repository.
        /// </summary>
        /// <param name="dso">The <see cref="IDynamicServiceObject"/> to be added.</param>
        void Add(IDynamicServiceObject dso);

        /// <summary>
        /// Replaces the specified service in the repository.
        /// </summary>
        /// <param name="dso">The <see cref="IDynamicServiceObject"/> to be replaced.</param>
        void Replace(IDynamicServiceObject dso);

        /// <summary>
        /// Removes the specified service from the repository.
        /// </summary>
        /// <param name="dso">The <see cref="IDynamicServiceObject"/> to be removed.</param>
        void Remove(IDynamicServiceObject dso);

        /// <summary>
        /// Loads the services in the server repository.
        /// before adding the repository's files.
        /// </summary>
        void Load();

        /// <summary>
        /// Loads a copy of the given service from the server repository.
        /// </summary>
        /// <param name="dso">The <see cref="IDynamicServiceObject"/> to be loaded.</param>
        void Load(IDynamicServiceObject dso);

        /// <summary>
        /// Commits (and compiles) a copy of the given service to the server repository
        /// and then <see cref="Remove" />'s  it from this repository
        /// </summary>
        /// <param name="dso">The <see cref="IDynamicServiceObject"/> to be committed.</param>
        void Commit(IDynamicServiceObject dso);

        /// <summary>
        /// Discards the given service and loads the one from the server repository.
        /// </summary>
        /// <param name="dso">The <see cref="IDynamicServiceObject"/> to be discarded.</param>
        void Discard(IDynamicServiceObject dso);
    }
}

using System.Collections.Generic;

namespace Dev2.Studio.Core.Interfaces
{
    /// <summary>
    /// Defines the requirements for a service that queries resource dependencies.
    /// </summary>
    public interface IResourceDependencyService
    {
        /// <summary>
        /// Gets the dependencies XML for the given <see cref="IResourceModel"/>.
        /// </summary>
        /// <param name="resourceModel">The resource model to be queried.</param>
        /// <returns>The dependencies XML.</returns>
        string GetDependenciesXml(IContextualResourceModel resourceModel);

        /// <summary>
        /// Gets a list of unique dependencies for the given <see cref="IResourceModel"/>.
        /// </summary>
        /// <param name="resourceModel">The resource model to be queried.</param>
        /// <returns>A list of <see cref="IResourceModel"/>'s.</returns>
        List<IResourceModel> GetUniqueDependencies(IContextualResourceModel resourceModel);

        bool HasDependencies(IContextualResourceModel resourceModel);
    }
}

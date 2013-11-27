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

        /// <summary>
        /// Gets a list of dependencies for the given ResourceModel's.
        /// </summary>
        /// <param name="resourceModels">The resource models to get dependancies for.</param>
        /// <returns>A list of resource name string's.</returns>
        List<string> GetDependanciesOnList(List<IContextualResourceModel> resourceModels, IEnvironmentModel environmentModel, bool getDependsOnMe = false);

        bool HasDependencies(IContextualResourceModel resourceModel);
    }
}

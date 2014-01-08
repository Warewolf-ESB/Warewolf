
// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    /// <summary>
    /// Defines the requirements for a deploy service.
    /// </summary>
    public interface IDeployService
    {
        /// <summary>
        /// Deploys the <see cref="IResourceModel"/>'s represented by the given DTO.
        /// </summary>
        /// <param name="deployDto">The DTO to be deployed.</param>
        /// <param name="environmentModel" />
        void Deploy(IDeployDto deployDto, IEnvironmentModel environmentModel);
    }
}

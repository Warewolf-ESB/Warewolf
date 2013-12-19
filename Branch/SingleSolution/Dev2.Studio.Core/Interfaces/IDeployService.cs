
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
        /// <param name="deployDTO">The DTO to be deployed.</param>
        void Deploy(IDeployDTO deployDTO, IEnvironmentModel environmentModel);
    }
}

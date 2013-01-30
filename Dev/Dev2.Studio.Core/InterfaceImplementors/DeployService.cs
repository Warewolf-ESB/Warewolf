using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.InterfaceImplementors
{
    /// <summary>
    /// A deploy service.
    /// </summary>
    public class DeployService : IDeployService
    {
        #region Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="DeployService" /> class.
        /// </summary>
        public DeployService()
        {
        }

        #endregion

        #region Deploy

        /// <summary>
        /// Deploys the <see cref="IResourceModel" />'s represented by the given DTO.
        /// </summary>
        /// <param name="deployDTO">The DTO to be deployed.</param>
        /// <param name="environmentModel">The environment model to be queried.</param>
        public void Deploy(IDeployDTO deployDTO, IEnvironmentModel environmentModel)
        {
            if (deployDTO == null || deployDTO.ResourceModels == null || environmentModel == null || environmentModel.Resources == null)
            {
                return;
            }

            if (!environmentModel.IsConnected)
            {
                environmentModel.Connect();
            }

            if (environmentModel != null && environmentModel.IsConnected)
            {
                foreach (var resourceModel in deployDTO.ResourceModels)
                {
                    environmentModel.Resources.DeployResource(resourceModel);
                }
            }
        }

        #endregion
    }
}

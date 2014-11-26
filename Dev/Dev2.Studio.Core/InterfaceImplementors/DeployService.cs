
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.InterfaceImplementors
{
    /// <summary>
    /// A deploy service.
    /// </summary>
    public class DeployService : IDeployService
    {
        #region Deploy

        /// <summary>
        /// Deploys the <see cref="IResourceModel" />'s represented by the given DTO.
        /// </summary>
        /// <param name="deployDto">The DTO to be deployed.</param>
        /// <param name="environmentModel">The environment model to be queried.</param>
        public void Deploy(IDeployDto deployDto, IEnvironmentModel environmentModel)
        {

            if(deployDto == null || deployDto.ResourceModels == null || environmentModel == null || environmentModel.ResourceRepository == null)
            {
                return;
            }

            if(!environmentModel.IsConnected)
            {
                environmentModel.Connect();
            }

            if(environmentModel.IsConnected)
            {
                foreach(var resourceModel in deployDto.ResourceModels)
                {
                    environmentModel.ResourceRepository.DeployResource(resourceModel);
                }
            }
        }

        #endregion
    }
}

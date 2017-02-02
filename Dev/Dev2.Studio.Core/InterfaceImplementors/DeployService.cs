/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
        /// <param name="sourceEnviroment"></param>
        /// <param name="environmentModel">The environment model to be queried.</param>
        public void Deploy(IDeployDto deployDto, IEnvironmentModel sourceEnviroment, IEnvironmentModel environmentModel)
        {

            if (deployDto?.ResourceModels == null || environmentModel?.ResourceRepository == null)
            {
                return;
            }

            if (!environmentModel.IsConnected)
            {
                environmentModel.Connect();
            }

            if (environmentModel.IsConnected)
            {
                foreach (var resourceModel in deployDto.ResourceModels)
                {
                    var savePath = resourceModel.GetSavePath();
                    environmentModel.ResourceRepository.DeployResource(resourceModel, savePath);
                    if (deployDto.DeployTests)
                    {
                        var models = sourceEnviroment.ResourceRepository.LoadResourceTestsForDeploy(resourceModel.ID);
                        environmentModel.ResourceRepository.SaveTests(resourceModel, models);
                    }
                }
            }
        }



        #endregion
    }
}

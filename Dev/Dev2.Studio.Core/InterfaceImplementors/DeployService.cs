/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Studio.Interfaces;

namespace Dev2.Studio.Core.InterfaceImplementors
{
    /// <summary>
    /// A deploy service.
    /// </summary>
    public class DeployService : IDeployService
    {
        /// <summary>
        /// Deploys the <see cref="IResourceModel" />'s represented by the given DTO.
        /// </summary>
        /// <param name="deployDto">The DTO to be deployed.</param>
        /// <param name="sourceEnviroment"></param>
        /// <param name="server">The environment model to be queried.</param>
        public void Deploy(IDeployDto deployDto, IServer sourceEnviroment, IServer server)
        {

            if (deployDto?.ResourceModels == null || server?.ResourceRepository == null)
            {
                return;
            }

            if (!server.IsConnected)
            {
                server.Connect();
            }

            if (server.IsConnected)
            {
                foreach (var resourceModel in deployDto.ResourceModels)
                {
                    var savePath = resourceModel.GetSavePath();
                    server.ResourceRepository.DeployResource(resourceModel, savePath);
                    if (deployDto.DeployTests)
                    {
                        var models = sourceEnviroment.ResourceRepository.LoadResourceTestsForDeploy(resourceModel.ID);
                        server.ResourceRepository.SaveTests(resourceModel, models);
                    }

                    if (deployDto.DeployTriggers)
                    {
                        //TODO: Load the triggers link to the resource
                        //var models = sourceEnviroment.ResourceRepository.LoadResourceTriggersForDeploy(resourceModel.ID);
                    }
                }
            }
        }
    }
}

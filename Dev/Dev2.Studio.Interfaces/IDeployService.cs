/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

namespace Dev2.Studio.Interfaces
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
        /// <param name="sourceEnviroment"></param>
        /// <param name="servernmentModel" />
        void Deploy(IDeployDto deployDto, IServer sourceEnviroment, IServer server);

    }
}

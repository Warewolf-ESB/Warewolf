/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.TO;
using Warewolf.Storage;

namespace Dev2.Data.SystemTemplates.Models
{
    public interface IDev2DataModel
    {
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        string Version { get; }

        /// <summary>
        /// Gets the name of the model.
        /// </summary>
        /// <value>
        /// The name of the model.
        /// </value>
        Dev2ModelType ModelName { get; }

        /// <summary>
        /// To the web model.
        /// </summary>
        /// <returns></returns>
        string ToWebModel();

        /// <summary>
        /// Generates the user friendly model.
        /// </summary>
        /// <returns></returns>
        string GenerateUserFriendlyModel(IExecutionEnvironment env, Dev2DecisionMode mode, out ErrorResultTO errors);

    }
}

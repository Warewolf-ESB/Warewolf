/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2025 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.TO;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.ServiceModel
{
    /// <summary>
    /// Strategy pattern interface for web execution
    /// Allows switching between WebClient and HttpClient implementations
    /// </summary>
    public interface IWebExecutionStrategy
    {
        /// <summary>
        /// Executes a web POST request using the strategy's underlying implementation
        /// </summary>
        /// <param name="options">The web post options containing all request parameters</param>
        /// <param name="errors">Output parameter for any errors encountered</param>
        /// <returns>Base64 encoded response string</returns>
        string Execute(IWebPostOptions options, out ErrorResultTO errors);
    }
}

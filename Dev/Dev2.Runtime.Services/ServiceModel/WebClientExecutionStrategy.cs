/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2025 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Data.TO;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.ServiceModel
{
    /// <summary>
    /// WebClient-based execution strategy
    /// Uses the existing WebSources.Execute implementation with WebClient
    /// This is the current/legacy implementation
    /// </summary>
    public class WebClientExecutionStrategy : IWebExecutionStrategy
    {
        public string Execute(IWebPostOptions options, out ErrorResultTO errors)
        {
            Dev2Logger.Info("WebClientExecutionStrategy - Using WebClient for POST execution", GlobalConstants.WarewolfInfo);
            
            // Delegate to the existing WebSources.Execute implementation
            return WebSources.Execute(options, out errors);
        }
    }
}

// 
// /*
// *  Warewolf - Once bitten, there's no going back
// *  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
// *  Licensed under GNU Affero General Public License 3.0 or later. 
// *  Some rights reserved.
// *  Visit our website for more information <http://warewolf.io/>
// *  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
// *  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
// */

using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Infrastructure.Communication;

namespace Dev2.Common
{
    public class DeployResult : IDeployResult
    {
        public string ErrorDetails { get; set; }

        public bool HasError { get; set; }

        public string Message { get; set; }

        public DeployResult()
        {
                
        }

        public DeployResult(IExecuteMessage result, string resource)
        {
            Message = result.HasError ? $"{resource} Deployed Successfuly" : $"Error Deploying {resource}";
            HasError = result.HasError;
            ErrorDetails = result.Message.ToString();
        }
    }
}
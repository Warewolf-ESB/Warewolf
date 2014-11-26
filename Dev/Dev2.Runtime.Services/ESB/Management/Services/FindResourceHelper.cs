
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.ServiceModel;
using Dev2.Providers.Errors;
using Dev2.Runtime.Security;
using Dev2.Services.Security;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FindResourceHelper
    {
        private IAuthorizationService _authorizationService;

        /// <summary>
        /// Strips for ship.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns></returns>
        public SerializableResource SerializeResourceForStudio(IResource resource)
        {

            // convert the fliping errors due to json issues in c# ;(
            var errors = new List<ErrorInfo>();
            var parseErrors = resource.Errors;
            if(parseErrors != null)
            {
                errors.AddRange(parseErrors.Select(error => (error as ErrorInfo)));
            }

            var datalist = "<DataList></DataList>";

            if(resource.DataList != null)
            {
                var replace = resource.DataList.Replace("\"", GlobalConstants.SerializableResourceQuote);
                datalist = replace.Replace("'", GlobalConstants.SerializableResourceSingleQuote).ToString();
            }

            return new SerializableResource
            {
                Inputs = resource.Inputs,
                Outputs = resource.Outputs,
                ResourceCategory = resource.ResourcePath,
                ResourceID = resource.ResourceID,
                VersionInfo = resource.VersionInfo,
                ResourceName = resource.ResourceName,
                Permissions = AuthorizationService.GetResourcePermissions(resource.ResourceID),
                ResourceType = resource.ResourceType,
                IsValid = resource.IsValid,
                DataList = datalist,
                Errors = errors,
                IsNewResource = resource.IsNewResource
            };
        }


        internal IAuthorizationService AuthorizationService
        {
            get
            {
                return _authorizationService ?? (_authorizationService = ServerAuthorizationService.Instance);
            }
            set
            {
                _authorizationService = value;
            }
        }
    }
}

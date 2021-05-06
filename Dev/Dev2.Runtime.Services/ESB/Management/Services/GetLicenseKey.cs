/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;
using Warewolf.Licensing;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetLicenseKey : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            var result = new ExecuteMessage {HasError = false};

            Dev2Logger.Info("Get LicenseKey Service", GlobalConstants.WarewolfInfo);
            var licenseData = new LicenseData {CustomerId = GlobalConstants.LicenseCustomerId, PlanId = GlobalConstants.LicensePlanId};

            var license = new WarewolfLicense();
            var resultData = license.Retrieve(licenseData);
            result.Message = serializer.SerializeToBuilder(resultData);

            return serializer.SerializeToBuilder(result);
        }

        public override DynamicService CreateServiceEntry()
        {
            var newDs = new DynamicService {Name = HandlesType()};
            var sa = new ServiceAction {Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType()};
            newDs.Actions.Add(sa);
            return newDs;
        }

        public override string HandlesType() => nameof(Warewolf.Service.GetLicenseKey);
    }
}
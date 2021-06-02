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
using Dev2.Runtime.Security;
using Dev2.Workspaces;
using Warewolf.Licensing;

namespace Dev2.Runtime.ESB.Management.Services
{
    //TODO: this service will be called once the user has completed registering using the UI. once that is completed it
    //call this service to save the CustomerId to the secureconfig and set the global constants.
    //it could also be deleted if we find it is not required.

    public class SaveLicenseKey : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            var result = new ExecuteMessage { HasError = false };
            Dev2Logger.Info("Save LicenseKey Service", GlobalConstants.WarewolfInfo);
            values.TryGetValue(Warewolf.Service.SaveLicenseKey.LicenseData, out var licenseData);

            var returnLicenseData = serializer.Deserialize<SubscriptionData>(licenseData);
            var subscription = new WarewolfLicense();
            var resultData = subscription.CreatePlan(returnLicenseData);

            if(resultData is null)
            {
                result.HasError = true;
            }

            var subscriptionData = HostSecurityProvider.Instance.UpdateSubscriptionData(resultData);
            if(!subscriptionData.IsLicensed)
            {
                result.HasError = true;
            }

            result.Message = serializer.SerializeToBuilder(subscriptionData);
            return serializer.SerializeToBuilder(result);
        }

        public override DynamicService CreateServiceEntry()
        {
            var newDs = new DynamicService {Name = HandlesType()};
            var sa = new ServiceAction {Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType()};
            newDs.Actions.Add(sa);
            return newDs;
        }

        public override string HandlesType() => nameof(Warewolf.Service.SaveLicenseKey);
    }
}
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
    public class GetSubscriptionData : DefaultEsbManagementEndpoint
    {
        private readonly ISubscriptionData _subscriptionDataInstance;
        private readonly IWarewolfLicense _warewolfLicense;

        public GetSubscriptionData()
            : this(new WarewolfLicense(), HostSecurityProvider.SubscriptionDataInstance)
        {
        }

        public GetSubscriptionData(IWarewolfLicense warewolfLicense, ISubscriptionData subscriptionData)
        {
            _warewolfLicense = warewolfLicense;
            _subscriptionDataInstance = subscriptionData;
        }

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            var result = new ExecuteMessage { HasError = false };
            Dev2Logger.Info("Get Subscription Data Service", GlobalConstants.WarewolfInfo);

            var subscriptionData = _warewolfLicense.RetrievePlan(_subscriptionDataInstance);

            if(subscriptionData.PlanId != _subscriptionDataInstance.PlanId || subscriptionData.Status != _subscriptionDataInstance.Status)
            {
                var updateSubscriptionData = HostSecurityProvider.Instance.UpdateSubscriptionData(subscriptionData);
                if(!updateSubscriptionData.IsLicensed)
                {
                    result.HasError = true;
                }
            }
            result.Message = serializer.SerializeToBuilder(subscriptionData);
            return serializer.SerializeToBuilder(result);
        }

        public override DynamicService CreateServiceEntry()
        {
            var newDs = new DynamicService { Name = HandlesType() };
            var sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);
            return newDs;
        }

        public override string HandlesType() => nameof(Warewolf.Service.GetSubscriptionData);
    }
}
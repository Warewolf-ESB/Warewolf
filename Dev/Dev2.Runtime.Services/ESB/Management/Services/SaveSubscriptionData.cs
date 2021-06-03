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
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Security;
using Dev2.Workspaces;
using Warewolf.Licensing;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveSubscriptionData : DefaultEsbManagementEndpoint
    {
        private readonly ISubscriptionData _subscriptionDataInstance;
        private readonly IWarewolfLicense _warewolfLicense;
        private IBuilderSerializer _serializer;

        public SaveSubscriptionData()
            : this(new Dev2JsonSerializer(), new WarewolfLicense(), HostSecurityProvider.SubscriptionDataInstance)
        {
        }

        public SaveSubscriptionData(IBuilderSerializer serializer, IWarewolfLicense warewolfLicense, ISubscriptionData subscriptionData)
        {
            _warewolfLicense = warewolfLicense;
            _subscriptionDataInstance = subscriptionData;
            _serializer = serializer;
        }

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var result = new ExecuteMessage { HasError = false };

            Dev2Logger.Info("Save Subscription Data Service", GlobalConstants.WarewolfInfo);
            values.TryGetValue(Warewolf.Service.SaveSubscriptionData.SubscriptionData, out var data);

            var subscriptionData = _serializer.Deserialize<SubscriptionData>(data);
            subscriptionData.SubscriptionKey = _subscriptionDataInstance.SubscriptionKey;
            subscriptionData.SubscriptionSiteName = _subscriptionDataInstance.SubscriptionSiteName;

            var resultData = _warewolfLicense.CreatePlan(subscriptionData);
            if(resultData is null)
            {
                result.HasError = true;
                result.SetMessage("An error occured.");
                return _serializer.SerializeToBuilder(result);
            }
            var updatedSubscriptionData = HostSecurityProvider.Instance.UpdateSubscriptionData(resultData);
            if(!updatedSubscriptionData.IsLicensed)
            {
                result.HasError = true;
            }

            result.Message = _serializer.SerializeToBuilder(updatedSubscriptionData);
            return _serializer.SerializeToBuilder(result);
        }

        public override DynamicService CreateServiceEntry()
        {
            var newDs = new DynamicService { Name = HandlesType() };
            var sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);
            return newDs;
        }

        public override string HandlesType() => nameof(Warewolf.Service.SaveSubscriptionData);
    }
}
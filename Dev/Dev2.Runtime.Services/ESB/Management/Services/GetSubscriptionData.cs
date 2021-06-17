/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Subscription;
using Dev2.Workspaces;
using Warewolf.Licensing;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetSubscriptionData : DefaultEsbManagementEndpoint
    {
        private ISubscriptionProvider _subscriptionProvider;
        private readonly IWarewolfLicense _warewolfLicense;

        public GetSubscriptionData()
            : this(new WarewolfLicense(), null)
        {
        }

        public GetSubscriptionData(IWarewolfLicense warewolfLicense, ISubscriptionProvider subscriptionData)
        {
            _warewolfLicense = warewolfLicense;
            _subscriptionProvider = subscriptionData;
        }

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            var result = new ExecuteMessage { HasError = false };
            try
            {
                Dev2Logger.Info("Get Subscription Data Service", GlobalConstants.WarewolfInfo);
                if(_subscriptionProvider == null)
                {
                    _subscriptionProvider = SubscriptionProvider.Instance;
                }

                if(!string.IsNullOrEmpty(_subscriptionProvider.SubscriptionId))
                {
                    var subscriptionData = _warewolfLicense.RetrievePlan(_subscriptionProvider.SubscriptionId, _subscriptionProvider.SubscriptionKey, _subscriptionProvider.SubscriptionSiteName);
                    if(subscriptionData.PlanId != _subscriptionProvider.PlanId || subscriptionData.Status != _subscriptionProvider.Status)
                    {
                        _subscriptionProvider.SaveSubscriptionData(subscriptionData);
                    }
                    result.Message = serializer.SerializeToBuilder(subscriptionData);
                }
                else
                {
                    var subscriptionData = new SubscriptionData
                    {
                        SubscriptionSiteName = _subscriptionProvider.SubscriptionSiteName,
                        SubscriptionKey = _subscriptionProvider.SubscriptionKey,
                        PlanId = _subscriptionProvider.PlanId,
                        Status = _subscriptionProvider.Status,
                        IsLicensed = _subscriptionProvider.IsLicensed
                    };
                    result.Message = serializer.SerializeToBuilder(subscriptionData);
                }
                return serializer.SerializeToBuilder(result);
            }
            catch(Exception e)
            {
                result.HasError = true;
                result.SetMessage(e.Message);
                Dev2Logger.Error(nameof(GetSubscriptionData), e, GlobalConstants.WarewolfError);
                return serializer.SerializeToBuilder(result);
            }
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
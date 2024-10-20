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
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Subscription;
using Dev2.Workspaces;
using Warewolf.Licensing;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveSubscriptionData : DefaultEsbManagementEndpoint
    {
        private readonly IWarewolfLicense _warewolfLicense;
        private readonly IBuilderSerializer _serializer;
        private ISubscriptionProvider _subscriptionProvider;

        public SaveSubscriptionData()
            : this(
                new Dev2JsonSerializer(),
                new WarewolfLicense(),
                SubscriptionProvider.Instance)
        {
        }

        public SaveSubscriptionData(
            IBuilderSerializer serializer,
            IWarewolfLicense warewolfLicense,
            ISubscriptionProvider subscriptionProvider)
        {
            _warewolfLicense = warewolfLicense;
            _serializer = serializer;
            _subscriptionProvider = subscriptionProvider;
        }

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var result = new ExecuteMessage { HasError = false };
            try
            {
                Dev2Logger.Info("Save Subscription Data Service", GlobalConstants.WarewolfInfo);
                if(_subscriptionProvider == null)
                {
                    _subscriptionProvider = SubscriptionProvider.Instance;
                }

                values.TryGetValue(Warewolf.Service.SaveSubscriptionData.SubscriptionData, out var data);
                var subscriptionData = _serializer.Deserialize<SubscriptionData>(data);
                subscriptionData.SubscriptionKey = _subscriptionProvider.SubscriptionKey;
                subscriptionData.SubscriptionSiteName = _subscriptionProvider.SubscriptionSiteName;

                if(string.IsNullOrEmpty(subscriptionData.SubscriptionId) && _warewolfLicense.SubscriptionExists(subscriptionData))
                {
                    result.SetMessage("A Subscription already exists for this Customer.  For help please contact support@warewolf.io");
                    return ReturnError(result);
                }

                var resultData = string.IsNullOrEmpty(subscriptionData.SubscriptionId)
                    ? _warewolfLicense.CreatePlan(subscriptionData)
                    : _warewolfLicense.RetrievePlan(
                        subscriptionData.SubscriptionId,
                        subscriptionData.SubscriptionKey,
                        subscriptionData.SubscriptionSiteName);

                if(resultData is null)
                {
                    result.SetMessage("An error occured. For help please contact support@warewolf.io");
                    return ReturnError(result);
                }

                if(resultData.CustomerEmail != subscriptionData.CustomerEmail)
                {
                    result.SetMessage("Email Address does not match email address used when this Subscription was created. For help please contact support@warewolf.io");
                    return ReturnError(result);
                }

                _subscriptionProvider.SaveSubscriptionData(resultData);
                result.SetMessage(GlobalConstants.Success);
                return _serializer.SerializeToBuilder(result);
            }
            catch(Exception e)
            {
                result.SetMessage(e.Message);
                return ReturnError(result);
            }
        }

        private StringBuilder ReturnError(IExecuteMessage result)
        {
            result.HasError = true;
            Dev2Logger.Error(nameof(SaveSubscriptionData), new Exception(result.Message.ToString()), GlobalConstants.WarewolfError);
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
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
using Dev2.Workspaces;
using Warewolf.License;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetLicenseKey : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            var result = new ExecuteMessage {HasError = false};
            try
            {
                Dev2Logger.Info("Get LicenseKey Service", GlobalConstants.WarewolfInfo);
                //TODO: Will get license data from Chargebee. Hardcoding info for now.
                var licenseData = new LicenseData
                {
                    CustomerId = "CustomerId",
                    Customer = "Customer Name",
                    PlanId = "Developer",
                    DaysLeft = 0,
                    IsValid = true
                };
                result.Message = serializer.SerializeToBuilder(licenseData);
            }
            catch (Exception err)
            {
                result.HasError = true;
                result.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }

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
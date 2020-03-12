#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchAllTests : EsbManagementEndpointBase
    {
        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            requestArgs.TryGetValue("resourceID", out StringBuilder tmp);
            if (tmp == null)
            {
                return Guid.Empty;
            }
            return Guid.TryParse(tmp.ToString(), out Guid resourceId) ? resourceId : Guid.Empty;
        }

        public override AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        ITestCatalog _testCatalog;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Fetch All Tests Service", GlobalConstants.WarewolfInfo);

                var tests = TestCatalog.FetchAllTests();
                var message = new CompressedExecuteMessage();
                message.SetMessage(serializer.Serialize(tests));
                message.HasError = false;

                return serializer.SerializeToBuilder(message);
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                var res = new CompressedExecuteMessage { HasError = true, Message = new StringBuilder(err.Message) };
                return serializer.SerializeToBuilder(res);
            }
        }

        public ITestCatalog TestCatalog
        {
            get => _testCatalog ?? Runtime.TestCatalog.Instance;
            set => _testCatalog = value;
        }

        public override DynamicService CreateServiceEntry()
        {
            var newDs = new DynamicService { Name = HandlesType() };
            var sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public override string HandlesType() => "FetchAllTests";
    }
}

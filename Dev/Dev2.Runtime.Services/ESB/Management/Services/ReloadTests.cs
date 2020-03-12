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
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class ReloadTests : EsbManagementEndpointBase
    {
        #region Implementation of IEsbManagementEndpoint
        
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var result = new CompressedExecuteMessage { HasError = false };
            try
            {
                TestCatalog.Load();
                result.SetMessage("Test reload succesful");
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.SetMessage("Error reloading tests...");
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
            }

            var serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(result);
        }

        public override  Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            requestArgs.TryGetValue("resourceID", out StringBuilder tmp);
            if (tmp != null && Guid.TryParse(tmp.ToString(), out Guid resourceId))
            {
                return resourceId;
            }

            return Guid.Empty;
        }

        public override  AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public ITestCatalog TestCatalog
        {
            private get => _testCatalog ?? Runtime.TestCatalog.Instance;
            set => _testCatalog = value;
        }
        ITestCatalog _testCatalog;

        #endregion

        public override  DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceID ColumnIODirection=\"Input\"/><ResourceType ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override  string HandlesType() => "ReloadTestsService";
    }
}

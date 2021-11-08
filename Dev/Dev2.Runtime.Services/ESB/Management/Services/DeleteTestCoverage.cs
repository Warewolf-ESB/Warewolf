#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;


namespace Dev2.Runtime.ESB.Management.Services
{
    public class DeleteTestCoverage : IEsbManagementEndpoint
    {
        ITestCoverageCatalog _testCoverageCatalog;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            requestArgs.TryGetValue("resourceID", out StringBuilder tmp);
            if (tmp != null && Guid.TryParse(tmp.ToString(), out Guid resourceId))
            {
                return resourceId;
            }


            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Delete Test Service", GlobalConstants.WarewolfInfo);

                values.TryGetValue("resourceID", out StringBuilder resourceIdString);
                if (resourceIdString == null)
                {
                    throw new InvalidDataContractException("resourceID is missing");
                }
                if (!Guid.TryParse(resourceIdString.ToString(), out Guid resourceId))
                {
                    throw new InvalidDataContractException("resourceID is not a valid GUID.");
                }                
                TestCoverageCatalog.DeleteAllCoverageReports(resourceId);
               
                var message = new CompressedExecuteMessage { HasError = false };
                return serializer.SerializeToBuilder(message);
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                var res = new CompressedExecuteMessage { HasError = true, Message = new StringBuilder(err.Message) };
                return serializer.SerializeToBuilder(res);
            }
        }

        public ITestCoverageCatalog TestCoverageCatalog
        {
            private get
            {
                return _testCoverageCatalog ?? Runtime.TestCoverageCatalog.Instance;
            }
            set
            {
                _testCoverageCatalog = value;
            }
        }

        public DynamicService CreateServiceEntry()
        {
            var newDs = new DynamicService { Name = HandlesType() };
            var sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType() => "DeleteTestCoverage";
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class DeleteAllTests : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            CompressedExecuteMessage result = new CompressedExecuteMessage { HasError = false };
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            try
            {
                values.TryGetValue("excludeList", out StringBuilder excludeTests);
                var testsToLive = jsonSerializer.Deserialize<List<string>>(excludeTests);
                TestCatalog.DeleteAllTests(testsToLive.Select(a => a.ToUpper()).ToList());
                result.SetMessage("Test reload succesful");
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.SetMessage("Error reloading tests...");
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(result);
        }
        
        public DynamicService CreateServiceEntry()
        {
            DynamicService reloadResourceServicesBinder = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = new StringBuilder("<DataList><ResourceID ColumnIODirection=\"Input\"/><ResourceType ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
            };
            using (ServiceAction reloadResourceServiceActionBinder = new ServiceAction
            {
                Name = HandlesType(),
                SourceMethod = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService
            })
            {
                reloadResourceServicesBinder.Actions.Add(reloadResourceServiceActionBinder);
                return reloadResourceServicesBinder;
            }
        }

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            requestArgs.TryGetValue("resourceID", out StringBuilder tmp);
            if (tmp == null)
            {
                return Guid.Empty;
            }
            return Guid.TryParse(tmp.ToString(), out Guid resourceId) ? resourceId : Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Contribute;
        }

        public string HandlesType()
        {
            return "DeleteAllTestsService";
        }
        public ITestCatalog TestCatalog
        {
            private get
            {
                return _testCatalog ?? Runtime.TestCatalog.Instance;
            }
            set
            {
                _testCatalog = value;
            }
        }
        private ITestCatalog _testCatalog;
    }
}
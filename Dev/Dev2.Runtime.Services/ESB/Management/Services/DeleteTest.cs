using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
    public class DeleteTest : IEsbManagementEndpoint
    {
        private ITestCatalog _testCatalog;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            requestArgs.TryGetValue("resourceID", out StringBuilder tmp);
            if (tmp != null)
            {
                if (Guid.TryParse(tmp.ToString(), out Guid resourceId))
                {
                    return resourceId;
                }
            }

            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Contribute;
        }
        
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
                values.TryGetValue("testName", out StringBuilder testName);
                if (string.IsNullOrEmpty(testName?.ToString()))
                {
                    throw new InvalidDataContractException("testName is missing");
                }

                TestCatalog.DeleteTest(resourceId,testName.ToString());
                CompressedExecuteMessage message = new CompressedExecuteMessage { HasError = false };
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
            private get
            {
                return _testCatalog ?? Runtime.TestCatalog.Instance;
            }
            set
            {
                _testCatalog = value;
            }
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType() };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "DeleteTest";
        }
    }
}
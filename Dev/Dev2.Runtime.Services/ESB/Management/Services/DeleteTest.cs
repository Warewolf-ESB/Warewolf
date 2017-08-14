using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
// ReSharper disable MemberCanBeInternal

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Adds a resource
    /// </summary>

    public class DeleteTest : IEsbManagementEndpoint
    {
        private ITestCatalog _testCatalog;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            StringBuilder tmp;
            requestArgs.TryGetValue("resourceID", out tmp);
            if (tmp != null)
            {
                Guid resourceId;
                if (Guid.TryParse(tmp.ToString(), out resourceId))
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
                Dev2Logger.Info("Delete Test Service");

                StringBuilder resourceIdString;
                values.TryGetValue("resourceID", out resourceIdString);
                if (resourceIdString == null)
                {
                    throw new InvalidDataContractException("resourceID is missing");
                }
                Guid resourceId;
                if (!Guid.TryParse(resourceIdString.ToString(), out resourceId))
                {
                    throw new InvalidDataContractException("resourceID is not a valid GUID.");
                }
                StringBuilder testName;
                values.TryGetValue("testName", out testName);
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
                Dev2Logger.Error(err);
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
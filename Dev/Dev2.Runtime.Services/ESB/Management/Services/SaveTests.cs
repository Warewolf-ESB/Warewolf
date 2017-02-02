using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Interfaces;
using Dev2.Services.Security;
using Dev2.Workspaces;
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Adds a resource
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SaveTests : IEsbManagementEndpoint
    {
        private ITestCatalog _testCatalog;
        private IResourceCatalog _resourceCatalog;

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
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Save Tests Service");
                StringBuilder testDefinitionMessage;
                StringBuilder resourceIdString;
                StringBuilder resourcePathString;
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
                values.TryGetValue("resourcePath", out resourcePathString);
                if (resourcePathString == null)
                {
                    throw new InvalidDataContractException("resourcePath is missing");
                }
                values.TryGetValue("testDefinitions", out testDefinitionMessage);
                if (testDefinitionMessage == null || testDefinitionMessage.Length == 0)
                {
                    throw new InvalidDataContractException("testDefinition is missing");
                }
                var res = new ExecuteMessage
                {
                    HasError = false,
                    Message = serializer.SerializeToBuilder(new TestSaveResult
                    {
                        Result = SaveResult.Success,
                    })
                };

                var decompressedMessage = serializer.Deserialize<CompressedExecuteMessage>(testDefinitionMessage).GetDecompressedMessage();
                var serviceTestModelTos = serializer.Deserialize<List<IServiceTestModelTO>>(decompressedMessage);
                var resource = ResourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);
                if (resource == null)
                {
                    var testResult = new TestSaveResult
                    {
                        Result = SaveResult.ResourceDeleted,
                        Message = $"Resource {resourcePathString} has been deleted. No Tests can be saved for this resource."
                    };
                    res.Message = serializer.SerializeToBuilder(testResult);
                }
                else
                {
                    var resourcePath = resource.GetResourcePath(GlobalConstants.ServerWorkspaceID);
                    if (!resourcePath.Equals(resourcePathString.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        var testResult = new TestSaveResult
                        {
                            Result = SaveResult.ResourceUpdated,
                            Message = $"Resource {resourcePathString} has changed to {resourcePath}. Tests have been saved for this resource."
                        };
                        res.Message = serializer.SerializeToBuilder(testResult);
                    }
                    TestCatalog.SaveTests(resourceId, serviceTestModelTos);
                }
                return serializer.SerializeToBuilder(res);
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err);
                var res = new ExecuteMessage { HasError = true, Message = new StringBuilder(err.Message) };
                return serializer.SerializeToBuilder(res);
            }
        }

        public ITestCatalog TestCatalog
        {
            get
            {
                return _testCatalog ?? Runtime.TestCatalog.Instance;
            }
            set
            {
                _testCatalog = value;
            }
        }

        public IResourceCatalog ResourceCatalog
        {
            get
            {
                return _resourceCatalog ?? Hosting.ResourceCatalog.Instance;
            }
            set
            {
                _resourceCatalog = value;
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
            return "SaveTests";
        }
    }
}
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
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;


namespace Dev2.Runtime.ESB.Management.Services
{

    public class SaveTests : IEsbManagementEndpoint
    {
        ITestCatalog _testCatalog;
        IResourceCatalog _resourceCatalog;

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
                Dev2Logger.Info("Save Tests Service", GlobalConstants.WarewolfInfo);
                values.TryGetValue("resourceID", out StringBuilder resourceIdString);
                if (resourceIdString == null)
                {
                    throw new InvalidDataContractException("resourceID is missing");
                }
                if (!Guid.TryParse(resourceIdString.ToString(), out Guid resourceId))
                {
                    throw new InvalidDataContractException("resourceID is not a valid GUID.");
                }
                values.TryGetValue("resourcePath", out StringBuilder resourcePathString);
                if (resourcePathString == null)
                {
                    throw new InvalidDataContractException("resourcePath is missing");
                }
                values.TryGetValue("testDefinitions", out StringBuilder testDefinitionMessage);
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
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
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
            var newDs = new DynamicService { Name = HandlesType() };
            var sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType() => "SaveTests";
    }
}
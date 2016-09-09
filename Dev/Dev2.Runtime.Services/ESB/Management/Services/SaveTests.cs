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
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Adds a resource
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SaveTests : IEsbManagementEndpoint
    {
        private ITestCatalog _testCatalog;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Save Tests Service");
                StringBuilder testDefinitionMessage;

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
                values.TryGetValue("testDefinitions", out testDefinitionMessage);
                if (testDefinitionMessage == null || testDefinitionMessage.Length == 0)
                {
                    throw new InvalidDataContractException("testDefinition is missing");
                }

                var serviceTestModelTos = serializer.Deserialize<List<IServiceTestModelTO>>(serializer.Deserialize<CompressedExecuteMessage>(testDefinitionMessage).GetDecompressedMessage());
                var res = new ExecuteMessage { HasError = false, Message = serializer.SerializeToBuilder(serviceTestModelTos) };
                TestCatalog.SaveTests(resourceId, serviceTestModelTos);
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
                return _testCatalog ?? new TestCatalog();
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
            return "SaveTests";
        }
    }
}
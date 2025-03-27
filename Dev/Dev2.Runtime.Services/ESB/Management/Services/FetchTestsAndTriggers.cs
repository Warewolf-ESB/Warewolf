using Dev2.Common.Interfaces;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Triggers;
 
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Services.ServiceModel.Data;

namespace Dev2.Runtime.Services.ESB.Management.Services
{
    public class FetchTestsAndTriggers : DefaultEsbManagementEndpoint
    {
        ITestCatalog _testCatalog;
        ITriggersCatalog _triggersCatalog;

        public override DynamicService CreateServiceEntry()
        {
            return EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><resourceIDs ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");
        }

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Fetch Tests and Triggers for deploy", GlobalConstants.WarewolfInfo);

                if (!values.TryGetValue("resourceIDs", out var resourceIdsString) || resourceIdsString == null)
                {
                    throw new InvalidDataContractException("resourceIDs is missing");
                }

                var resourceIds = serializer.Deserialize<List<Guid>>(resourceIdsString);
                var totalTests = 0;
                var totalTriggers = 0;

                foreach (var resourceId in resourceIds)
                {
                    var tests = TestCatalog.Fetch(resourceId);
                    if (tests != null)
                    {
                        totalTests += tests.Count;
                    }

                    var triggers = TriggersCatalog.LoadQueuesByResourceId(resourceId);
                    if (triggers != null)
                    {
                        totalTriggers += triggers.Count;
                    }
                }

                var result = new ResourceTestTriggerData
                {
                    TestsCount = totalTests,
                    TriggersCount = totalTriggers
                };

                var message = new CompressedExecuteMessage();
                message.SetMessage(serializer.Serialize(result));
                message.HasError = false;
                return serializer.SerializeToBuilder(message);
            }
            catch (Exception e)
            {
                Dev2Logger.Error("FetchTestsAndTriggers error", e, GlobalConstants.WarewolfError);
                var errorMsg = new CompressedExecuteMessage { HasError = true, Message = new StringBuilder(e.Message) };
                return serializer.SerializeToBuilder(errorMsg);
            }
        }

        public ITestCatalog TestCatalog
        {
            get => _testCatalog ?? Runtime.TestCatalog.Instance;
            set => _testCatalog = value;
        }

        public ITriggersCatalog TriggersCatalog
        {
            get => _triggersCatalog ?? Hosting.TriggersCatalog.Instance;
            set => _triggersCatalog = value;
        }

        public override string HandlesType() => nameof(FetchTestsAndTriggers);
    }
}

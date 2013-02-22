using System;
using System.Collections.Generic;

namespace Dev2.DynamicServices {
    public interface IDynamicServicesHost {
        dynamic AddBizRule(BizRule bizRule);
        dynamic AddDynamicService(DynamicService dynamicService, string roles, string resourceDef = null, bool saveResource=true);
        dynamic AddResources(List<DynamicServiceObjectBase> resources, string roles);
        void RestoreResources(string[] directoryNames, string resourceName="");
        dynamic AddSource(Source source, string roles, bool saveResource=true);
        dynamic AddWorkflowActivity(WorkflowActivityDef activity, string roles, bool saveResource = true);
        dynamic RemoveDynamicService(DynamicService dynamicService, string roles, bool deleteResource = true);
        dynamic RemoveSource(Source source, string roles, bool deleteResource = true);
        void MapActivityToService(WorkflowActivityDef activity);
        void MapServiceActionDependencies(ServiceAction serviceAction);

        void LockServices();
        void LockSources();
        void LockReservedServices();
        void LockReservedSources();
        void LockActivities();
        void UnlockServices();
        void UnlockSources();
        void UnlockReservedServices();
        void UnlockReservedSources();
        void UnlockActivities();


        List<BizRule> BizRules { get; set; }
        List<DynamicServiceObjectBase> GenerateObjectGraphFromString(string serviceDefinitionsXml);
        Guid InvokeService(dynamic xmlRequest, Guid dataListID);
        List<DynamicService> Services { get; set; }
        List<DynamicService> ReservedServices { get; set; }
        List<Source> ReservedSources { get; set; }
        List<Source> Sources { get; set; }
        List<WorkflowActivityDef> WorkflowActivityDefs { get; set; }
        void SendMessageToConnectedClients(string message);
    }
}

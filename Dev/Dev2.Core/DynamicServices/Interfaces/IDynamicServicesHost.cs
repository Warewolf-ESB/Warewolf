using System;
using System.Collections.Generic;

namespace Dev2.DynamicServices {
    public interface IDynamicServicesHost {
        string AddResources(List<DynamicServiceObjectBase> resources, string roles);
        void RestoreResources(string[] directoryNames, string resourceName="");
        string AddDynamicService(DynamicService dynamicService, string roles, string resourceDef = null, bool saveResource = true);
        string AddSource(Source source, string roles, bool saveResource=true);
        //dynamic AddWorkflowActivity(WorkflowActivityDef activity, string roles, bool saveResource = true);
        string RemoveDynamicService(DynamicService dynamicService, string roles, bool deleteResource = true);
        string RemoveSource(Source source, string roles, bool deleteResource = true);
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

        #region Additional Mgt Methods

        DynamicService FindServiceByName(string serviceName);

        string FindServiceShape(string serviceName);

        #endregion

        #region Missing Methods from Interface as per TWR's work

        void CopyTo(string destWorkspacePath, bool overwrite = false, IList<string> filesToIgnore = null);

        IDynamicServiceObject Find(string serviceName, enDynamicServiceObjectType serviceType);

        void SyncTo(string destWorkspacePath, bool overwrite = true, bool delete = true, IList<string> filesToIgnore = null);

        bool RollbackResource(string resourceName, int versionNo, string resourceType = "Service");

        // Missing Property
        string WorkspacePath { get; }


        #endregion


        List<DynamicServiceObjectBase> GenerateObjectGraphFromString(string serviceDefinitionsXml);
        List<DynamicService> Services { get; set; }
        List<DynamicService> ReservedServices { get; set; }
        List<Source> ReservedSources { get; set; }
        List<Source> Sources { get; set; }
        void SendMessageToConnectedClients(string message);
    }
}

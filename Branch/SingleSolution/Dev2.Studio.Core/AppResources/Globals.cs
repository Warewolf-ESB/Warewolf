using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.AppResources {

    public delegate void OperatorTypeEventHandler(string Expression);
    public delegate void ResourceEventHandler(IResourceModel IResourceModel);
    
    public delegate void CancelOperationEventHandler(object cancelledobject, object[] arguments);
    public delegate void ClosedOperationEventHandler(object closedobject, object[] arguments);
    public delegate void EnvironmentCreatedEventHandler(IEnvironmentModel environment);
    

    public delegate void RoleEventHandler(string Roles);
    public delegate void StringMessageEventHandler(string stringMsg);
    public delegate void InputDataReceivedEventHandler(string inputData, double transitionPeriod);
    public delegate void ObjectSelectedEventHandler(object dataObject);
    public enum DebugMode { Run, DebugInteractive }
    public enum enDsfChannelMode { Development, Test, Live }
    public enum enResourceMode{ New, Existing};


}

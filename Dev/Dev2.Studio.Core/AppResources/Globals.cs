using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources
{

    public delegate void OperatorTypeEventHandler(string expression);
    public delegate void ResourceEventHandler(IResourceModel resourceModel);

    public delegate void CancelOperationEventHandler(object cancelledobject, object[] arguments);
    public delegate void ClosedOperationEventHandler(object closedobject, object[] arguments);
    public delegate void EnvironmentCreatedEventHandler(IEnvironmentModel environment);


    public delegate void RoleEventHandler(string roles);
    public delegate void StringMessageEventHandler(string stringMsg);
    public delegate void InputDataReceivedEventHandler(string inputData, double transitionPeriod);
    public delegate void ObjectSelectedEventHandler(object dataObject);
    public enum DebugMode { Run, DebugInteractive }
    // ReSharper disable once InconsistentNaming
    public enum enDsfChannelMode { Development, Test, Live }
    // ReSharper disable once InconsistentNaming
    public enum enResourceMode { New, Existing };


}

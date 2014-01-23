
namespace Dev2.Instrumentation
{
    public enum TrackerEventGroup
    {
        Workflows,
        ActivityExecution,       
        Deploy,
        Settings
    }

    public enum TrackerEventName
    {
        Execute,
        OpenSettings,
        SaveSettings,
        OpenDeploy,
        Deploy,
        ViewInBrowser,
        Debug
    }
}

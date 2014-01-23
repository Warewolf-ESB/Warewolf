
namespace Dev2.Instrumentation
{
    public enum TrackerEventGroup
    {
        Workflows,
        ActivityExecution,       
        Deploy,
        Settings,
        Installations
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

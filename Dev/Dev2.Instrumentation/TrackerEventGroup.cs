
namespace Dev2.Instrumentation
{
    public enum TrackerEventGroup
    {
        Workflows,
        ActivityExecution,       
        Tabs,
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

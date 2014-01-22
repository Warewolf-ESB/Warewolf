
namespace Dev2.Instrumentation
{
    public enum TrackerEventGroup
    {
        Workflows,
        ActivityExecution,
        Tabs
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

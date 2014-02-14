
namespace Dev2.Instrumentation
{
    public enum TrackerEventGroup
    {
        Installations,
        Workflows,
        ActivityExecution,
        Deploy,
        Settings,
        Exception
    }

    public enum TrackerEventName
    {
        Installed,
        Uninstalled,
        Executed,
        Opened,
        SaveClicked,
        DeployClicked,
        ViewInBrowserClicked,
        DebugClicked,
    }
}

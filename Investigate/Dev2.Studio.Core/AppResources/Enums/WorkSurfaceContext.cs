using Dev2.Studio.Core.AppResources.Attributes;
using System.ComponentModel;

namespace Dev2.Studio.Core.AppResources.Enums
{
    public enum WorkSurfaceContext
    {
        Unknown,
        Workflow,
        Service,
        Webpage,
        Website,
        Webpart,
        Wizard,
        SourceManager,
        ConnectionManager,
        ScheduleManager,
        PeopleAndSecurityManager,
        BackupManager,

        [Description("Reports Manager")]
        ReportsManager,
        WebserverManager,

        [IconLocation("pack://application:,,,/images/Settings-16.png")]
        [Description("Settings")]
        Settings,
        BuyOrBrowse,
        Sell,
        AppstoreAccount,
        Installed,
        CommunityAccount,
        CommunitySearch,

        [IconLocation("pack://application:,,,/images/DependencyGraph-16.png")]
        [Description("Dependency Visualiser")]
        DependencyVisualiser,

        [IconLocation("Pack_Uri_Application_Image_Information", typeof(StringResources))]
        [Description("Shortcut Keys")]
        ShortcutKeys,

        [IconLocation("Pack_Uri_Application_Image_Help", typeof(StringResources))]
        [Description("Help")]
        Help,
        Community,

        [IconLocation("/images/Deploy-32.png")]
        [Description("Deploy")]
        DeployResources,

        [IconLocation("Pack_Uri_Application_Image_Home", typeof(StringResources))]
        [Description("Start Page")]
        StartPage,
        [IconLocation("pack://application:,,,/images/DependencyGraph-16.png")]
        [Description("Reverse Dependency Visualiser")]
        ReverseDependencyVisualiser
    }

}

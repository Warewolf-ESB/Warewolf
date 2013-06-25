using System.ComponentModel;
using Dev2.Studio.Core.AppResources.Attributes;

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

        [IconLocation("pack://application:,,,/images/ReportsManager-32.png")]
        [Description("Reports Viewer")]
        ReportsManager,
        WebserverManager,

        [IconLocation("pack://application:,,,/images/Settings-32.png")]
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
        ReverseDependencyVisualiser,

        [IconLocation("Pack_Uri_Application_Image_Community", typeof(StringResources))]
        [Description("Community Portal")]
        CommunityPage,
    }

}

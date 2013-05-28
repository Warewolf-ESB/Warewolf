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
        ReportsManager,
        WebserverManager,

        [IconLocation("pack://application:,,,/images/settingsIcon.png")]
        [Description("Settings")]
        Settings,
        BuyOrBrowse,
        Sell,
        AppstoreAccount,
        Installed,
        CommunityAccount,
        CommunitySearch,

        [IconLocation("pack://application:,,,/images/dependency.png")]
        [Description("Dependency Visualiser")]
        DependencyVisualiser,

        [IconLocation("Pack_Uri_Application_Image_Information", typeof(StringResources))]
        [Description("Shortcut Keys")]
        ShortcutKeys,

        [IconLocation("Pack_Uri_Application_Image_Help", typeof(StringResources))]
        [Description("Help")]
        Help,
        Community,

        [IconLocation("/images/database_save.png")]
        [Description("Deploy")]
        DeployResources,

        [IconLocation("Pack_Uri_Application_Image_Home", typeof(StringResources))]
        [Description("Start Page")]
        StartPage,
        [IconLocation("pack://application:,,,/images/dependency.png")]
        [Description("Reverse Dependency Visualiser")]
        ReverseDependencyVisualiser
    }

}

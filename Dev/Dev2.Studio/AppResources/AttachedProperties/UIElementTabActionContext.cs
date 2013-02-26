using System.Windows;

namespace Dev2.Studio.AppResources.AttachedProperties
{
    public enum TabActionContext
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
        Settings,
        BuyOrBrowse,
        Sell,
        AppstoreAccount,
        Installed,
        CommunityAccount,
        CommunitySearch,
        HelpSearch,
        Community,
        DeployResources
    }

    public static class UIElementTabActionContext
    {
        public static void SetTabActionContext(UIElement element, TabActionContext value)
        {
            element.SetValue(tabActionContextProperty, value);
        }

        public static TabActionContext GetTabActionContext(UIElement element)
        {
            return (TabActionContext)element.GetValue(tabActionContextProperty);
        }

        public static readonly DependencyProperty tabActionContextProperty =
            DependencyProperty.RegisterAttached("TabActionContext", typeof(TabActionContext), typeof(UIElementTabActionContext), new PropertyMetadata(TabActionContext.Unknown));
    }
}

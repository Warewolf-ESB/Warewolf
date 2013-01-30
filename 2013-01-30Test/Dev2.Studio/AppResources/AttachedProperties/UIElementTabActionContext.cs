using System.Windows;

namespace Dev2.Studio.AppResources.AttachedProperties
{
    public enum TabActionContexts
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
        public static void SetTabActionContext(UIElement element, TabActionContexts value)
        {
            element.SetValue(tabActionContextProperty, value);
        }

        public static TabActionContexts GetTabActionContext(UIElement element)
        {
            return (TabActionContexts)element.GetValue(tabActionContextProperty);
        }

        public static readonly DependencyProperty tabActionContextProperty =
            DependencyProperty.RegisterAttached("TabActionContext", typeof(TabActionContexts), typeof(UIElementTabActionContext), new PropertyMetadata(TabActionContexts.Unknown));
    }
}

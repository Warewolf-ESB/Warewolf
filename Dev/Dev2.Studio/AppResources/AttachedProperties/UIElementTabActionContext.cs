using System.Windows;
using Dev2.Studio.Core.AppResources.Enums;

namespace Dev2.Studio.AppResources.AttachedProperties
{
    public static class UIElementTabActionContext
    {
        public static void SetTabActionContext(UIElement element, WorkSurfaceContext value)
        {
            element.SetValue(tabActionContextProperty, value);
        }

        public static WorkSurfaceContext GetTabActionContext(UIElement element)
        {
            return (WorkSurfaceContext)element.GetValue(tabActionContextProperty);
        }

        public static readonly DependencyProperty tabActionContextProperty =
            DependencyProperty.RegisterAttached("WorkSurfaceContext", typeof(WorkSurfaceContext), 
            typeof(UIElementTabActionContext), new PropertyMetadata(WorkSurfaceContext.Unknown));
    }
}

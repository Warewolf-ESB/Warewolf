
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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

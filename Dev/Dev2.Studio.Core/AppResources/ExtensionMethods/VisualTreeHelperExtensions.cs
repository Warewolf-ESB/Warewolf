using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Dev2.Studio.Core.AppResources.ExtensionMethods
{
    public static class VisualTreeHelperExtensions
    {
        public static DependencyObject GetChildByType(DependencyObject source, Type type)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(source); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(source, i);

                if (child.GetType() == type)
                {
                    return child;
                }
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(source); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(source, i);
                DependencyObject nestedchild = GetChildByType(child, type);
                if (nestedchild != null)
                {
                    return nestedchild;
                }
            }

            return null;
        }

        public static DependencyObject GetParentByType(this DependencyObject source, Type type)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(source);

            if(parent == null) return null;

            return parent.GetType() == type ? parent : GetParentByType(source, type);
        }
    }
}

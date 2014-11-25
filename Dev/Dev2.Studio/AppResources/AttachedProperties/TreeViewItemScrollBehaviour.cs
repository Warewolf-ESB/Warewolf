
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
using System.Windows.Controls;

namespace Unlimited.Applications.BusinessDesignStudio {
    public static class BringIntoViewProperty {

        public static bool GetIsInView(DependencyObject obj) {
            return (bool)obj.GetValue(IsInViewProperty);
        }

        public static void SetIsInView(DependencyObject obj, bool value) {
            obj.SetValue(IsInViewProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsInView.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsInViewProperty =
            DependencyProperty.RegisterAttached("IsInView", typeof(bool), typeof(BringIntoViewProperty), new UIPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChangedCallback)));

        private static void OnPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var treeViewItem = d as TreeViewItem;
            if(treeViewItem != null){
                treeViewItem.BringIntoView();
            }
        }        
        
    }
}

/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
        // Using a DependencyProperty as the backing store for IsInView.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsInViewProperty =
            DependencyProperty.RegisterAttached("IsInView", typeof(bool), typeof(BringIntoViewProperty), new UIPropertyMetadata(false, OnPropertyChangedCallback));

        private static void OnPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var treeViewItem = d as TreeViewItem;
            if(treeViewItem != null){
                treeViewItem.BringIntoView();
            }
        }        
        
    }
}

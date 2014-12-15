
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class PopupBuildVisualTreeOnLoad : Behavior<Popup>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;

            AssociatedObject.Visibility = System.Windows.Visibility.Hidden;
            AssociatedObject.IsOpen = true;
            AssociatedObject.IsOpen = false;
            AssociatedObject.Visibility = System.Windows.Visibility.Visible;
        }
    }
}

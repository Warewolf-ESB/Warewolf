using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Dev2.CustomControls.Trigger
{
    public class TreeViewItemExpandTrigger : ParentKeyPressedTrigger<TreeViewItem>
    {
        private bool _invokeOnNextPress;

        public TreeViewItemExpandTrigger()
        {
            Key = Key.Right;
        }

        protected override void ProcessKeyPress(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key && _invokeOnNextPress)
            {
                keyEventArgs.Handled = true;
                Dispatcher.BeginInvoke(() => InvokeActions(null));
            }
            else if (keyEventArgs.Key == Key && !_invokeOnNextPress)
            {
                keyEventArgs.Handled = true;
                Parent.IsExpanded = true;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            Parent.Expanded += Parent_Expanded;
            Parent.Collapsed += Parent_Collapsed;
        }

        void Parent_Collapsed(object sender, System.Windows.RoutedEventArgs e)
        {
            _invokeOnNextPress = false;
        }

        void Parent_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            _invokeOnNextPress = true;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            Parent.Expanded -= Parent_Expanded;
            Parent.Collapsed -= Parent_Collapsed;
        }

    }
}

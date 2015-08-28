/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Threading;

namespace Dev2.CustomControls.Trigger
{
    public class KeyPressedTrigger : TriggerBase<FrameworkElement>
    {
        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("MyProperty", typeof (Key),
                typeof (KeyPressedTrigger), new PropertyMetadata(Key.Enter));

        public Key Key
        {
            get { return (Key) GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }


        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewKeyDown += ProcessKeyPress;
        }

        protected virtual void ProcessKeyPress(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key)
            {
                Dispatcher.BeginInvoke(() => InvokeActions(null));
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewKeyDown -= ProcessKeyPress;
            base.OnDetaching();
        }
    }
}
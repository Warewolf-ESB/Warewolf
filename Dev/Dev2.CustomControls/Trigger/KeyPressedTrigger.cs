using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Threading;

namespace Dev2.CustomControls.Trigger
{
    public class KeyPressedTrigger : TriggerBase<FrameworkElement>
    {    
        public Key Key
        {
            get { return (Key)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("MyProperty", typeof(Key), 
            typeof(KeyPressedTrigger), new PropertyMetadata(Key.Enter));

        
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

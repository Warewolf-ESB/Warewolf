using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class TextboxSetFocusOnCreationBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateVisibility();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        public bool ServerIsNotBusyRenaming
        {
            get { return (bool)GetValue(ServerIsNotBusyRenamingProperty); }
            set { SetValue(ServerIsNotBusyRenamingProperty, value); }
        }

        public static readonly DependencyProperty ServerIsNotBusyRenamingProperty =
            DependencyProperty.Register("ServerIsNotBusyRenaming", typeof(bool), typeof(TextboxSetFocusOnCreationBehavior), 
            new PropertyMetadata(true, ServerBusyRenamingChangedCallback));

        private static void ServerBusyRenamingChangedCallback(DependencyObject o, 
            DependencyPropertyChangedEventArgs args)
        {
            var behavior = (TextboxSetFocusOnCreationBehavior) o;
            behavior.UpdateVisibility();
        }

        public bool IsRenaming
        {
            get { return (bool)GetValue(IsRenamingProperty); }
            set { SetValue(IsRenamingProperty, value); }
        }

        public static readonly DependencyProperty IsRenamingProperty =
            DependencyProperty.Register("IsRenaming", typeof(bool), typeof(TextboxSetFocusOnCreationBehavior), 
            new PropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            var behavior = (TextboxSetFocusOnCreationBehavior) o;
            behavior.UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (IsRenaming && ServerIsNotBusyRenaming)
            {
                AssociatedObject.Visibility = Visibility.Visible;
                AssociatedObject.Focus();
            }
            else
            {
                AssociatedObject.Visibility = Visibility.Collapsed;
            }
        }
    }
}

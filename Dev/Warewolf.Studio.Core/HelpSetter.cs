using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.Core
{
    public class HelpSetter : Behavior<FrameworkElement>
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(HelpSetter), new PropertyMetadata(null));

        public IUpdatesHelp DataContext
        {
            get { return (IUpdatesHelp)GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }

        public static readonly DependencyProperty DataContextProperty =
            DependencyProperty.Register("DataContext", typeof(IUpdatesHelp), typeof(HelpSetter), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            FocusManager.AddGotFocusHandler(AssociatedObject, OnGotFocus);
        }

        protected override void OnDetaching()
        {
            FocusManager.RemoveGotFocusHandler(AssociatedObject, OnGotFocus);
            base.OnDetaching();
        }

        void OnGotFocus(object sender, RoutedEventArgs args)
        {
            if(DataContext != null)
            {
                DataContext.UpdateHelpDescriptor(Text);
            }
        }
    }
}
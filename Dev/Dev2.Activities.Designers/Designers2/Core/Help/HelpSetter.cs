
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

namespace Dev2.Activities.Designers2.Core.Help
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

        public IHelpSource DataContext
        {
            get { return (IHelpSource)GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }

        public static readonly DependencyProperty DataContextProperty =
            DependencyProperty.Register("DataContext", typeof(IHelpSource), typeof(HelpSetter), new PropertyMetadata(null));

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
                DataContext.HelpText = Text;
            }
        }
    }
}

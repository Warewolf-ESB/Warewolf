#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(HelpSetter), new PropertyMetadata(null));

        public IUpdatesHelp DataContext
        {
            get => (IUpdatesHelp)GetValue(DataContextProperty);
            set => SetValue(DataContextProperty, value);
        }

        public static readonly DependencyProperty DataContextProperty =
            DependencyProperty.Register("DataContext", typeof(IUpdatesHelp), typeof(HelpSetter), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            FocusManager.AddGotFocusHandler(AssociatedObject, OnGotFocus);
        }

        void OnGotFocus(object sender, RoutedEventArgs args)
        {
            DataContext?.UpdateHelpDescriptor(Text);
        }
    }
}
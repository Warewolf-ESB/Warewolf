
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;

namespace Dev2.Activities.Designers2.Core.Errors
{
    public class ErrorsSetter : Behavior<FrameworkElement>
    {
        public IErrorsSource Target
        {
            get { return (IErrorsSource)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(IErrorsSource), typeof(ErrorsSetter), new PropertyMetadata(null));

        public IPerformsValidation Source
        {
            get { return (IPerformsValidation)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(IPerformsValidation), typeof(ErrorsSetter), new PropertyMetadata(null));

        public string SourcePropertyName
        {
            get { return (string)GetValue(SourcePropertyNameProperty); }
            set { SetValue(SourcePropertyNameProperty, value); }
        }

        public static readonly DependencyProperty SourcePropertyNameProperty =
            DependencyProperty.Register("SourcePropertyName", typeof(string), typeof(ErrorsSetter), new PropertyMetadata(null));

        public string SourcePropertyValue
        {
            get { return (string)GetValue(SourcePropertyValueProperty); }
            set { SetValue(SourcePropertyValueProperty, value); }
        }

        public static readonly DependencyProperty SourcePropertyValueProperty =
            DependencyProperty.Register("SourcePropertyValue", typeof(string), typeof(ErrorsSetter), new PropertyMetadata(null, OnSourcePropertyValueChanged));

        static void OnSourcePropertyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var setter = (ErrorsSetter)d;
            setter.UpdateErrors();
        }

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
            UpdateErrors();
        }

        void UpdateErrors()
        {
            if(Target != null && Source != null && Source.Errors != null && !string.IsNullOrEmpty(SourcePropertyName))
            {
                Source.Validate(SourcePropertyName, "");
                List<IActionableErrorInfo> errors;
                Target.Errors = Source.Errors.TryGetValue(SourcePropertyName, out errors) ? errors : null;
            }
        }
    }
}

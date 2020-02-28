using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Warewolf.UI
{
    public class ErrorsSetter : Behavior<FrameworkElement>
    {
        public IErrorsSource Target
        {
            get => (IErrorsSource)GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(IErrorsSource), typeof(ErrorsSetter), new PropertyMetadata(null));

        public IPerformsValidation Source
        {
            get => (IPerformsValidation)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(IPerformsValidation), typeof(ErrorsSetter), new PropertyMetadata(null));

        public string SourcePropertyName
        {
            get => (string)GetValue(SourcePropertyNameProperty);
            set => SetValue(SourcePropertyNameProperty, value);
        }

        public static readonly DependencyProperty SourcePropertyNameProperty =
            DependencyProperty.Register("SourcePropertyName", typeof(string), typeof(ErrorsSetter), new PropertyMetadata(null));

        public string SourcePropertyValue
        {
            get => (string)GetValue(SourcePropertyValueProperty);
            set => SetValue(SourcePropertyValueProperty, value);
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
            if (Target != null && Source?.Errors != null && !string.IsNullOrEmpty(SourcePropertyName))
            {
                Source.Validate(SourcePropertyName, "");
                Target.Errors = Source.Errors.TryGetValue(SourcePropertyName, out List<IActionableErrorInfo> errors) ? errors : null;
            }
        }
    }
}

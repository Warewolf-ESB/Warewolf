using System.Collections.Generic;
using System.Windows;
using Dev2.Activities.Adorners;
using Dev2.Activities.Designers;
using Dev2.Providers.Errors;
using Dev2.Studio.AppResources.ExtensionMethods;

namespace Dev2.Activities.Help
{
    public static class HelpText
    {
        public static string GetText(DependencyObject obj)
        {
            return (string)obj.GetValue(TextProperty);
        }

        public static void SetText(DependencyObject obj, string value)
        {
            obj.SetValue(TextProperty, value);
        }

        // Using a DependencyProperty as the backing store for HelptText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached("Text", typeof(string), typeof(HelpText), 
                new PropertyMetadata(string.Empty, HelpTextPropertyChangedCallback));

        static void HelpTextPropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var uiElement = (UIElement)o;
            DependencyPropertyChangedEventHandler handler =
                delegate(object sender, DependencyPropertyChangedEventArgs args)
                {
                    var helpText = e.NewValue as string;
                    if(helpText != null)
                    {
                        SetHelpText(args, uiElement, helpText);
                    }
                };
            uiElement.IsKeyboardFocusedChanged -= handler;
            uiElement.IsKeyboardFocusedChanged += handler;
        }

        static void SetHelpText(DependencyPropertyChangedEventArgs args, UIElement uiElement, string helpText)
        {
            var isFocused = (bool)args.NewValue;
            if(isFocused)
            {
                var overlay = uiElement.FindVisualParent<ActivityTemplate>();
                var parentAsAdorner = overlay.Parent as AdornerPresenterBase;
                if(parentAsAdorner != null)
                {
                    var presenter = parentAsAdorner;
                    var activityDesigner = presenter.AssociatedActivityDesigner;
                    activityDesigner.HelpText = helpText;
                }
            }
        }

        static void SetErrorText(UIElement args, UIElement uiElement,
            List<IActionableErrorInfo> errors)
        {
            var isFocused = args.IsFocused;
            if(isFocused)
            {
                var overlayAdorner = uiElement.FindVisualParent<OverlayAdorner>();
                if(overlayAdorner != null)
                {
                    overlayAdorner.Errors = errors;
                }
            }
        }

        public static List<IActionableErrorInfo> GetErrors(DependencyObject obj)
        {
            return (List<IActionableErrorInfo>)obj.GetValue(ErrorsProperty);
        }

        public static void SetErrors(DependencyObject obj, List<IActionableErrorInfo> value)
        {
            obj.SetValue(ErrorsProperty, value);
        }

        public static readonly DependencyProperty ErrorsProperty =
            DependencyProperty.RegisterAttached("Errors", typeof(List<IActionableErrorInfo>), typeof(HelpText),
                new PropertyMetadata(null, ErrorsPropertyChangedCallback));

        static UIElement UiElement;

        static void ErrorsPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var errors = e.NewValue as List<IActionableErrorInfo>;
            UiElement = (UIElement)d;
            SetErrorText(UiElement, UiElement, errors);
        }
    }
}


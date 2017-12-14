using System;
using System.Windows;
using System.Windows.Controls;

namespace Dev2.CustomControls
{
    public static class TemplateGenerator
    {
        private sealed class _TemplateGeneratorControl : ContentControl
        {
            internal static readonly DependencyProperty FactoryProperty = DependencyProperty.Register("Factory", typeof(Func<object>), typeof(_TemplateGeneratorControl), new PropertyMetadata(null, _FactoryChanged));

            private static void _FactoryChanged(DependencyObject instance, DependencyPropertyChangedEventArgs args)
            {
                var control = (_TemplateGeneratorControl)instance;
                var factory = (Func<object>)args.NewValue;
                control.Content = factory();
            }
        }

        /// <summary>
        /// Creates a data-template that uses the given delegate to create new instances.
        /// </summary>
        public static DataTemplate CreateDataTemplate(Func<object> factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            var frameworkElementFactory = new FrameworkElementFactory(typeof(_TemplateGeneratorControl));
            frameworkElementFactory.SetValue(_TemplateGeneratorControl.FactoryProperty, factory);

            var dataTemplate = new DataTemplate(typeof(DependencyObject));
            dataTemplate.VisualTree = frameworkElementFactory;
            return dataTemplate;
        }

        /// <summary>
        /// Creates a control-template that uses the given delegate to create new instances.
        /// </summary>
        public static ControlTemplate CreateControlTemplate(Type controlType, Func<object> factory)
        {
            if (controlType == null)
                throw new ArgumentNullException("controlType");

            if (factory == null)
                throw new ArgumentNullException("factory");

            var frameworkElementFactory = new FrameworkElementFactory(typeof(_TemplateGeneratorControl));
            frameworkElementFactory.SetValue(_TemplateGeneratorControl.FactoryProperty, factory);

            var controlTemplate = new ControlTemplate(controlType);
            controlTemplate.VisualTree = frameworkElementFactory;
            return controlTemplate;
        }
    }
}

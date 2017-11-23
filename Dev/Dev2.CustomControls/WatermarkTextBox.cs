using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace Dev2.CustomControls
{
    public class WatermarkTextBox : TextBox
    {
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached(
           "Watermark",
           typeof(object),
           typeof(WatermarkTextBox),
           new FrameworkPropertyMetadata(null, OnWatermarkChanged));

        public static object GetWatermark(DependencyObject d) => d.GetValue(WatermarkProperty);

        public static void SetWatermark(DependencyObject d, object value) => d.SetValue(WatermarkProperty, value);

        static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Control)d;
            control.Loaded += Control_Loaded;

            if (d is ComboBox || d is TextBox)
            {
                control.GotKeyboardFocus += Control_GotKeyboardFocus;
                control.LostKeyboardFocus += Control_Loaded;

            }

            if (d is TextBox)
            {
                var textBox = d as TextBox;
                textBox.TextChanged += (sender, args) =>
                {
                    if (!ShouldShowWatermark(control))
                    {
                        RemoveWatermark(control);
                    }
                };
            }
        }

        #region Event Handlers

        static void Control_GotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            var c = (Control)sender;
            if (ShouldShowWatermark(c))
            {
                RemoveWatermark(c);
            }
        }

        static void Control_Loaded(object sender, RoutedEventArgs e)
        {
            var control = (Control)sender;
            if (ShouldShowWatermark(control))
            {
                ShowWatermark(control);
            }
        }

        #endregion

        #region Helper Methods

        static void RemoveWatermark(UIElement control)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);

            // layer could be null if control is no longer in the visual tree
            if (layer != null)
            {
                var adorners = layer.GetAdorners(control);
                if (adorners == null)
                {
                    return;
                }

                foreach (Adorner adorner in adorners)
                {
                    if (adorner is WatermarkAdorner)
                    {
                        adorner.Visibility = Visibility.Hidden;
                        layer.Remove(adorner);
                    }
                }
            }
        }

        static void ShowWatermark(Control control)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);

            // layer could be null if control is no longer in the visual tree
            layer?.Add(new WatermarkAdorner(control, GetWatermark(control)));
        }

        static bool ShouldShowWatermark(Control c)
        {
            if (c is TextBoxBase)
            {
                var textBox = c as TextBox;
                return textBox != null && textBox.Text == string.Empty;
            }
            return (c as ItemsControl)?.Items.Count == 0;
        }

        #endregion
    }
}

using System.Windows;
using System.Windows.Controls;

namespace Warewolf.Studio.CustomControls
{
    public class TextBoxHelper
    {
        static bool _isInistialised = false;

        public static string GetWatermark(DependencyObject obj)
        {
            return (string)obj.GetValue(WatermarkProperty);
        }

        public static void SetWatermark(DependencyObject obj, string value)
        {
            obj.SetValue(WatermarkProperty, value);
        }

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.RegisterAttached("Watermark", typeof(string), typeof(TextBoxHelper), new UIPropertyMetadata(null, WatermarkChanged));



        public static bool GetShowWatermark(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowWatermarkProperty);
        }

        public static void SetShowWatermark(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowWatermarkProperty, value);
        }

        public static readonly DependencyProperty ShowWatermarkProperty =
            DependencyProperty.RegisterAttached("ShowWatermark", typeof(bool), typeof(TextBoxHelper), new UIPropertyMetadata(false));



        static void WatermarkChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var box = obj as TextBox;

            CheckShowWatermark(box);

            if (!_isInistialised)
            {
                if(box != null)
                {
                    box.TextChanged += BoxTextChanged;
                    box.Unloaded += BoxUnloaded;
                }
                _isInistialised = true;
            }
        }

        private static void CheckShowWatermark(TextBox box)
        {
            box.SetValue(ShowWatermarkProperty, box.Text == string.Empty);
        }

        static void BoxTextChanged(object sender, TextChangedEventArgs e)
        {
            var box = sender as TextBox;
            CheckShowWatermark(box);
        }

        static void BoxUnloaded(object sender, RoutedEventArgs e)
        {
            var box = sender as TextBox;
            if(box != null)
            {
                box.TextChanged -= BoxTextChanged;
            }
        }
    }
}

using System.Windows;
using System.Windows.Controls;

namespace Warewolf.Studio.CustomControls
{
    public class PasswordBoxHelper
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
            DependencyProperty.RegisterAttached("Watermark", typeof(string), typeof(PasswordBoxHelper), new UIPropertyMetadata(null, WatermarkChanged));



        public static bool GetShowWatermark(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowWatermarkProperty);
        }

        public static void SetShowWatermark(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowWatermarkProperty, value);
        }

        public static readonly DependencyProperty ShowWatermarkProperty =
            DependencyProperty.RegisterAttached("ShowWatermark", typeof(bool), typeof(PasswordBoxHelper), new UIPropertyMetadata(false));



        static void WatermarkChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var pwd = obj as PasswordBox;

            CheckShowWatermark(pwd);

            if (!_isInistialised)
            {
                if(pwd != null)
                {
                    pwd.PasswordChanged += PwdPasswordChanged;
                    pwd.Unloaded += PwdUnloaded;
                }
                _isInistialised = true;
            }
        }

        private static void CheckShowWatermark(PasswordBox pwd)
        {
            pwd.SetValue(ShowWatermarkProperty, pwd.Password == string.Empty);
        }

        static void PwdPasswordChanged(object sender, RoutedEventArgs e)
        {
            var pwd = sender as PasswordBox;
            CheckShowWatermark(pwd);
        }

        static void PwdUnloaded(object sender, RoutedEventArgs e)
        {
            var pwd = sender as PasswordBox;
            if(pwd != null)
            {
                pwd.PasswordChanged -= PwdPasswordChanged;
            }
        }
    }
}

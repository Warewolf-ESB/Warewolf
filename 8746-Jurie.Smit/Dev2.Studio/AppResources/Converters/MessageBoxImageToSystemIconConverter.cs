using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Dev2.Studio.AppResources.Converters
{
    public class MessageBoxImageToSystemIconConverter : IValueConverter
    {
        private static Bitmap emptyBitmap;
        private static IntPtr hicon;

        static MessageBoxImageToSystemIconConverter()
        {
            emptyBitmap = new Bitmap(1,1);
            hicon = emptyBitmap.GetHicon();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Icon icon = Icon.FromHandle(hicon);

            if (value == null)
            {
                return BitmapSourceFromIcon(icon);
            }

            MessageBoxImage messageBoxImage;
            if (Enum.TryParse(value.ToString(), true, out messageBoxImage))
            {
                if (messageBoxImage == MessageBoxImage.Asterisk)
                {
                    icon = SystemIcons.Asterisk;
                }
                else if (messageBoxImage == MessageBoxImage.Error)
                {
                    icon = SystemIcons.Error;
                }
                else if (messageBoxImage == MessageBoxImage.Exclamation)
                {
                    icon = SystemIcons.Exclamation;
                }
                else if (messageBoxImage == MessageBoxImage.Hand)
                {
                    icon = SystemIcons.Hand;
                }
                else if (messageBoxImage == MessageBoxImage.Information)
                {
                    icon = SystemIcons.Information;
                }
                else if (messageBoxImage == MessageBoxImage.None)
                {
                    //bitmap = new Bitmap(1, 1);
                }
                else if (messageBoxImage == MessageBoxImage.Question)
                {
                    icon = SystemIcons.Question;
                }
                else if (messageBoxImage == MessageBoxImage.Stop)
                {
                    icon = SystemIcons.Error;
                }
                else if (messageBoxImage == MessageBoxImage.Warning)
                {
                    icon = SystemIcons.Warning;
                }
            }

            return BitmapSourceFromIcon(icon);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private BitmapSource BitmapSourceFromIcon(Icon icon)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
    }
}

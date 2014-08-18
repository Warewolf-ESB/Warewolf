using Dev2.Common;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Dev2.Studio.AppResources.Converters
{

    public class MessageBoxImageToSystemIconConverter : IValueConverter
    {


        private static Bitmap _emptyBitmap;
        private static IntPtr _hicon;

        static MessageBoxImageToSystemIconConverter()
        {
            _emptyBitmap = new Bitmap(1, 1);
            _hicon = _emptyBitmap.GetHicon();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Icon icon = Icon.FromHandle(_hicon);

            if(value == null)
            {
                return BitmapSourceFromIcon(icon);
            }

            MessageBoxImage messageBoxImage;
            if(Enum.TryParse(value.ToString(), true, out messageBoxImage))
            {
                switch(messageBoxImage)
                {
                    case MessageBoxImage.Error:
                        return CustomIcons.Error;
                    case MessageBoxImage.Information:
                        return CustomIcons.Information;
                    case MessageBoxImage.None:
                        break;
                    case MessageBoxImage.Question:
                        return CustomIcons.Question;
                    case MessageBoxImage.Warning:
                        return CustomIcons.Warning;
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

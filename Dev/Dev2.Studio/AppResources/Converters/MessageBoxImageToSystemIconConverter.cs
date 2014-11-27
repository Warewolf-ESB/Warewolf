
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Dev2.Common;

namespace Dev2.Studio.AppResources.Converters
{

    public class MessageBoxImageToSystemIconConverter : IValueConverter
    {
        private static readonly IntPtr Hicon;

        static MessageBoxImageToSystemIconConverter()
        {
            var emptyBitmap = new Bitmap(1, 1);
            Hicon = emptyBitmap.GetHicon();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var icon = Icon.FromHandle(Hicon);

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

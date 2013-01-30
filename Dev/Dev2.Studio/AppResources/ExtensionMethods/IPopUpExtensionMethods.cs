using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Dev2.Studio.Core.ViewModels;

namespace Dev2.Studio.AppResources.ExtensionMethods
{
    public static class IPopUpExtensionMethods
    {
        public static MessageBoxResult Show(this IPopUp popup, string description, string header = "", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Information)
        {
            popup.Buttons = buttons;
            popup.Description = description;
            popup.Header = header;
            popup.ImageType = image;
            return popup.Show();
        }
    }
}

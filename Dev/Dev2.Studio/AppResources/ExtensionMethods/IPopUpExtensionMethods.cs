using Dev2.Studio.Core.Controller;
using System.Windows;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.AppResources.ExtensionMethods
{
    public static class IPopUpExtensionMethods
    {
        public static MessageBoxResult Show(this IPopupController popup, string description, string header = "", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Information, string dontShowAgainKey = null)
        {
            popup.Buttons = buttons;
            popup.Description = description;
            popup.Header = header;
            popup.ImageType = image;
            popup.DontShowAgainKey = dontShowAgainKey;
            return popup.Show();
        }
    }
}

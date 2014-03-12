using System;
using System.Windows;
using Dev2.Composition;
using Dev2.Studio.Core.Controller;

namespace Dev2.Utils
{
    public static class HelperUtils
    {
        public static void ShowTrustRelationshipError(SystemException exception)
        {
            if(exception.Message.Contains("The trust relationship between this workstation and the primary domain failed"))
            {
                var popup = ImportService.GetExportValue<IPopupController>();
                popup.Header = "Error connecting to server";
                popup.Description = "This computer cannot contact the Domain Controller."
                                    + Environment.NewLine + "If it does not belong to a domain, please ensure it is removed from the domain in computer management.";
                popup.Buttons = MessageBoxButton.OK;
                popup.ImageType = MessageBoxImage.Error;
                popup.Show();
            }
        }
    }
}

using Dev2.Common;
using Dev2.Composition;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core.ViewModels;
using Dev2.UI;
using System;
using System.Windows;

namespace Dev2.Studio
{
    /// <summary>
    /// Holds all static routed event handlers
    /// </summary>
    public static class ClassRoutedEventHandlers
    {
        #region Fields

        private static bool Registered;

        #endregion Fields

        #region Methods

        public static void RegisterEvents()
        {
            if (Registered)
            {
                return;
            }

            Registered = true;
            EventManager.RegisterClassHandler(typeof(IntellisenseTextBox), IntellisenseTextBox.TabInsertedEvent, new RoutedEventHandler(IntellisenseTextBoxTabInsertedEvent));
        }

        #endregion

        #region IntellisenseTextBox

        internal static void IntellisenseTextBoxTabInsertedEvent(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                IPopUp popup = ImportService.GetExportValue<IPopUp>();
                popup.Show("You have pasted text which contins tabs into a textbox on the design surface. Tabs are not allowed in textboxes on the design surface and will be replaced with spaces. Please note that tabs are fully supported but the runtime, in variables and when reading from files.", "Paste", MessageBoxButton.OK, MessageBoxImage.Information, GlobalConstants.Dev2MessageBoxDesignSurfaceTabPasteDialog);
            }), null);
        }

        #endregion

    }
}


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
using System.Windows;
using Dev2.Common;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.UI;

// ReSharper disable CheckNamespace
namespace Dev2.Studio
{
    /// <summary>
    /// Holds all static routed event handlers
    /// </summary>
    public static class ClassRoutedEventHandlers
    {
        #region Fields

        private static bool _registered;

        #endregion Fields

        #region Methods

        public static void RegisterEvents()
        {
            if(_registered)
            {
                return;
            }

            _registered = true;
            EventManager.RegisterClassHandler(typeof(IntellisenseTextBox), IntellisenseTextBox.TabInsertedEvent, new RoutedEventHandler(IntellisenseTextBoxTabInsertedEvent));
        }

        #endregion

        #region IntellisenseTextBox

        internal static void IntellisenseTextBoxTabInsertedEvent(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                IPopupController popup = CustomContainer.Get<IPopupController>();
                popup.Show("You have pasted text which contins tabs into a textbox on the design surface. Tabs are not allowed in textboxes on the design surface and will be replaced with spaces. "
                    + Environment.NewLine + Environment.NewLine +
                    "Please note that tabs are fully supported but the runtime, in variables and when reading from files.",
                    "Tabs Pasted", MessageBoxButton.OK, MessageBoxImage.Information, GlobalConstants.Dev2MessageBoxDesignSurfaceTabPasteDialog);
            }), null);
        }

        #endregion
    }
}

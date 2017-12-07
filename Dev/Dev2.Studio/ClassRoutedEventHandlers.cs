/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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


namespace Dev2.Studio
{
    public static class ClassRoutedEventHandlers
    {
        #region Fields

        static bool _registered;

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
                var popup = CustomContainer.Get<IPopupController>();
                popup.Show(Warewolf.Studio.Resources.Languages.Core.IntellisenseTabInserted,
                    Warewolf.Studio.Resources.Languages.Core.IntellisenseTabInsertedHeader, MessageBoxButton.OK, MessageBoxImage.Information, GlobalConstants.Dev2MessageBoxDesignSurfaceTabPasteDialog, false, false, true, false, false, false);
            }), null);
        }

        #endregion
    }
}

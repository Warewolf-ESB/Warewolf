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

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Dock
{
    /// <summary>
    /// Static class for dealing with UIElement related members.
    /// </summary>
    public static class UiElementHelper
    {
        #region RaiseEvent
        /// <summary>
        /// Raises an event for the specified event arguments on the specified element
        /// </summary>
        /// <param name="element">The element that listens to the event</param>
        /// <param name="e">The arguments for the routed event to be raised</param>
        public static void RaiseEvent(DependencyObject element, RoutedEventArgs e)
        {
            if(element == null)
                throw new ArgumentNullException("element");

            IInputElement inputElement = element as IInputElement;

            inputElement?.RaiseEvent(e);
        }
        #endregion //RaiseEvent

    }
}

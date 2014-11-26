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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Dev2.Util.ExtensionMethods
{
    public static class FrameWorkElementExtensions
    {
        public static void BringToFront(this FrameworkElement element)
        {
            if (element == null) return;

            var parent = element.Parent as Panel;
            if (parent == null) return;

            List<int> maxZ = parent.Children.OfType<UIElement>()
                .Where(x => !Equals(x, element))
                .Select(Panel.GetZIndex)
                .ToList();

            if (!maxZ.Any())
            {
                return;
            }
            int max = maxZ.Max();
            Panel.SetZIndex(element, max + 1);
        }

        public static void BringToMaxFront(this FrameworkElement element)
        {
            if (element == null)
            {
                return;
            }

            Panel.SetZIndex(element, Int32.MaxValue);
        }

        public static void SendToBack(this FrameworkElement element)
        {
            if (element == null)
            {
                return;
            }

            Panel.SetZIndex(element, Int32.MinValue);
        }
    }
}
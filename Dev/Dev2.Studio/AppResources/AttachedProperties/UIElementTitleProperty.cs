
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
using System.Text;
using System.Windows;

namespace Dev2.Studio.AppResources.AttachedProperties
{
    public static class UIElementTitleProperty
    {
        public static void SetTitle(UIElement element, string value)
        {
            element.SetValue(titleProperty, value);
        }

        public static string GetTitle(UIElement element)
        {
            return (string)element.GetValue(titleProperty);
        }

        public static readonly DependencyProperty titleProperty =
            DependencyProperty.RegisterAttached("Title", typeof(string), typeof(UIElementTitleProperty), new PropertyMetadata(default(string)));
    }
}

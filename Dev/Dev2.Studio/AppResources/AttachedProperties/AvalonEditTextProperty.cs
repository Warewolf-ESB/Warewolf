
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows;

namespace Dev2.Studio.AttachedProperties
{
    public static class AvalonEditTextProperty
    {
        public static readonly DependencyProperty propertyNameProperty =
            DependencyProperty.RegisterAttached("propertyName", typeof(string), typeof(AvalonEditTextProperty), new PropertyMetadata(default(string)));

        public static void SetpropertyName(UIElement element, string value)
        {
            element.SetValue(propertyNameProperty, value);
        }

        public static string GetpropertyName(UIElement element)
        {
            return (string)element.GetValue(propertyNameProperty);
        }
    }
}

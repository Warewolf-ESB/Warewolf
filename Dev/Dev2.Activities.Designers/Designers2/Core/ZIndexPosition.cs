
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using System.Windows.Controls;

namespace Dev2.Activities.Designers2.Core
{
    public enum ZIndexPosition
    {
        Back,
        Front
    }

    public static class ZIndexUtils
    {
        public static void SetZIndex(this FrameworkElement element, ZIndexPosition position)
        {
            switch(position)
            {
                case ZIndexPosition.Front:
                    Panel.SetZIndex(element, Int32.MaxValue);
                    break;

                case ZIndexPosition.Back:
                    Panel.SetZIndex(element, Int32.MinValue);
                    break;
            }
        }
    }
}

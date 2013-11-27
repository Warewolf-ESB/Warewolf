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

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
using System.Windows.Interactivity;

namespace Dev2.CustomControls.Behavior
{
    public static class AttachBehavior
    {
        public static Type GetBehaviorType(DependencyObject obj)
        {
            return (Type)obj.GetValue(BehaviorTypeProperty);
        }

        public static void SetBehaviorType(DependencyObject obj, Type value)
        {
            obj.SetValue(BehaviorTypeProperty, value);
        }

        public static readonly DependencyProperty BehaviorTypeProperty =
            DependencyProperty.RegisterAttached("BehaviorType", typeof(Type),
            typeof(object), new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            if(args.NewValue == null)
            {
                return;
            }

            if(!(args.NewValue is Type))
            {
                return;
            }

            var behavior = Activator.CreateInstance((Type)args.NewValue)
                as System.Windows.Interactivity.Behavior;
            if(behavior != null)
            {
                Interaction.GetBehaviors(o).Add(behavior);
            }
        }
    }
}

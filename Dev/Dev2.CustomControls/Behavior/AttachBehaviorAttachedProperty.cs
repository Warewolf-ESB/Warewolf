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

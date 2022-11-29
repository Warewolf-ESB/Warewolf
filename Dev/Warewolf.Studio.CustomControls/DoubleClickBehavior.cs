﻿using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Warewolf.Studio.CustomControls
{
    public class DoubleClickBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            AssociatedObject.MouseDoubleClick += AssociatedObjectOnMouseDoubleClick;
            base.OnAttached();
        }

        void AssociatedObjectOnMouseDoubleClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            AssociatedObject.SelectAll();
        }
    }
}

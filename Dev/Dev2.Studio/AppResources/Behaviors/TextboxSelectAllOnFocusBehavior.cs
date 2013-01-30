using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class TextboxSelectAllOnFocusBehavior : Behavior<TextBox>, IDisposable
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotFocus += AssociatedObject_GotFocus;           
        }

        void AssociatedObject_GotFocus(object sender, EventArgs e)
        {
            AssociatedObject.SelectAll();
        }       

        protected override void OnDetaching()
        {         
            AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
            base.OnDetaching();
        }

        public void Dispose()
        {
            AssociatedObject.GotFocus -= AssociatedObject_GotFocus;         
        }
    }
}

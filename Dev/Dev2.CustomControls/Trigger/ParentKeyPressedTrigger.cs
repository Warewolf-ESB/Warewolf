using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dev2.Studio.AppResources.ExtensionMethods;

namespace Dev2.CustomControls.Trigger
{
    public class ParentKeyPressedTrigger<TParent> : KeyPressedTrigger
        where TParent : FrameworkElement
    {
        protected TParent Parent;

        protected override void OnAttached()
        {
            base.OnAttached();
            Parent = AssociatedObject.FindVisualParent<TParent>();
            Parent.PreviewKeyDown += ProcessKeyPress;
        }

        protected override void OnDetaching()
        {
            Parent.PreviewKeyDown -= ProcessKeyPress;
            base.OnDetaching();
        }

    }
}

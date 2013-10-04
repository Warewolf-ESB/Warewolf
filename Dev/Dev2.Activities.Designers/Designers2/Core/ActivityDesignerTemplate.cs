using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2.Activities.Designers2.Core.Controls;

namespace Dev2.Activities.Designers2.Core
{
    public abstract class ActivityDesignerTemplate : UserControl
    {
        protected ActivityDesignerTemplate()
        {
            LeftButtons = new ObservableCollection<Button>();
            RightButtons = new ObservableCollection<Button>();
        }

        public ObservableCollection<Button> LeftButtons { get; set; }
        public ObservableCollection<Button> RightButtons { get; set; }
        public Dev2DataGrid DataGrid { get; set; }

        public void SetInitialFocus()
        {
            // Wait for the UI to be fully rendered BEFORE trying to set the focus
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
            {
                var focusElement = GetInitialFocusElement();
                if(focusElement != null)
                {
                    Keyboard.Focus(focusElement);
                }
            }));
        }

        protected abstract IInputElement GetInitialFocusElement();
    }
}

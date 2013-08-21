using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace Dev2.CustomControls.Progress
{
    public interface IProgressDialog
    {
        event EventHandler CancelClick;
        event CancelEventHandler Closing;

        string Label { get; set; }
        string SubLabel { get; set; }
        double ProgressValue { get; set; }
        bool IsCancelButtonEnabled { get; set; }

        void InvokeOnDispatcher(DispatcherPriority priority, Delegate method);

        void Close();

        bool? ShowDialog();

    }
}
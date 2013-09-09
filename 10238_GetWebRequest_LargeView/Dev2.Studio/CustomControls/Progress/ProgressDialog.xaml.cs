using System;
using System.Windows;
using System.Windows.Threading;

namespace Dev2.CustomControls.Progress
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : IProgressDialog
    {
        public ProgressDialog(Window owner)
        {
            Owner = owner;
            InitializeComponent();
        }

        #region Properties / Events

        public event EventHandler CancelClick;

        public string Label { get { return TextLabel.Text; } set { TextLabel.Text = value; } }
        public string SubLabel { get { return SubTextLabel.Text; } set { SubTextLabel.Text = value; } }
        public double ProgressValue { get { return ProgressBar.Value; } set { ProgressBar.Value = value; } }
        public bool IsCancelButtonEnabled { get { return CancelButton.IsEnabled; } set { CancelButton.IsEnabled = value; } }

        #endregion

        #region InvokeOnDispatcher

        public void InvokeOnDispatcher(DispatcherPriority priority, Delegate method)
        {
            Dispatcher.BeginInvoke(priority, method);
        }

        #endregion

        #region OnCancelButtonClick

        void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            if(CancelClick != null)
            {
                CancelClick(this, e);
            }
        }

        #endregion

    }
}

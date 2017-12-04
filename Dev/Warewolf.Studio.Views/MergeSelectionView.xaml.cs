using System.Windows.Controls;
using Dev2.Common.Interfaces;
using Warewolf.Studio.Core;
using System.Windows.Input;
using Dev2;
using Dev2.Studio.Interfaces;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for MergeSelectionView.xaml
    /// </summary>
    public partial class MergeSelectionView : IMergeView
    {
        readonly Grid _blackoutGrid = new Grid();

        public MergeSelectionView()
        {
            InitializeComponent();
        }

        public void ShowView()
        {
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
            Closing += WindowClosing;
            ShowDialog();
        }

        void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
        }

        public void RequestClose()
        {
            Close();
        }

        public bool IsMergeButtonEnabled()
        {
            return MergeButton.Command.CanExecute(null);
        }

        public void Cancel()
        {
            CancelButton.Command.Execute(null);
        }

        public void Merge()
        {
            if (MergeButton.IsEnabled)
            {
                MergeButton.Command.Execute(null);
            }
        }

        private void MergeDialogView_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            if ((Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Control)) && (e.Key == Key.F4))
            {
                var mainViewModel = CustomContainer.Get<IShellViewModel>();
                mainViewModel?.ResetMainView();
            }
        }
    }
}

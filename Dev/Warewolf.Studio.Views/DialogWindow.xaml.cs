using Dev2.Common.Interfaces.PopupController;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : IActionDialogueWindow
    {
        public DialogWindow()
        {
            InitializeComponent();
        }

        #region Implementation of IDialogueWindow

        public void ShowThis()
        {

        }

        #endregion
    }
}

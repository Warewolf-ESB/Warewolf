using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : IActionDialogueWindow
    {
        Window _window;

        public DialogWindow()
        {
            InitializeComponent();
            IsModal = true;
            
        }

        #region Implementation of IDialogueWindow

        public MessageBoxResult ShowThis(IDialogueTemplate serverPopup)
        {
            DataContext = serverPopup;



            var blurEffect = new BlurEffect { Radius = 10 };
            Application.Current.MainWindow.Effect = blurEffect;
            Window = new Window { WindowStyle = WindowStyle.None, AllowsTransparency = true, Background = Brushes.Transparent, SizeToContent = SizeToContent.WidthAndHeight, ResizeMode = ResizeMode.NoResize, WindowStartupLocation = WindowStartupLocation.CenterScreen, Content = this };
            var res = Window.ShowDialog();
            Window.Close();
            return MessageBoxResult.OK;
        }

        public Window Window
        {
            get
            {
                return _window;
            }
            set
            {
                _window = value;
            }
        }

        public new void Close()
        {
            if (Window != null)
            {
                Application.Current.MainWindow.Effect = null;
                Window.Close();
            }
        }

        #endregion


    }
}

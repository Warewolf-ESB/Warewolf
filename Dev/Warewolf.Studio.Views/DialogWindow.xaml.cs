using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Infragistics.Controls.Interactions.Primitives;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : IActionDialogueWindow
    {
        Window _window;
        Grid _blackoutGrid;

        public DialogWindow()
        {
            InitializeComponent();
            IsModal = true;
            IsVisibleChanged +=DialogWindow_IsVisibleChanged;

        }


        void DialogWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false && (bool)e.OldValue)
                Close();
        }

        #region Implementation of IDialogueWindow

        public MessageBoxResult ShowThis(IDialogueTemplate serverPopup)
        {
            DataContext = serverPopup;
            //var effect = BlurEffect();
           // Application.Current.MainWindow.Effect = effect;
            Window = new Window { WindowStyle = WindowStyle.None, AllowsTransparency = true, Background = Brushes.Transparent, SizeToContent = SizeToContent.WidthAndHeight, ResizeMode = ResizeMode.NoResize, WindowStartupLocation = WindowStartupLocation.CenterScreen, Content = this };         
            Window.ShowDialog();
            return MessageBoxResult.OK;
        }

        BlurEffect BlurEffect()
        {
            var effect = new BlurEffect { Radius = 10, KernelType = KernelType.Gaussian, RenderingBias = RenderingBias.Quality };
            var content = Application.Current.MainWindow.Content as Grid;
            _blackoutGrid = new Grid();
            _blackoutGrid.Background = new SolidColorBrush(Colors.Black);
            _blackoutGrid.Opacity = 0.75;
            if (content != null)
            {
                content.Children.Add(_blackoutGrid);
            }
            return effect;
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
                try
                {
                    Window.Close();
                }
                catch { }
               
                RemoveBlur();
            }
        }

        void RemoveBlur()
        {
            var content = Application.Current.MainWindow.Content as Grid;

            if(content != null)
            {
                content.Children.Remove(_blackoutGrid);
            }
        }

        #endregion


    }
}

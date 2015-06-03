using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageWebservicePasteView.xaml
    /// </summary>
    public partial class ManageWebservicePasteView:IPasteView
    {
        Grid _blackoutGrid;
        Window _window;

        public ManageWebservicePasteView()
        {
            InitializeComponent();
        }



        public string ShowView(string text)
        {
            IsModal = true;
            var effect = new BlurEffect { Radius = 10, KernelType = KernelType.Gaussian, RenderingBias = RenderingBias.Quality };
            var content = Application.Current.MainWindow.Content as Grid;
            _blackoutGrid = new Grid { Background = new SolidColorBrush(Colors.Black), Opacity = 0.75 };
            if (content != null)
            {
                content.Children.Add(_blackoutGrid);
            }
            Application.Current.MainWindow.Effect = effect;

            _window = new Window { WindowStyle = WindowStyle.None, AllowsTransparency = true, Background = Brushes.Transparent, SizeToContent = SizeToContent.Manual, MinWidth = 640, MinHeight = 480, ResizeMode = ResizeMode.CanResize, WindowStartupLocation = WindowStartupLocation.CenterScreen, Content = this };
            var vm = new PasteVM(text);
            _window.DataContext = vm;
            _window.ShowDialog();
            return 
        }

        void RemoveBlackOutEffect()
        {
            Application.Current.MainWindow.Effect = null;
            var content = Application.Current.MainWindow.Content as Grid;
            if (content != null)
            {
                content.Children.Remove(_blackoutGrid);
            }
        }

        public void RequestClose()
        {
            RemoveBlackOutEffect();
            _window.Close();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            RequestClose();
        }


    }

    public class PasteVM
    {
         string _text;

        public PasteVM(string text)
        {
            _text = text;
        }

        public string Text
        {
            get
            {
                return _text;
            }
            set { _text = value; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageWebservicePasteView.xaml
    /// </summary>
    public partial class ManageWebservicePasteView
    {
        Grid _blackoutGrid;
        Window _window;

        public ManageWebservicePasteView()
        {
            InitializeComponent();
        }

        public void ShowView()
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
            _window.ShowDialog();
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
}

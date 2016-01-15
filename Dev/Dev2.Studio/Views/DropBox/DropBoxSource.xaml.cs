using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Dev2.Views.DropBox
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class DropBoxViewWindow
    {
        Grid _blackoutGrid;

        public DropBoxViewWindow()
        {
            InitializeComponent();

            ShowView();
        }

        public void ShowView()
        {
            var effect = new BlurEffect { Radius = 10, KernelType = KernelType.Gaussian, RenderingBias = RenderingBias.Quality };
            var content = Application.Current.MainWindow.Content as Grid;
            _blackoutGrid = new Grid
            {
                Background = new SolidColorBrush(Colors.DarkGray),
                Opacity = 0.5
            };
            if (content != null)
            {
                content.Children.Add(_blackoutGrid);
            }
            Application.Current.MainWindow.Effect = effect;
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

        void RequestClose()
        {
            RemoveBlackOutEffect();
            Close();
        }

        void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            RequestClose();
        }

        void DropBoxViewWindow_OnClosing(object sender, CancelEventArgs e)
        {
            RemoveBlackOutEffect();
        }
    }
}

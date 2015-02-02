using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Studio.AppResources.Converters;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for PopupView.xaml
    /// </summary>
    public partial class PopupView : IPopupWindow
    {
        MessageBoxResult _dialogResult;
        Window _window;
        Grid _blackoutGrid;

        public PopupView()
        {
            InitializeComponent();
            IsModal = true;
            _dialogResult = MessageBoxResult.None;
        }

        #region Implementation of IPopupWindow

        public MessageBoxResult Show(IPopupMessage message)
        {
            MessageText.Text = message.Description;
            Header = message.Header;
            var imageSource = new MessageBoxImageToSystemIconConverter().Convert(message.Image, null, null, null) as string;
            if(imageSource != null)
            {
                MessageImage.Source = new BitmapImage(new Uri(imageSource));
            }

            var effect = new BlurEffect { Radius = 10, KernelType = KernelType.Gaussian, RenderingBias = RenderingBias.Quality };
            var content = Application.Current.MainWindow.Content as Grid;
            _blackoutGrid = new Grid();
            _blackoutGrid.Background = new SolidColorBrush(Colors.Black);
            _blackoutGrid.Opacity = 0.75;
            if (content != null)
            {
                content.Children.Add(_blackoutGrid);
            }
            Application.Current.MainWindow.Effect = effect;

            _window = new Window { WindowStyle = WindowStyle.None, AllowsTransparency = true, Background = Brushes.Transparent, SizeToContent = SizeToContent.WidthAndHeight, ResizeMode = ResizeMode.NoResize, WindowStartupLocation = WindowStartupLocation.CenterScreen, Content = this };
            _window.ShowDialog();
            return _dialogResult;
        }

        #endregion

        void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            _dialogResult = MessageBoxResult.OK;
            RemoveBlackOutEffect();
            _window.Close();
        }

        void RemoveBlackOutEffect()
        {
            Application.Current.MainWindow.Effect = null;
            var content = Application.Current.MainWindow.Content as Grid;
            if(content != null)
            {
                content.Children.Remove(_blackoutGrid);
            }
        }

        void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            _dialogResult = MessageBoxResult.Cancel;
            RemoveBlackOutEffect();
            _window.Close();
        }
    }
}

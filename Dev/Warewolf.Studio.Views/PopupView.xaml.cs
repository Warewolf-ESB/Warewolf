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
           _dialogResult = MessageBoxResult.None;
        }

        #region Implementation of IPopupWindow

        public MessageBoxResult Show(IPopupMessage message)
        {
            InitializeComponent(); //Note this is here due to the fact that the Header does not update if set at later stage than the InitializeComponent
            MessageText.Text = message.Description;
            IsModal = true;
            Header = message.Header;
            var imageSource = new MessageBoxImageToSystemIconConverter().Convert(message.Image, null, null, null) as string;
            if(imageSource != null)
            {
                MessageImage.Source = new BitmapImage(new Uri(imageSource));
            }
            SetupButtons(message);
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

        private void SetupButtons(IPopupMessage message)
        {
            switch (message.Buttons)
            {
                case MessageBoxButton.OK:
                    OkButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Collapsed;
                    NoButton.Visibility = Visibility.Collapsed;
                    YesButton.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.OKCancel:
                    OkButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Collapsed;
                    YesButton.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNoCancel:
                    OkButton.Visibility = Visibility.Collapsed;
                    CancelButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    YesButton.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNo:
                    OkButton.Visibility = Visibility.Collapsed;
                    CancelButton.Visibility = Visibility.Collapsed;
                    NoButton.Visibility = Visibility.Visible;
                    YesButton.Visibility = Visibility.Visible;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetDialogResult(MessageBoxResult.OK);
        }

        private void SetDialogResult(MessageBoxResult messageBoxResult)
        {
            _dialogResult = messageBoxResult;
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
            SetDialogResult(MessageBoxResult.Cancel);
        }

        private void YesButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetDialogResult(MessageBoxResult.Yes);
        }

        private void NoButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetDialogResult(MessageBoxResult.No);
        }
    }
}

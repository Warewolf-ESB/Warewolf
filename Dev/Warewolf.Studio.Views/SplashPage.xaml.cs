using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for SplashPage.xaml
    /// </summary>
    public partial class SplashPage : ISplashView
    {
        Grid _blackoutGrid;
        bool _isDialog;

        public SplashPage()
        {
            if (_isDialog)
            {
                var content = Application.Current.MainWindow.Content as Grid;
                _blackoutGrid = new Grid();
                _blackoutGrid.Background = new SolidColorBrush(Colors.Black);
                _blackoutGrid.Opacity = 0.75;
                if (content != null)
                {
                    content.Children.Add(_blackoutGrid);
                }
            }
            InitializeComponent();
        }

        void SplashPage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDialog)
            {
                RemoveBlackOutEffect();
            }
            CloseSplash();
        }

        public void CloseSplash()
        {
            if (_isDialog)
            {
                Close();
            }
            else
            {
                Dispatcher.InvokeShutdown();
            }
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

        public void Show(bool isDialog)
        {
            _isDialog = isDialog;
            if (_isDialog)
            {
                ShowDialog();
            }
            else
            {
                base.Show();
            }
        }

        void SplashPage_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        void SplashPage_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}

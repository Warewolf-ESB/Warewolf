using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Dev2.Common;
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
                _blackoutGrid = new Grid
                {
                    Background = new SolidColorBrush(Colors.DarkGray),
                    Opacity = 0.5
                };
                if (content != null)
                {
                    content.Children.Add(_blackoutGrid);
                }
            }
            Topmost = true;
            try
            {
                InitializeComponent();
            }
            catch(Exception e)
            {
                Dev2Logger.Error(e.Message,e);
            }
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
                RemoveBlackOutEffect();
                Close();
            }
        }

        #region Implementation of IComponentConnector

        /// <summary>
        /// Attaches events and names to compiled content. 
        /// </summary>
        /// <param name="connectionId">An identifier token to distinguish calls.</param><param name="target">The target to connect events and names to.</param>
        public void Connect(int connectionId, object target)
        {
        }

        #endregion

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            CloseSplash();
        }
    }
}

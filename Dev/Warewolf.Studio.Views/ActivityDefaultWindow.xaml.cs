using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Dev2;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Decision;
using Dev2.Common.Interfaces.Studio.Controller;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ActivityDefaultWindow.xaml
    /// </summary>
    public partial class ActivityDefaultWindow
    {
        Grid _blackoutGrid;
        private static readonly IPopupController PopupController = CustomContainer.Get<IPopupController>();

        public ActivityDefaultWindow()
        {
            InitializeComponent();
            AddBlackOutEffect();
        }

        void WindowBorderLess_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        void WindowBorderLess_OnClosed(object sender, EventArgs e)
        {
            RemoveBlackOutEffect();
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
        void AddBlackOutEffect()
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

        #region Implementation of IComponentConnector

        #endregion

        void DoneButton_OnClick(object sender, RoutedEventArgs e)
        {
            bool valid = true;
            var content = ControlContentPresenter.Content as ActivityDesignerTemplate;

            if (content != null)
            {
                var dataContext = content.DataContext as DecisionDesignerViewModel;
                if(dataContext != null)
                {
                    dataContext.Validate();
                    if (dataContext.Errors != null)
                    {
                        PopupController.Show(dataContext.Errors[0].Message, "Decision Error", MessageBoxButton.OK, MessageBoxImage.Error, "", false, true, false, false);
                        valid = false;
                    }
                }
            }

            if (valid)
            {
                DialogResult = true;
                Close();
            }
        }

        void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

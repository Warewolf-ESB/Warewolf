using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Dev2.Studio.Core.Views
{
    /// <summary>
    /// Interaction logic for JsonObjectsView.xaml
    /// </summary>
    public partial class JsonObjectsView
    {
        Grid _blackoutGrid;

        public JsonObjectsView()
        {
            InitializeComponent();
            AddBlackOutEffect();
        }

        void JsonObjectsView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        void JsonObjectsView_OnClosed(object sender, EventArgs e)
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

        void DoneButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}

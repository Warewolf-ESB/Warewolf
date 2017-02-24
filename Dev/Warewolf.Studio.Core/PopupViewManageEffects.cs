using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Warewolf.Studio.Core
{
    public static class PopupViewManageEffects
    {
        public static void AddBlackOutEffect(Grid blackoutGrid)
        {
            var effect = new BlurEffect { Radius = 3, KernelType = KernelType.Gaussian, RenderingBias = RenderingBias.Quality };
            blackoutGrid.Background = new SolidColorBrush(Colors.DarkGray);
            blackoutGrid.Opacity = 0.2;
            var content = Application.Current?.MainWindow?.Content as Grid;
            content?.Children.Add(blackoutGrid);
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Effect = effect;
            }
        }

        public static void RemoveBlackOutEffect(Grid blackoutGrid)
        {
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                if (!Application.Current.Dispatcher.CheckAccess())
                    return;
                Application.Current.MainWindow.Effect = null;
                var content = Application.Current.MainWindow.Content as Grid;
                content?.Children.Remove(blackoutGrid);
            }
        }
    }
}

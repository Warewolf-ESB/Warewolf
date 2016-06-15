
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Dev2.Common.Interfaces;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageWebservicePasteView.xaml
    /// </summary>
    public partial class ManageWebservicePasteView : IPasteView
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

            _window = new Window { WindowStyle = WindowStyle.None, AllowsTransparency = true, Background = Brushes.Transparent, SizeToContent = SizeToContent.Manual, MinWidth = 640, MinHeight = 480, ResizeMode = ResizeMode.CanResize, WindowStartupLocation = WindowStartupLocation.CenterScreen, Content = this };
            var vm = new ManagePasteViewModel(text, RequestClose);
            _window.DataContext = vm;
            _window.ShowDialog();
            return vm.Text;
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
    }
}

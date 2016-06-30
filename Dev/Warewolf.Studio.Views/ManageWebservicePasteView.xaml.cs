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
using Dev2.Common.Interfaces;
using Warewolf.Studio.Core;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageWebservicePasteView.xaml
    /// </summary>
    public partial class ManageWebservicePasteView : IPasteView
    {
        readonly Grid _blackoutGrid = new Grid();
        Window _window;

        public ManageWebservicePasteView()
        {
            InitializeComponent();
        }

        public string ShowView(string text)
        {
            IsModal = true;
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);

            _window = new Window { WindowStyle = WindowStyle.None, AllowsTransparency = true, Background = Brushes.Transparent, SizeToContent = SizeToContent.Manual, MinWidth = 640, MinHeight = 480, ResizeMode = ResizeMode.CanResize, WindowStartupLocation = WindowStartupLocation.CenterScreen, Content = this };
            var vm = new ManagePasteViewModel(text, RequestClose);
            _window.DataContext = vm;
            _window.ShowDialog();
            return vm.Text;
        }

        private void RequestClose()
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
            _window.Close();
        }
    }
}

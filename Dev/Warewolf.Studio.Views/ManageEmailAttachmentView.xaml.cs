/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dev2;
using Dev2.Common.Interfaces;
using Warewolf.Studio.Core;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageEmailAttachmentView.xaml
    /// </summary>
    public partial class ManageEmailAttachmentView : IEmailAttachmentView
    {
        readonly Grid _blackoutGrid = new Grid();
        Window _window;

        public ManageEmailAttachmentView()
        {
            InitializeComponent();
        }

        public void ShowView(IList<string> attachments)
        {
            IsModal = true;
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);

            _window = new Window { WindowStyle = WindowStyle.None, AllowsTransparency = true, Background = Brushes.Transparent, SizeToContent = SizeToContent.Manual, MinWidth = 640, MinHeight = 480, ResizeMode = ResizeMode.CanResize, WindowStartupLocation = WindowStartupLocation.CenterScreen, Content = this };
            
            var server = CustomContainer.Get<IServer>();
            var vm = new EmailAttachmentVm(attachments, new EmailAttachmentModel(server.QueryProxy), RequestClose);
            _window.DataContext = vm;
            _window.ShowDialog();
        }

        void RequestClose()
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
            _window.Close();
        }
    }
}

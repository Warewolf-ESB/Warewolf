﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Wrappers;
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

        public ManageEmailAttachmentView()
        {
            InitializeComponent();
        }

        public void ShowView(IList<string> attachments)
        {
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
            var server = CustomContainer.Get<IServer>();
            var vm = new EmailAttachmentVm(attachments, new EmailAttachmentModel(server.QueryProxy), RequestClose);
            DataContext = vm;
            ShowDialog();
        }

        public void ShowView(IList<string> attachments, string filter)
        {
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
            var server = CustomContainer.Get<IServer>();
            var vm = new EmailAttachmentVm(attachments, new EmailAttachmentModel(server.QueryProxy, filter, new FileWrapper()), RequestClose);
            DataContext = vm;
            ShowDialog();
        }

        public void RequestClose()
        {
            Close();
        }

        private void ManageEmailAttachmentView_OnClosing(object sender, CancelEventArgs e)
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
        }

        private void ManageEmailAttachmentView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void ManageEmailAttachmentView_OnKeyUp(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Control)) && (e.Key == Key.F4))
            {
                if (Application.Current != null)
                {
                    var windowCollection = Application.Current.Windows;

                    foreach (var window in windowCollection)
                    {
                        var window1 = window as Window;

                        if (window1 != null && window1.Name != "MainViewWindow")
                        {
                            window1.Close();
                        }
                    }
                }
            }
        }
    }
}

/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
    /// Interaction logic for FileChooserView.xaml
    /// </summary>
    public partial class FileChooserView : IFileChooserView
    {
        readonly Grid _blackoutGrid = new Grid();

        public FileChooserView()
        {
            InitializeComponent();
        }

        public void ShowView(IList<string> files)
        {
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
            var server = CustomContainer.Get<IServer>();
            var vm = new FileChooser(files, new FileChooserModel(server.QueryProxy), RequestClose, true);
            DataContext = vm;
            ShowDialog();
        }

        public void ShowView(IList<string> files, string filter)
        {
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
            var server = CustomContainer.Get<IServer>();
            var vm = new FileChooser(files, new FileChooserModel(server.QueryProxy, filter, new FileWrapper()), RequestClose, true);
            DataContext = vm;
            ShowDialog();
        }

        public void ShowView(bool allowMultipleSelection)
        {
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
            var server = CustomContainer.Get<IServer>();
            var vm = new FileChooser(new List<string>(), new FileChooserModel(server.QueryProxy), RequestClose, allowMultipleSelection);
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

        private void DrivesDataTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var newValueModel = e.NewValue as IFileListingModel;
            if (newValueModel != null && !newValueModel.IsDirectory)
            {
                newValueModel.IsSelected = true;
                DriveNameIntellisenseTextBox.Text = newValueModel.FullName;
            }

            var oldValueModel = e.OldValue as IFileListingModel;
            if (oldValueModel != null)
            {
                oldValueModel.IsSelected = false;
            }
        }

        private void DriveNameIntellisenseTextBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0) // make sure there is at least one item..
            {
                var selection = e.AddedItems[0];

                var fileChooser = DataContext as FileChooser;
                if (fileChooser != null)
                {
                    fileChooser.SelectedDriveName = selection.ToString();
                }
            }
        }
    }
}

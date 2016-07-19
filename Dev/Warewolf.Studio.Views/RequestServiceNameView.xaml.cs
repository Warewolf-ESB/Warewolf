using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2.Common.Interfaces;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for RequestServiceNameView.xaml
    /// </summary>
    public partial class RequestServiceNameView:IRequestServiceNameView
    {
        readonly Grid _blackoutGrid = new Grid();

        public RequestServiceNameView()
        {
            InitializeComponent();
        }

        public void ShowView()
        {
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);

            ContentRendered += (sender, args) =>
            {
                Dispatcher.BeginInvoke(
                    DispatcherPriority.Render,
                    new Action(delegate
                    {
                        ServiceNameTextBox.Focus();
                    }));
            };
            Closing += WindowClosing;
            ShowDialog();
        }

        void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
        }

        public void RequestClose()
        {
            Close();
        }

        public bool HasServer(string serverName)
        {
            ExplorerViewTestClass viewTestClass = new ExplorerViewTestClass(ExplorerView);
            var environmentViewModel = viewTestClass.OpenEnvironmentNode(serverName);
            return environmentViewModel != null;
        }

        public void CreateNewFolder(string newFolderName, string rootPath)
        {
            ExplorerViewTestClass viewTestClass = new ExplorerViewTestClass(ExplorerView);
            viewTestClass.PerformFolderAdd(newFolderName,rootPath);
        }

        public IExplorerView GetExplorerView()
        {
            return ExplorerView;
        }

        public void OpenFolder(string folderName)
        {
            ExplorerViewTestClass viewTestClass = new ExplorerViewTestClass(ExplorerView);
            viewTestClass.OpenFolderNode(folderName);
        }

        public void EnterName(string serviceName)
        {
            ServiceNameTextBox.Text = serviceName;            
        }

        public bool IsSaveButtonEnabled()
        {
            return OkButton.Command.CanExecute(null);
        }

        public string GetValidationMessage()
        {
            BindingExpression be = ErrorMessageTextBlock.GetBindingExpression(TextBlock.TextProperty);
            be?.UpdateTarget();
            return ErrorMessageTextBlock.Text;
        }

        public List<IExplorerTreeItem> GetFoldersVisible()
        {
            ExplorerViewTestClass viewTestClass = new ExplorerViewTestClass(ExplorerView);
            return viewTestClass.GetFoldersVisible();
        }

        public void Filter(string filter)
        {
            ExplorerViewTestClass viewTestClass = new ExplorerViewTestClass(ExplorerView);
            viewTestClass.PerformSearch(filter);
        }

        public void Cancel()
        {
            CancelButton.Command.Execute(null);
        }

        public void PerformActionOnContextMenu(string menuAction, string itemName, string path)
        {
            ExplorerViewTestClass viewTestClass = new ExplorerViewTestClass(ExplorerView);
            viewTestClass.PerformActionOnContextMenu(menuAction, itemName,path);
        }

        public IExplorerTreeItem GetCurrentItem()
        {
            ExplorerViewTestClass viewTestClass = new ExplorerViewTestClass(ExplorerView);
            return viewTestClass.GetCurrentItem();
        }

        public void CreateNewFolderInFolder(string newFolderName, string currentFolder)
        {
            ExplorerViewTestClass viewTestClass = new ExplorerViewTestClass(ExplorerView);
            viewTestClass.PerformFolderAdd(currentFolder + "/" + newFolderName);
        }

        public void Save()
        {
            OkButton.Command.Execute(null);
        }

        private void ExplorerView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ExplorerView?.ExplorerTree?.EditingSettings != null)
            {
                ExplorerView.ExplorerTree.EditingSettings.IsF2EditingEnabled = false;
            }
        }

        private void RequestServiceNameView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}

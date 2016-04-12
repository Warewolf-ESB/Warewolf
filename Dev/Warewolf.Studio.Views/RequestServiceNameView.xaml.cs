using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for RequestServiceNameView.xaml
    /// </summary>
    public partial class RequestServiceNameView:IRequestServiceNameView
    {
        private Grid _blackoutGrid;

        public RequestServiceNameView()
        {
            InitializeComponent();
        }

        public void ShowView()
        {
            Grid content;
            var effect = BlurEffect(out content);
            if (content != null)
            {
                content.Children.Add(_blackoutGrid);
            }
            if(Application.Current != null)
            {
                Application.Current.MainWindow.Effect = effect;
            }

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

        private BlurEffect BlurEffect(out Grid content)
        {
            var effect = new BlurEffect { Radius = 10, KernelType = KernelType.Gaussian, RenderingBias = RenderingBias.Quality };
            content = Application.Current.MainWindow.Content as Grid;
            _blackoutGrid = new Grid
            {
                Background = new SolidColorBrush(Colors.DarkGray),
                Opacity = 0.5
            };
            return effect;
        }

        void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
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

        public void RequestClose()
        {
            RemoveBlackOutEffect();
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
            if (be != null)
            {
                be.UpdateTarget();
            }
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

        /// <summary>
        /// Attaches events and names to compiled content. 
        /// </summary>
        /// <param name="connectionId">An identifier token to distinguish calls.</param><param name="target">The target to connect events and names to.</param>
        public void Connect(int connectionId, object target)
        {
        }

        private void ExplorerView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ExplorerView != null)
            {
                if (ExplorerView.ExplorerTree != null)
                {
                    if (ExplorerView.ExplorerTree.EditingSettings != null)
                    {
                        ExplorerView.ExplorerTree.EditingSettings.IsF2EditingEnabled = false;
                    }
                }
            }
        }
    }
}

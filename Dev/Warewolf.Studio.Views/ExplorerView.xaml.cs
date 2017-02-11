using System;
using System.Activities.Presentation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Interfaces;
using Warewolf.Studio.ViewModels;
// ReSharper disable MemberCanBePrivate.Global

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ExplorerView.xaml
    /// </summary>
    public partial class ExplorerView
    {
        public ExplorerView()
        {
            InitializeComponent();
        }

        private Point _startPoint;
        private bool _isDragging;
        private bool _canDrag;

        private void StartDrag()
        {
            _isDragging = true;
            var temp = ExplorerTree.SelectedItem as ExplorerItemViewModel;
            if (temp != null)
            {
                var dragData = new DataObject();
                dragData.SetData(DragDropHelper.WorkflowItemTypeNameFormat, temp.ActivityName);
                dragData.SetData(temp);
                DragDrop.DoDragDrop(this, dragData, DragDropEffects.Copy);
            }
            _isDragging = false;
        }

        private void TreeMouseMove(object sender, MouseEventArgs e)
        {
            if (_canDrag)
            {
                if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed && !_isDragging)
                {
                    Point position = e.GetPosition(null);
                    if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                    {
                        StartDrag();
                    }
                }
            }
        }

        private void DropTreeDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (_canDrag)
                {
                    if (e.Data.GetDataPresent(typeof (ExplorerItemViewModel)))
                    {
                        var explorerItemViewModel = e.Data.GetData(typeof (ExplorerItemViewModel)) as ExplorerItemViewModel;
                        if (explorerItemViewModel == null)
                        {
                            e.Handled = true;
                            return;
                        }
                        var destination = FindAncestor<TreeViewItem>((DependencyObject) e.OriginalSource);

                        if (!Equals(explorerItemViewModel.Parent, destination.DataContext))
                        {
                            var dropTarget = destination.DataContext as IExplorerItemViewModel;

                            if (dropTarget != null && dropTarget.IsFolder)
                            {
                                var itemViewModel = (IExplorerItemViewModel) explorerItemViewModel;
                                itemViewModel.Move(dropTarget);
                            }
                            var destEnv = destination.DataContext as IEnvironmentViewModel;
                            if (destEnv != null)
                            {
                                var itemViewModel = (IExplorerItemViewModel) explorerItemViewModel;
                                itemViewModel.Move(destEnv);
                            }
                            else
                            {
                                return;
                            }
                        }
                        destination.Background = Brushes.Transparent;
                        _isDragging = false;
                        _canDrag = false;
                    }
                }
            }
            catch (Exception)
            {
                _isDragging = false;
                _canDrag = false;
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            try
            {
                do
                {
                    var anchestor = current as T;
                    if (anchestor != null)
                    {
                        return anchestor;
                    }
                    current = VisualTreeHelper.GetParent(current);
                }
                while (current != null);
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void ExplorerTree_OnDragEnter(object sender, DragEventArgs e)
        {
            if (_canDrag)
            {
                ValidateDragEnter(sender, e);
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void ExplorerTree_OnDragOver(object sender, DragEventArgs e)
        {
            if (_canDrag)
            {
                ValidateDragEnter(sender, e);
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private static void ValidateDragEnter(object sender, DragEventArgs e)
        {
            TreeViewItem treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
            var dropOntoItem = treeViewItem?.DataContext as ExplorerItemViewModel;
            var treeView = sender as TreeView;
            var itemToMove = treeView?.SelectedItem as ExplorerItemViewModel;
            if (itemToMove == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
            else if (dropOntoItem == null || !dropOntoItem.IsFolder)
            {
                var environmentViewModel = treeViewItem?.DataContext as EnvironmentViewModel;

                if (environmentViewModel == null)
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                }
                else if (itemToMove.Server.EnvironmentID != environmentViewModel.ResourceId)
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                }
                else if (Equals(itemToMove.Parent, environmentViewModel))
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                }
                else
                {
                    e.Effects = DragDropEffects.Copy;
                }
            }
            else
            {
                if (itemToMove.Server != dropOntoItem.Server)
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                }
                else if (dropOntoItem.ResourcePath.Contains(itemToMove.ResourceName))
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                }
                else
                {
                    e.Effects = DragDropEffects.Copy;
                }
            }
        }

        private void ExplorerTree_OnDragLeave(object sender, DragEventArgs e)
        {
            TreeViewItem treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
            if (treeViewItem != null)
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        private void ExplorerTree_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _canDrag = false;

            if (e.OriginalSource.GetType() == typeof (ScrollViewer))
            {
                var explorerView = sender as ExplorerView;
                var singleEnvironmentExplorerViewModel = explorerView?.DataContext as SingleEnvironmentExplorerViewModel;
                if (singleEnvironmentExplorerViewModel != null) return;
                var explorerItemViewModel = ExplorerTree.SelectedItem as ExplorerItemViewModel;
                var environmentViewModel = ExplorerTree.SelectedItem as EnvironmentViewModel;
                if (explorerItemViewModel != null && explorerItemViewModel.IsSelected)
                {
                    explorerItemViewModel.IsSelected = false;
                }
                else if (environmentViewModel != null && environmentViewModel.IsSelected)
                {
                    environmentViewModel.IsSelected = false;
                }
            }
        }

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape || e.Key == Key.Tab)
            {
                var textBox = sender as TextBox;
                var explorerItemViewModel = textBox?.DataContext as ExplorerItemViewModel;
                if (explorerItemViewModel != null)
                {
                    explorerItemViewModel.ResourceName = textBox.Text;
                }
            }
        }
        
        private void UIElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            var explorerItemViewModel = textBox?.DataContext as ExplorerItemViewModel;
            if (explorerItemViewModel != null && explorerItemViewModel.IsRenaming && !explorerItemViewModel.IsSaveDialog)
            {
                explorerItemViewModel.ResourceName = textBox.Text;
            }
        }

        private void UIElement_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            Keyboard.Focus(textBox);
            textBox?.SelectAll();
        }

        private void TreeViewItemPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeView = sender as TreeView;
            TreeViewItem treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
            if (treeViewItem != null)
            {
                SetSelected(treeView,treeViewItem.DataContext);
            }
            _isDragging = false;
            _canDrag = false;
        }

        private void TreeViewItemPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
            if (treeViewItem != null)
            {
                treeViewItem.Focus();

                var explorerItemViewModel = treeViewItem.DataContext as ExplorerItemViewModel;

                if (explorerItemViewModel == null || !explorerItemViewModel.CanDrag)
                {
                    _canDrag = false;
                }
                else
                {
                    _canDrag = true;
                    // Store the mouse position
                    _startPoint = e.GetPosition(null);
                }
            }
        }

        private void ExplorerTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var treeView = sender as TreeView;
            var item = e.NewValue;
            SetSelected(treeView, item);
        }

        private void SetSelected(TreeView treeView, object item)
        {
            var singleEnvironmentExplorerViewModel = treeView?.DataContext as SingleEnvironmentExplorerViewModel;
            if(singleEnvironmentExplorerViewModel != null)
            {
                var explorerItemViewModel = item as IExplorerItemViewModel;
                if(explorerItemViewModel != null)
                {
                    singleEnvironmentExplorerViewModel.SelectedItem = explorerItemViewModel;
                    singleEnvironmentExplorerViewModel.SelectedEnvironment = null;
                }
                else
                {
                    var environmentViewModel = item as IEnvironmentViewModel;
                    singleEnvironmentExplorerViewModel.SelectedEnvironment = environmentViewModel;
                    singleEnvironmentExplorerViewModel.SelectedItem = null;
                }
            }
            else
            {
                var explorerViewModel = DataContext as ExplorerViewModel;

                var explorerItemViewModel = item as IExplorerItemViewModel;
                if(explorerItemViewModel != null)
                {
                    explorerItemViewModel.IsSelected = true;
                    if(explorerViewModel?.ConnectControlViewModel != null)
                        explorerViewModel.ConnectControlViewModel.SelectedConnection = explorerItemViewModel.Server;
                }
                else
                {
                    var environmentViewModel = item as IEnvironmentViewModel;
                    if(environmentViewModel != null)
                    {
                        environmentViewModel.IsSelected = true;
                        if(explorerViewModel?.ConnectControlViewModel != null)
                            explorerViewModel.ConnectControlViewModel.SelectedConnection = environmentViewModel.Server;
                    }
                }
            }
        }

        private void ExplorerTree_OnKeyUp(object sender, KeyEventArgs e)
        {
            var treeView = sender as TreeView;
            var singleEnvironmentExplorerViewModel = treeView?.DataContext as SingleEnvironmentExplorerViewModel;
            if (singleEnvironmentExplorerViewModel != null)
            {
                if (e.Key == Key.F2)
                {
                    var expItemViewModel = ExplorerTree.SelectedItem as ExplorerItemViewModel;
                    if (expItemViewModel != null)
                        expItemViewModel.IsRenaming = expItemViewModel.CanRename;
                }
                return;
            }
            var explorerItemViewModel = ExplorerTree.SelectedItem as ExplorerItemViewModel;
            if (explorerItemViewModel != null && explorerItemViewModel.IsSelected)
            {
                ExplorerItemShortcuts(e, explorerItemViewModel);
            }
            else
            {
                var environmentViewModel = ExplorerTree.SelectedItem as EnvironmentViewModel;
                if (environmentViewModel != null && environmentViewModel.IsSelected)
                {
                    EnvironmentShortcuts(e, environmentViewModel);
                }
            }
        }

        private static void ExplorerItemShortcuts(KeyEventArgs e, ExplorerItemViewModel explorerItemViewModel)
        {
            if (e.Key == Key.F2)
            {
                explorerItemViewModel.IsRenaming = explorerItemViewModel.CanRename;
            }
            if (e.Key == Key.D && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (explorerItemViewModel.CanDeploy)
                    explorerItemViewModel.DeployCommand.Execute(null);
            }
            if (explorerItemViewModel.IsFolder)
            {
                if (e.Key == Key.W && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    explorerItemViewModel.NewServiceCommand.Execute(null);
                }
                if (e.Key == Key.F && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
                {
                    explorerItemViewModel.CreateNewFolder();
                }
            }
            if (e.Key == Key.Delete)
            {
                if (!explorerItemViewModel.IsRenaming && !explorerItemViewModel.IsSaveDialog)
                {
                    explorerItemViewModel.DeleteCommand.Execute(null);
                }
            }
        }

        private static void EnvironmentShortcuts(KeyEventArgs e, EnvironmentViewModel environmentViewModel)
        {
            if (e.Key == Key.W && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                environmentViewModel.NewServiceCommand.Execute(null);
            }
            if (e.Key == Key.D && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                environmentViewModel.DeployCommand.Execute(null);
            }
            if (e.Key == Key.F && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                environmentViewModel.CreateFolder();
            }
        }

        private void ExplorerView_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (ExplorerTree.SelectedItem == null)
            {
                if (e.Key == Key.W && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    var mainViewModel = CustomContainer.Get<IMainViewModel>();
                    mainViewModel?.NewServiceCommand.Execute(null);
                }
            }
        }

        private void ExplorerContextMenuView_OnKeyUp(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Control)) && (e.Key == Key.F4))
            {
                var mainViewModel = CustomContainer.Get<IShellViewModel>();
                mainViewModel?.ResetMainView();
            }
        }

        private void TreeViewItemRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            Keyboard.Focus(textBox);
            textBox?.SelectAll();
        }

        private void ExplorerTree_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ExplorerTree?.Items?.Refresh();
            ExplorerTree?.UpdateLayout();
        }

        private void TreeViewItemPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = FindAncestor<TreeViewItem>((DependencyObject) e.OriginalSource);
            var explorerItemViewModel = treeViewItem?.DataContext as ExplorerItemViewModel;
            if (explorerItemViewModel != null)
            {
                if (ValidateItemDoubleClick(e, explorerItemViewModel)) return;

                if (explorerItemViewModel.IsFolder)
                {
                    explorerItemViewModel.IsExpanded = !explorerItemViewModel.IsExpanded;
                    e.Handled = false;
                }
                else if (explorerItemViewModel.CanView)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    explorerItemViewModel.OpenCommand.Execute(this);
                    e.Handled = true;
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private static bool ValidateItemDoubleClick(MouseButtonEventArgs e, ExplorerItemViewModel explorerItemViewModel)
        {
            if (explorerItemViewModel.CanView && !explorerItemViewModel.IsFolder && explorerItemViewModel.IsSaveDialog)
            {
                e.Handled = true;
                return true;
            }
            if (!explorerItemViewModel.CanView && !explorerItemViewModel.IsFolder)
            {
                e.Handled = true;
                return true;
            }
            return false;
        }
    }
}
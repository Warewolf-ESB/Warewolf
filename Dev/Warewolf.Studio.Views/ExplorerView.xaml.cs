using System;
using System.Activities.Presentation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dev2;
using Dev2.Studio.Interfaces;
using Warewolf.Studio.ViewModels;


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
            if (ExplorerTree.SelectedItem is ExplorerItemViewModel temp)
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
                    if (e.Data.GetDataPresent(typeof(ExplorerItemViewModel)))
                    {
                        if (!(e.Data.GetData(typeof(ExplorerItemViewModel)) is ExplorerItemViewModel explorerItemViewModel))
                        {
                            e.Handled = true;
                            return;
                        }
                        var destination = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);

                        if (!Equals(explorerItemViewModel.Parent, destination.DataContext))
                        {

                            if (destination.DataContext is IExplorerItemViewModel dropTarget && dropTarget.IsFolder)
                            {
                                var itemViewModel = (IExplorerItemViewModel)explorerItemViewModel;
                                itemViewModel.Move(dropTarget);
                            }
                            if (destination.DataContext is IEnvironmentViewModel destEnv)
                            {
                                var itemViewModel = (IExplorerItemViewModel)explorerItemViewModel;
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
                    if (current is T anchestor)
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
            if (!(treeView?.SelectedItem is ExplorerItemViewModel itemToMove))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
            else if (dropOntoItem == null || !dropOntoItem.IsFolder)
            {
                if (!(treeViewItem?.DataContext is EnvironmentViewModel environmentViewModel))
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
                if (!Equals(itemToMove.Server, dropOntoItem.Server))
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

            if (e.OriginalSource.GetType() == typeof(ScrollViewer))
            {
                var explorerView = sender as ExplorerView;
                if (explorerView?.DataContext is SingleEnvironmentExplorerViewModel singleEnvironmentExplorerViewModel)
                {
                    return;
                }

                var explorerItemViewModel = ExplorerTree.SelectedItem as ExplorerItemViewModel;
                var environmentViewModel = ExplorerTree.SelectedItem as EnvironmentViewModel;
                if (explorerItemViewModel != null && explorerItemViewModel.IsSelected)
                {
                    explorerItemViewModel.IsSelected = false;
                }
                else
                {
                    if (environmentViewModel != null && environmentViewModel.IsSelected)
                    {
                        environmentViewModel.IsSelected = false;
                    }
                }
            }
        }

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape || e.Key == Key.Tab)
            {
                var textBox = sender as TextBox;
                if (textBox?.DataContext is ExplorerItemViewModel explorerItemViewModel)
                {
                    explorerItemViewModel.ResourceName = textBox.Text;
                }
            }
        }

        private void UIElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox?.DataContext is ExplorerItemViewModel explorerItemViewModel && explorerItemViewModel.IsRenaming)
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
                SetSelected(treeView, treeViewItem.DataContext);
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

                if (!(treeViewItem.DataContext is ExplorerItemViewModel explorerItemViewModel) || !explorerItemViewModel.CanDrag)
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
            if (treeView?.DataContext is SingleEnvironmentExplorerViewModel singleEnvironmentExplorerViewModel)
            {
                if (item is IExplorerItemViewModel explorerItemViewModel)
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
            else if (treeView?.DataContext is MergeServiceViewModel mergeServiceViewModel)
            {
                if (item is IExplorerItemViewModel explorerItemViewModel)
                {
                    if (explorerItemViewModel.IsService)
                    {
                        mergeServiceViewModel.SelectedMergeItem = explorerItemViewModel;
                    }
                }
            }
            else
            {
                var explorerViewModel = DataContext as ExplorerViewModel;

                if (item is IExplorerItemViewModel explorerItemViewModel)
                {
                    explorerItemViewModel.IsSelected = true;
                    SetActiveServer(explorerItemViewModel.Server);
                    if (explorerViewModel?.ConnectControlViewModel != null)
                    {
                        explorerViewModel.ConnectControlViewModel.SelectedConnection = explorerItemViewModel.Server;
                    }
                }
                else
                {
                    if (item is IEnvironmentViewModel environmentViewModel)
                    {
                        SetActiveServer(environmentViewModel.Server);
                        if (explorerViewModel?.ConnectControlViewModel != null)
                        {
                            {
                                explorerViewModel.ConnectControlViewModel.SelectedConnection = environmentViewModel.Server;
                            }
                        }
                    }
                }
            }
        }

        private static void SetActiveServer(IServer server)
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            shellViewModel.SetActiveServer(server.EnvironmentID);
        }

        private void ExplorerTree_OnKeyUp(object sender, KeyEventArgs e)
        {
            var treeView = sender as TreeView;
            if (treeView?.DataContext is SingleEnvironmentExplorerViewModel singleEnvironmentExplorerViewModel)
            {
                if (e.Key == Key.F2)
                {
                    if (ExplorerTree.SelectedItem is ExplorerItemViewModel expItemViewModel)
                    {
                        expItemViewModel.IsRenaming = expItemViewModel.CanRename;
                    }
                }
                return;
            }
            if (ExplorerTree.SelectedItem is ExplorerItemViewModel explorerItemViewModel && explorerItemViewModel.IsSelected)
            {
                ExplorerItemShortcuts(e, explorerItemViewModel);
            }
            else
            {
                if (ExplorerTree.SelectedItem is EnvironmentViewModel environmentViewModel && environmentViewModel.IsSelected)
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
                {
                    explorerItemViewModel.DeployCommand.Execute(null);
                }
            }
            if (e.Key == Key.M && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (explorerItemViewModel.CanMerge)
                {
                    explorerItemViewModel.MergeCommand.Execute(null);
                }
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
                    var mainViewModel = CustomContainer.Get<IShellViewModel>();
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
            ExplorerTree?.Items.Refresh();
            ExplorerTree?.UpdateLayout();
        }

        private void TreeViewItemPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewItem = sender as TreeViewItem;
            TreeViewItem treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);

            var name = viewItem?.DataContext?.GetType().Name;

            if (name == "EnvironmentViewModel")
            {
                e.Handled = true;
                return;
            }
            if (treeViewItem?.DataContext is ExplorerItemViewModel explorerItemViewModel)
            {
                if (ValidateItemDoubleClick(e, explorerItemViewModel))
                {
                    return;
                }

                if (explorerItemViewModel.CanView)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    if (name == "VersionViewModel")
                    {
                        explorerItemViewModel.OpenCommand.Execute(this);
                    }
                    else
                    {
                        if (name == "ExplorerItemViewModel" && !explorerItemViewModel.IsResourceVersion)
                        {
                            explorerItemViewModel.OpenCommand.Execute(this);
                        }
                    }
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

    

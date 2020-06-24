#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dev2;
using Dev2.Studio.Interfaces;
using Warewolf.Studio.ViewModels;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Instrumentation;
using Warewolf.Studio.Resources.Languages;

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
            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(new Size(ActualWidth, ActualHeight)));
        }

        Point _startPoint;
        bool _isDragging;
        bool _canDrag;

        void StartDrag()
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

        void TreeMouseMove(object sender, MouseEventArgs e)
        {
            if (_canDrag && (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed && !_isDragging))
            {
                var position = e.GetPosition(null);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    StartDrag();
                }
            }

        }

        void TryDropTreeDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (_canDrag && e.Data.GetDataPresent(typeof(ExplorerItemViewModel)))
                {
                    if (!(e.Data.GetData(typeof(ExplorerItemViewModel)) is ExplorerItemViewModel explorerItemViewModel))
                    {
                        e.Handled = true;
                        return;
                    }
                    var destination = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);

                    if (!Equals(explorerItemViewModel.Parent, destination.DataContext))
                    {
                        MoveAsync(explorerItemViewModel, destination);
                        return;
                    }
                    destination.Background = Brushes.Transparent;
                    _isDragging = false;
                    _canDrag = false;
                }

            }
            catch (Exception)
            {
                _isDragging = false;
                _canDrag = false;
            }
        }

        static void MoveAsync(ExplorerItemViewModel explorerItemViewModel, TreeViewItem destination)
        {
            if (destination.DataContext is IExplorerItemViewModel dropTarget && dropTarget.IsFolder)
            {
                var itemViewModel = (IExplorerItemViewModel)explorerItemViewModel;
                itemViewModel.MoveAsync(dropTarget);
            }
            if (destination.DataContext is IEnvironmentViewModel destEnv)
            {
                var itemViewModel = (IExplorerItemViewModel)explorerItemViewModel;
                itemViewModel.MoveAsync(destEnv);
            }
        }

        static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
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

        void ExplorerTree_OnDragEnter(object sender, DragEventArgs e)
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

        void ExplorerTree_OnDragOver(object sender, DragEventArgs e)
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

        static void ValidateDragEnter(object sender, DragEventArgs e)
        {
            var treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
            var dropOntoItem = treeViewItem?.DataContext as ExplorerItemViewModel;
            var treeView = sender as TreeView;
            if (!(treeView?.SelectedItem is ExplorerItemViewModel itemToMove))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
            else
            {
                if (dropOntoItem == null || !dropOntoItem.IsFolder)
                {
                    if (!(treeViewItem?.DataContext is EnvironmentViewModel environmentViewModel))
                    {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                    }
                    else
                    {
                        Drag(e, itemToMove, environmentViewModel);
                    }
                }
                else
                {
                    if (!Equals(itemToMove.Server, dropOntoItem.Server))
                    {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                    }
                    else
                    {
                        Drag(e, itemToMove, dropOntoItem);
                    }
                }
            }
        }

        static void Drag(DragEventArgs e, ExplorerItemViewModel itemToMove, ExplorerItemViewModel dropOntoItem)
        {
            if (dropOntoItem.ResourcePath.Contains(itemToMove.ResourceName))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        static void Drag(DragEventArgs e, ExplorerItemViewModel itemToMove, EnvironmentViewModel environmentViewModel)
        {
            if (itemToMove.Server.EnvironmentID != environmentViewModel.ResourceId)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
            else
            {
                if (Equals(itemToMove.Parent, environmentViewModel))
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

        void ExplorerTree_OnDragLeave(object sender, DragEventArgs e)
        {
            var treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
            if (treeViewItem != null)
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        void ExplorerTree_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

                if (ExplorerTree.SelectedItem is ExplorerItemViewModel explorerItemViewModel && explorerItemViewModel.IsSelected)
                {
                    explorerItemViewModel.IsSelected = false;
                }
                else
                {
                    if (ExplorerTree.SelectedItem is EnvironmentViewModel environmentViewModel && environmentViewModel.IsSelected)
                    {
                        environmentViewModel.IsSelected = false;
                    }
                }
            }
        }

        void UIElement_OnKeyDown(object sender, KeyEventArgs e)
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

        void UIElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox?.DataContext is ExplorerItemViewModel explorerItemViewModel && explorerItemViewModel.IsRenaming)
            {
                explorerItemViewModel.ResourceName = textBox.Text;
            }
        }

        void UIElement_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            Keyboard.Focus(textBox);
            textBox?.SelectAll();
        }

        void TreeViewItemPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeView = sender as TreeView;
            var treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
            if (treeViewItem != null)
            {
                SetSelected(treeView, treeViewItem.DataContext);
            }
            _isDragging = false;
            _canDrag = false;
        }

        void TreeViewItemPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is DeploySourceExplorerViewModel)
            {
                return;
            }

            var treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
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

        void ExplorerTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var treeView = sender as TreeView;
            var item = e.NewValue;
            SetSelected(treeView, item);
        }

        void SetSelected(TreeView treeView, object item)
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
                    singleEnvironmentExplorerViewModel.SelectedItem = environmentViewModel;
                }
            }
            else if (treeView?.DataContext is MergeServiceViewModel mergeServiceViewModel)
            {
                if (item is IExplorerItemViewModel explorerItemViewModel && explorerItemViewModel.IsService)
                {
                    mergeServiceViewModel.SelectedMergeItem = explorerItemViewModel;
                }
            }
            else
            {
                if (DataContext is DeploySourceExplorerViewModel)
                {
                    return;
                }

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
                    TryChangeSelectedConnection(item, explorerViewModel);
                }
            }
        }

        static void TryChangeSelectedConnection(object item, ExplorerViewModel explorerViewModel)
        {
            if (item is IEnvironmentViewModel environmentViewModel)
            {
                SetActiveServer(environmentViewModel.Server);
                if (explorerViewModel?.ConnectControlViewModel != null)
                {
                    explorerViewModel.ConnectControlViewModel.SelectedConnection = environmentViewModel.Server;
                }
            }
        }

        static void SetActiveServer(IServer server)
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            shellViewModel.SetActiveServer(server.EnvironmentID);
        }

        void ExplorerTree_OnKeyUp(object sender, KeyEventArgs e)
        {
            var treeView = sender as TreeView;
            if (treeView?.DataContext is SingleEnvironmentExplorerViewModel singleEnvironmentExplorerViewModel)
            {
                if (e.Key == Key.F2 && ExplorerTree.SelectedItem is ExplorerItemViewModel expItemViewModel)
                {
                    expItemViewModel.IsRenaming = expItemViewModel.CanRename;
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

#pragma warning disable S1541 // Methods and properties should not be too complex
        static void ExplorerItemShortcuts(KeyEventArgs e, ExplorerItemViewModel explorerItemViewModel)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (e.Key == Key.F2)
            {
                explorerItemViewModel.IsRenaming = explorerItemViewModel.CanRename;
            }
            if (e.Key == Key.D && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && explorerItemViewModel.CanDeploy)
            {
                explorerItemViewModel.DeployCommand.Execute(null);
            }

            if (e.Key == Key.M && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && explorerItemViewModel.CanMerge)
            {
                explorerItemViewModel.MergeCommand.Execute(null);
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
            if (e.Key == Key.Delete && !explorerItemViewModel.IsRenaming && !explorerItemViewModel.IsSaveDialog)
            {
                explorerItemViewModel.DeleteCommand.Execute(null);
            }

        }

        static void EnvironmentShortcuts(KeyEventArgs e, EnvironmentViewModel environmentViewModel)
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

        void ExplorerView_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (ExplorerTree.SelectedItem == null && e.Key == Key.W && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                var mainViewModel = CustomContainer.Get<IShellViewModel>();
                mainViewModel?.NewServiceCommand.Execute(null);
            }

        }

        void ExplorerContextMenuView_OnKeyUp(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Control)) && (e.Key == Key.F4))
            {
                var mainViewModel = CustomContainer.Get<IShellViewModel>();
                mainViewModel?.ResetMainView();
            }
        }

        void TreeViewItemRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            Keyboard.Focus(textBox);
            textBox?.SelectAll();
        }

        void ExplorerTree_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ExplorerTree?.Items.Refresh();
            ExplorerTree?.UpdateLayout();
        }

        void TreeViewItemPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewItem = sender as TreeViewItem;
            var treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);

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
                    TryExecuteOpenCommand(name, explorerItemViewModel);
                    e.Handled = true;
                    Mouse.OverrideCursor = null;
                }
            }
        }

        void TryExecuteOpenCommand(string name, ExplorerItemViewModel explorerItemViewModel)
        {
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
        }

        static bool ValidateItemDoubleClick(MouseButtonEventArgs e, ExplorerItemViewModel explorerItemViewModel)
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
            if (explorerItemViewModel.AllowResourceCheck)
            {
                e.Handled = true;
                return true;
            }
            return false;
        }

        void ResourceNameCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            var explorerItemViewModel = checkbox?.DataContext as ExplorerItemViewModel;
            if (explorerItemViewModel == null)
            {
                return;
            }
            if (explorerItemViewModel.IsFolder && explorerItemViewModel.ChildrenCount == 0)
            {
                var header = Studio.Resources.Languages.Core.DeployEmptyFolderHeader;
                var description = Studio.Resources.Languages.Core.DeployEmptyFolderDescription;
                ShowNoResourcesToDeploy(e, header, description);
            }
        }

        void EnvironmentNameCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            var environmentViewModel = checkbox?.DataContext as EnvironmentViewModel;
            if (environmentViewModel == null)
            {
                return;
            }
            if (environmentViewModel.ChildrenCount == 0)
            {
                var header = Studio.Resources.Languages.Core.DeployEmptyServerHeader;
                var description = Studio.Resources.Languages.Core.DeployEmptyServerDescription;
                ShowNoResourcesToDeploy(e, header, description);
            }
        }

        static void ShowNoResourcesToDeploy(RoutedEventArgs e, string header, string description)
        {
            var popupController = CustomContainer.Get<IPopupController>();
            popupController?.ShowDeployNoResourcesToDeploy(header, description);
            e.Handled = true;
        }

        private void SearchTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var _applicationTracker = CustomContainer.Get<IApplicationTracker>();
            if (sender is TextBox searchText)
            {
                _applicationTracker?.TrackCustomEvent(TrackEventExplorer.EventCategory, TrackEventExplorer.ExplorerSearch, searchText.Text);
            }
        }
    }
}
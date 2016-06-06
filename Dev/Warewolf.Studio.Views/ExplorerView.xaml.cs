using System;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Infragistics.Controls.Menus;
using Infragistics.DragDrop;
using Infragistics.Windows;
using Warewolf.Studio.ViewModels;
// ReSharper disable MemberCanBePrivate.Global

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ExplorerView.xaml
    /// </summary>
    public partial class ExplorerView : IExplorerView
    {
        private readonly ExplorerViewTestClass _explorerViewTestClass;
        DataObject _dragData;

        public ExplorerView()
        {
            InitializeComponent();
            _explorerViewTestClass = new ExplorerViewTestClass(this);
        }



        public ExplorerViewTestClass ExplorerViewTestClass
        {
            get { return _explorerViewTestClass; }
        }
        public IServer SelectedServer
        {
            get
            {
                return ConnectControl.SelectedServer;
            }
        }

        public IEnvironmentViewModel OpenEnvironmentNode(string nodeName)
        {
            return ExplorerViewTestClass.OpenEnvironmentNode(nodeName);
        }
        public IEnvironmentViewModel OpenEnvironment(string nodeName)
        {
            return ((IExplorerViewModel)DataContext).Environments.FirstOrDefault(a => a.DisplayName.Contains(nodeName));
        }

        public void CloseEnvironmentNode(string nodeName)
        {
            ExplorerViewTestClass.Close(nodeName);
        }

        public List<IExplorerTreeItem> GetFoldersVisible()
        {
            return ExplorerViewTestClass.GetFoldersVisible();
        }

        public IExplorerTreeItem OpenFolderNode(string folderName)
        {
            return ExplorerViewTestClass.OpenFolderNode(folderName);
        }

        public IExplorerTreeItem OpenItem(string resourceName, string folderName)
        {
            return ExplorerViewTestClass.OpenItem(resourceName, folderName);
        }

        public void Move(string originalPath, string destinationPath)
        {
            ExplorerViewTestClass.Move(originalPath, destinationPath);
        }

        public int GetVisibleChildrenCount(string folderName)
        {
            return ExplorerViewTestClass.GetVisibleChildrenCount(folderName);
        }

        public void PerformFolderRename(string originalFolderName, string newFolderName)
        {
            ExplorerViewTestClass.PerformFolderRename(originalFolderName, newFolderName);
        }

        public void PerformSearch(string searchTerm)
        {
            ExplorerViewTestClass.PerformSearch(searchTerm);
        }

        public void AddNewFolder(string folder, string server)
        {
            ExplorerViewTestClass.PerformFolderAdd(server, folder);
        }

        public void VerifyItemExists(string path)
        {
            ExplorerViewTestClass.VerifyItemExists(path);
        }

        public void DeletePath(string path, object env)
        {
            ExplorerViewTestClass.DeletePath(path, (IEnvironmentModel)env);
        }

        public void AddNewFolderFromPath(string path)
        {
            ExplorerViewTestClass.PerformFolderAdd(path);
        }

        public void AddNewResource(string path, string itemType)
        {
            ExplorerViewTestClass.PerformItemAdd(path);
        }

        public void AddResources(int resourceNumber, string path, string type, string name)
        {
            ExplorerViewTestClass.AddChildren(resourceNumber, path, type, name);
        }

        public int GetResourcesVisible(string path)
        {
            return ExplorerViewTestClass.GetFoldersResourcesVisible(path);
        }

        public void VerifyItemDoesNotExist(string path)
        {
            ExplorerViewTestClass.VerifyItemDoesNotExist(path);
        }

        public void Refresh()
        {
            ExplorerViewTestClass.Reset();
        }

        /// <summary>
        /// Attaches events and names to compiled content. 
        /// </summary>
        /// <param name="connectionId">An identifier token to distinguish calls.</param><param name="target">The target to connect events and names to.</param>
        public void Connect(int connectionId, object target)
        {
        }

        void ExplorerTree_OnInitializeNode(object sender, InitializeNodeEventArgs e)
        {
            var xamDataTreeNode = e.Node;
            if (xamDataTreeNode == null)
            {
                return;
            }
            var dataItem = xamDataTreeNode.Data as IExplorerItemViewModel;
            if (dataItem == null)
            {
                return;
            }
            if (!dataItem.IsRenaming)
            {
                return;
            }
            if (dataItem.ResourceName.StartsWith("New Folder"))
            {
                ExplorerTree.EnterEditMode(xamDataTreeNode);
            }
        }

        void UIElement_OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.SelectAll();
            }
        }

        void ExplorerTree_OnNodeExitedEditMode(object sender, NodeEventArgs e)
        {
            var dataItem = e.Node.Data as IExplorerItemViewModel;
            if (dataItem == null)
            {
                return;
            }
            if (dataItem.IsRenaming && dataItem.ResourceName.StartsWith("New Folder"))
            {
                ExplorerTree.EnterEditMode(e.Node);
            }
        }

        private Point startPoint;
        void DragSource_OnDragOver(object sender, DragDropMoveEventArgs e)
        {
            var drop = Utilities.GetAncestorFromType(e.DropTarget, typeof(XamDataTreeNodeControl), false) as XamDataTreeNodeControl;
            var drag = Utilities.GetAncestorFromType(e.DragSource, typeof(XamDataTreeNodeControl), false) as XamDataTreeNodeControl;
            Cursor grabbingCursor = Application.Current.TryFindResource("CursorGrabbing") as Cursor;
            Mouse.SetCursor(Cursors.Arrow);

            // Get the current mouse position
            Point mousePos = Mouse.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                StopDragging();
            }

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (drag != null && drag.Node != null && drop != null && drop.Node != null)
                {
                    if (drag.Node.Manager.ParentNode != null && drop.Node.Manager.ParentNode != null)
                    {
                        Mouse.SetCursor(!CancelDrag ? grabbingCursor : Cursors.No);
                        var dragType = drag.Node.Data.GetType();
                        var dropType = drop.Node.Data.GetType();
                        ExplorerTree.ScrollNodeIntoView(drop.Node);
                        var destination = drop.Node.Data as IExplorerItemViewModel;
                        var source = drag.Node.Data as IExplorerItemViewModel;
                        if (destination != null && source != null && dragType == dropType && destination.CanDrop)
                        {
                            IEnvironmentViewModel vmSource = GetEnv(source);
                            IEnvironmentViewModel vmDestination = GetEnv(destination);

                            if (!Equals(vmSource.ResourceName, vmDestination.ResourceName))
                            {
                                CancelDrag = true;
                                return;
                            }
                            if ((source.ResourceType == "ServerSource" || source.IsServer) &&
                                string.IsNullOrWhiteSpace(source.ResourcePath))
                            {
                                CancelDrag = true;
                                return;
                            }
                            if (source.ResourceType == "DbService" || source.ResourceType == "PluginService" ||
                                source.ResourceType == "WebService")
                            {
                                CancelDrag = true;
                                return;
                            }

                            if (!CancelDrag)
                            {
                                if (e.GetPosition(e.DropTarget).Y < drop.ActualHeight / 2)
                                {
                                    if (!destination.CanDrop && !destination.CanDrop)
                                    {
                                        Mouse.SetCursor(Cursors.No);
                                        ((Grid)Utilities.GetDescendantFromName(drop, "DropBeforeElem")).Visibility =
                                            Visibility.Collapsed;
                                        ((Grid)Utilities.GetDescendantFromName(drop, "DropAfterElem")).Visibility =
                                            Visibility.Collapsed;
                                        return;
                                    }
                                    ((Grid)Utilities.GetDescendantFromName(drop, "DropBeforeElem")).Visibility =
                                        Visibility.Visible;
                                    ((Grid)Utilities.GetDescendantFromName(drop, "DropAfterElem")).Visibility =
                                        Visibility.Visible;
                                    ((Grid)Utilities.GetDescendantFromName(drop, "main")).AllowDrop = true;
                                    return;
                                }
                                if (!destination.CanDrop && !destination.CanDrop)
                                {
                                    Mouse.SetCursor(Cursors.No);
                                    ((Grid)Utilities.GetDescendantFromName(drop, "DropAfterElem")).Visibility =
                                        Visibility.Collapsed;
                                    ((Grid)Utilities.GetDescendantFromName(drop, "DropBeforeElem")).Visibility =
                                        Visibility.Collapsed;
                                    return;
                                }
                                ((Grid)Utilities.GetDescendantFromName(drop, "DropAfterElem")).Visibility =
                                    Visibility.Visible;
                                ((Grid)Utilities.GetDescendantFromName(drop, "DropBeforeElem")).Visibility =
                                    Visibility.Visible;
                                ((Grid)Utilities.GetDescendantFromName(drop, "main")).AllowDrop = true;
                                return;
                            }
                        }
                    }
                    Mouse.SetCursor(grabbingCursor);
                    return;
                }

                var dropActivity = Utilities.GetAncestorFromType(e.DropTarget, typeof(ContentControl), false) as ContentControl;
                var dragTool = Utilities.GetAncestorFromType(e.DragSource, typeof(XamDataTreeNodeControl), false) as XamDataTreeNodeControl;

                if (dropActivity == null || dragTool == null)
                {
                    return;
                }
                var dragData = new DataObject();
                var context = dragTool.DataContext as XamDataTreeNodeDataContext;
                if (context != null)
                {
                    Mouse.SetCursor(!CancelDrag ? grabbingCursor : Cursors.No);
                    var dataContext = context.Data as ExplorerItemViewModel;

                    if (dataContext != null)
                    {
                        dragData.SetData(DragDropHelper.WorkflowItemTypeNameFormat, dataContext.ActivityName);
                        dragData.SetData(dataContext);
                    }
                    if (!CancelDrag)
                    {
                        try
                        {
                            DragDropManager.EndDrag(true);
                        }
                        catch
                        {
                            //Something
                        }
                    }
                    _dragData = dragData;
                    Mouse.SetCursor(grabbingCursor);
                    DragDrop.DoDragDrop(e.DragSource, dragData, DragDropEffects.Copy);
                }
            }
        }

        void DragSource_OnDragLeave(object sender, DragDropEventArgs e)
        {
            var drop = Utilities.GetAncestorFromType(e.DropTarget, typeof(XamDataTreeNodeControl), false) as XamDataTreeNodeControl;
            Reset(drop);
            ResetDragEvents();
        }

        void DragSource_OnDrop(object sender, DropEventArgs e)
        {
            var exp = DataContext as ExplorerViewModelBase;
            var drop = Utilities.GetAncestorFromType(e.DropTarget, typeof(XamDataTreeNodeControl), false) as XamDataTreeNodeControl;
            var drag = Utilities.GetAncestorFromType(e.DragSource, typeof(XamDataTreeNodeControl), false) as XamDataTreeNodeControl;

            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                if (drag != null && drag.Node != null && drop != null && drop.Node != null)
                {
                    if (drag.Node.Manager.ParentNode != null && drop.Node.Manager.ParentNode != null)
                    {
                        if (Equals(drag.Node.Manager.ParentNode, drop.Node))
                        {
                            CancelDrag = true;
                            Reset(drop);
                            return;
                        }
                        var destination = drop.Node.Data as IExplorerItemViewModel;
                        var source = drag.Node.Data as IExplorerItemViewModel;

                        if (source != null && destination != null)
                        {
                            IEnvironmentViewModel vmSource = GetEnv(source);
                            IEnvironmentViewModel vmDestination = GetEnv(destination);
                            if (!Equals(vmDestination.ResourceName, vmSource.ResourceName))
                            {
                                CancelDrag = true;
                                return;
                            }

                            if (((source.ResourceType == "ServerSource" || source.IsServer) &&
                                 string.IsNullOrWhiteSpace(source.ResourcePath))
                                || !destination.CanDrop || !source.CanDrag)
                            {
                                CancelDrag = true;
                                return;
                            }

                            if (!CancelDrag)
                            {
                                vmSource.IsConnecting = true;
                                if (destination.Children.Count >= 1)
                                {
                                    var checkExists =
                                        destination.Children.FirstOrDefault(o => o.ResourceId == source.ResourceId);
                                    if (checkExists == null)
                                    {
                                        if (exp != null)
                                        {
                                            exp.AllowDrag = false;
                                        }
                                        source.Move(destination).ContinueWith(async =>
                                        {
                                            vmSource.IsConnecting = false;
                                            if (exp != null)
                                            {
                                                exp.AllowDrag = true;
                                            }
                                        }, TaskScheduler.FromCurrentSynchronizationContext());
                                    }
                                }
                                else
                                {
                                    source.Move(destination).ContinueWith(async =>
                                    {
                                        vmSource.IsConnecting = false;
                                        if (exp != null)
                                        {
                                            exp.AllowDrag = true;
                                        }
                                    }, TaskScheduler.FromCurrentSynchronizationContext());
                                }
                            }
                        }

                        Reset(drop);
                    }
                }
                else
                {
                    if (Mouse.LeftButton == MouseButtonState.Released)
                    {
                        var target = e.DropTarget as ContentControl;
                        if (target != null)
                        {
                            DragDrop.DoDragDrop(e.DragSource, _dragData, DragDropEffects.Copy);
                        }

                        if (drag != null && drag.Node != null && drop != null && drop.Node != null)
                        {
                            Mouse.SetCursor(Application.Current.TryFindResource("CursorGrabbing") as Cursor);
                            var destination = drop.Node.Data as IEnvironmentViewModel;
                            var source = drag.Node.Data as IExplorerItemViewModel;
                            if (source != null && destination != null)
                            {
                                if (!CancelDrag)
                                {
                                    IEnvironmentViewModel vm = GetEnv(source);
                                    vm.IsConnecting = true;
                                    if (!source.CanDrag)
                                    {
                                        return;
                                    }
                                    if (destination.Children.Count >= 1)
                                    {
                                        var checkExists =
                                            destination.Children.FirstOrDefault(o => o.ResourceId == source.ResourceId);
                                        if (checkExists == null)
                                        {
                                            source.Move(destination).ContinueWith(async =>
                                            {
                                                vm.IsConnecting = false;
                                                if (exp != null)
                                                {
                                                    exp.AllowDrag = true;
                                                }
                                            }, TaskScheduler.FromCurrentSynchronizationContext());
                                        }
                                    }
                                    else
                                    {
                                        source.Move(destination).ContinueWith(async =>
                                        {
                                            vm.IsConnecting = false;
                                            if (exp != null)
                                            {
                                                exp.AllowDrag = true;
                                            }
                                        }, TaskScheduler.FromCurrentSynchronizationContext());
                                    }
                                }
                            }
                        }
                    }
                }
                Mouse.SetCursor(Cursors.Arrow);
            }
        }

        IEnvironmentViewModel GetEnv(IExplorerTreeItem source)
        {
            var x = source;
            var env = source as IEnvironmentViewModel;
            if (env != null)
                return env;
            return GetEnv(x.Parent);
        }

        private void Reset(XamDataTreeNodeControl drop)
        {
            if (drop != null)
            {
                ((Grid)Utilities.GetDescendantFromName(drop, "DropBeforeElem")).Visibility = Visibility.Collapsed;
                ((Grid)Utilities.GetDescendantFromName(drop, "DropAfterElem")).Visibility = Visibility.Collapsed;
            }
        }

        void DragSource_OnDragStart(object sender, DragDropStartEventArgs e)
        {
            var source = e.DragSource as FrameworkElement;
            e.DragSnapshotElement = null;
            Mouse.SetCursor(Cursors.Arrow);
            startPoint = Mouse.GetPosition(null);
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                StopDragging();
            }
            if (source != null && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var context = source.DataContext as XamDataTreeNodeDataContext;
                if (context == null)
                {
                    StopDragging();
                    return;
                }

                var dataContext = context.Data as ExplorerItemViewModel;
                if (dataContext == null)
                {
                    StopDragging();
                    return;
                }
                CancelDrag = false;

                if (((dataContext.ResourceType == "ServerSource" || dataContext.IsServer) &&
                        string.IsNullOrWhiteSpace(dataContext.ResourcePath)) || dataContext.IsRenaming)
                {
                    Mouse.SetCursor(Cursors.No);
                    return;
                }
                if (dataContext.ResourceType == "DbService" || dataContext.ResourceType == "PluginService" || dataContext.ResourceType == "WebService")
                {
                    Mouse.SetCursor(Cursors.No);
                    return;
                }
                var dragData = new DataObject();

                var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.ID == dataContext.Server.EnvironmentID);
                var hasPermissionToDrag = true;
                if (environmentModel != null && environmentModel.AuthorizationService != null)
                {
                    var canExecute = environmentModel.AuthorizationService.IsAuthorized(AuthorizationContext.Execute, dataContext.ResourceId.ToString());
                    var canView = environmentModel.AuthorizationService.IsAuthorized(AuthorizationContext.View, dataContext.ResourceId.ToString());
                    hasPermissionToDrag = canExecute && canView;
                }
                if (hasPermissionToDrag)
                {
                    if (dataContext.IsService && dataContext.ResourceType != "Version"
                        && !dataContext.IsReservedService && dataContext.ResourceType != "Unknown")
                    {
                        dragData.SetData(DragDropHelper.WorkflowItemTypeNameFormat, dataContext.ActivityName);
                        dragData.SetData(dataContext);
                    }
                    dragData.SetData(dataContext);
                }
                Mouse.SetCursor(Application.Current.TryFindResource("CursorGrabbing") as Cursor);
                _dragData = dragData;
            }
        }

        void StopDragging()
        {
            CancelDrag = true;
            try
            {
                DragDropManager.EndDrag(true);
            }
            catch (Exception e)
            {
                //aaa
            }
        }

        private Cursor _customCursor;
        void ExplorerTree_OnGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effects == DragDropEffects.Copy)
            {
                if (_customCursor == null)
                    _customCursor = Application.Current.TryFindResource("CursorGrabbing") as Cursor;

                e.UseDefaultCursors = false;
                Mouse.SetCursor(_customCursor);
            }
            else
                e.UseDefaultCursors = true;
            e.Handled = true;
        }

        void ExplorerTree_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            ResetDragEvents();
            e.Handled = true;
        }

        void ExplorerTree_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
            var source = e.Source as FrameworkElement;
            if (source != null && e.ButtonState == MouseButtonState.Pressed)
            {
                var dataContext = source.DataContext as IExplorerViewModel;
                if (dataContext != null)
                {
                    var selectedItem = dataContext.SelectedItem;
                    if (dataContext.SelectedDataItems != null && dataContext.SelectedDataItems.Any() && selectedItem == null)
                    {
                        selectedItem = dataContext.SelectedDataItems[0] as IExplorerTreeItem;
                        dataContext.SelectedItem = selectedItem;
                    }
                    if (selectedItem != null)
                    {
                        if (!selectedItem.CanDrag)
                        {
                            StopDragging();
                            ExplorerTree.QueryContinueDrag += ExplorerTreeOnQueryContinueDrag;
                            e.Handled = true;
                        }
                        else
                        {
                            CancelDrag = false;
                            ExplorerTree.QueryContinueDrag += ExplorerTreeOnQueryContinueDrag;
                        }
                    }
                }
            }
        }

        public bool CancelDrag { get; set; }

        void ExplorerTreeOnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (e.EscapePressed || CancelDrag)
            {
                e.Action = DragAction.Cancel;
            }
            else
            {
                e.Action = DragAction.Continue;
            }
        }

        void ExplorerTree_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ResetDragEvents();
            e.Handled = true;
        }

        void ExplorerView_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Released)
            {
                ResetDragEvents();
                e.Handled = true;
            }
        }

        void ResetDragEvents()
        {
            CancelDrag = false;
            Mouse.SetCursor(Cursors.Arrow);
        }
    }
}
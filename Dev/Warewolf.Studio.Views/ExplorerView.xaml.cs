using System;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Dev2.Common.Interfaces;
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
        private bool _exceptionThrown;
        private string _errorMessage;
        //private bool _isDropOntoNode;

        public ExplorerView()
        {
            InitializeComponent();
            _explorerViewTestClass = new ExplorerViewTestClass(this);
            ExplorerTree.NodeDragDrop+=ExplorerTreeOnNodeDragDrop;
        }

        private void ExplorerTreeOnNodeDragDrop(object sender, TreeDropEventArgs treeDropEventArgs)
        {
            //_isDropOntoNode = true;
            // if (treeDropEventArgs.DropDestination != TreeDropDestination.DropOnto)
            //{
            //    treeDropEventArgs.DragDropEventArgs.OperationType=OperationType.DropNotAllowed;
            //    DropNotAllowedStyle(treeDropEventArgs.DragDropEventArgs.DropTarget);
            //    _isDropOntoNode = false;
            //}
        }

        public ExplorerViewTestClass ExplorerViewTestClass => _explorerViewTestClass;

        public IServer SelectedServer => ConnectControl.SelectedServer;

        public IEnvironmentViewModel OpenEnvironmentNode(string nodeName)
        {
            return ExplorerViewTestClass.OpenEnvironmentNode(nodeName);
        }
        public IEnvironmentViewModel OpenEnvironment(string nodeName)
        {
            return ((IExplorerViewModel)DataContext).Environments.FirstOrDefault(a => a.DisplayName.Contains(nodeName));
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

        void ExplorerTree_OnInitializeNode(object sender, InitializeNodeEventArgs e)
        {
            var xamDataTreeNode = e.Node;
            var dataItem = xamDataTreeNode?.Data as IExplorerItemViewModel;
            if (dataItem == null)
            {
                return;
            }
            if (!dataItem.IsRenaming)
            {
                return;
            }
            if (dataItem.ResourceName.StartsWith(Studio.Resources.Languages.Core.NewFolderLabel))
            {
                ExplorerTree.EnterEditMode(xamDataTreeNode);
            }
        }

        void UIElement_OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox?.SelectAll();
        }

        void ExplorerTree_OnNodeExitedEditMode(object sender, NodeEventArgs e)
        {
            var dataItem = e.Node.Data as IExplorerItemViewModel;
            if (dataItem == null)
            {
                return;
            }
            if (dataItem.IsRenaming && dataItem.ResourceName.StartsWith(Studio.Resources.Languages.Core.NewFolderLabel))
            {
                ExplorerTree.EnterEditMode(e.Node);
            }
        }

        void ExplorerTree_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            ResetDragEvents();
            e.Handled = true;
        }

        public bool CancelDrag { get; set; }

        void ExplorerView_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Released)
            {
                ResetDragEvents();
                e.Handled = true;
            }
        }

        private void ExplorerTree_OnNodeDraggingStart(object sender, DragDropStartEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                ResetDragDropTemplate(e);
                StopDragging();
            }
            else
            {
                var xamDataTreeNodeControl = e.DragSource as XamDataTreeNodeControl;
                if (xamDataTreeNodeControl != null &&
                    xamDataTreeNodeControl.Node.Data.GetType() == typeof(ExplorerItemViewModel))
                {
                    DragSource dragSource = DragDropManager.GetDragSource(e.DragSource);
                    dragSource.DragOver += DragSourceDragOver;
                    dragSource.Drop += DragSourceDrop;                    
                    dragSource.DragLeave += DragSourceDragLeave;
                    dragSource.DragEnd += DragSourceDragEnd;
                }
            }
        }

        private void DragSourceDragOver(object sender, DragDropMoveEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                ResetDragDropTemplate(e);
                StopDragging();
            }
            else
            {
                //var dropTarget = e.DropTarget as XamDataTreeNodeControl;
                var dragSource = e.DragSource as XamDataTreeNodeControl;
                //if (dropTarget != null && dragSource != null)
                //{
                //    var dropNodeData = dropTarget.Node.Data;
                //    var sourceNodeData = dragSource.Node.Data;
                //    if (dropNodeData.GetType() == typeof(ExplorerItemViewModel))
                //    {
                //        var destination = dropNodeData as IExplorerItemViewModel;
                //        var source = sourceNodeData as IExplorerItemViewModel;

                //        if (destination != null && source != null)
                //        {
                //            IEnvironmentViewModel vmDestination = GetEnv(destination);

                //            if (!ValidateDragDrop(source, vmDestination) && destination.IsFolder)
                //            {
                //                e.DropNotAllowedCursorTemplate = null;
                //                e.CopyCursorTemplate = DragDropManager.CurrentCopyCursorTemplate;
                //                e.MoveCursorTemplate = DragDropManager.CurrentMoveCursorTemplate;
                //                e.OperationType = OperationType.Move;
                //                DropAllowedStyle(e.DropTarget);

                //                ClearException(e);
                //                var checkExists =
                //                    destination.Children.FirstOrDefault(o => o.ResourceName == source.ResourceName);
                //                if (checkExists != null)
                //                {
                //                    SetException(e);
                //                }
                //            }
                //            else
                //            {
                //                e.DropNotAllowedCursorTemplate = DragDropManager.CurrentDropNotAllowedCursorTemplate;
                //                e.CopyCursorTemplate = DragDropManager.CurrentDropNotAllowedCursorTemplate;
                //                e.MoveCursorTemplate = DragDropManager.CurrentDropNotAllowedCursorTemplate;
                //                e.OperationType = OperationType.DropNotAllowed;
                //                DropNotAllowedStyle(e.DropTarget);
                //            }
                //        }
                //    }
                //    else if (dropNodeData.GetType() == typeof(EnvironmentViewModel))
                //    {
                //        var destination = dropNodeData as IEnvironmentViewModel;
                //        var source = sourceNodeData as IExplorerItemViewModel;

                //        if (destination != null && source != null)
                //        {
                //            if (!ValidateDragDrop(source, destination))
                //            {
                //                e.DropNotAllowedCursorTemplate = null;
                //                e.CopyCursorTemplate = DragDropManager.CurrentCopyCursorTemplate;
                //                e.MoveCursorTemplate = DragDropManager.CurrentMoveCursorTemplate;
                //                e.OperationType = OperationType.Move;
                //                DropAllowedStyle(e.DropTarget);

                //                ClearException(e);
                //                var checkExists = destination.Children.FirstOrDefault(o => o.ResourceName == source.ResourceName);
                //                if (checkExists != null)
                //                {
                //                    SetException(e);
                //                }
                //            }
                //            else
                //            {
                //                e.DropNotAllowedCursorTemplate = DragDropManager.CurrentDropNotAllowedCursorTemplate;
                //                e.CopyCursorTemplate = DragDropManager.CurrentDropNotAllowedCursorTemplate;
                //                e.MoveCursorTemplate = DragDropManager.CurrentDropNotAllowedCursorTemplate;
                //                e.OperationType = OperationType.DropNotAllowed;
                //                DropNotAllowedStyle(e.DropTarget);
                //            }
                //        }
                //    }
                //}
                //else
                //{
                    var dropActivity =
                        Utilities.GetAncestorFromType(e.DropTarget, typeof(ContentControl), false) as ContentControl;
                    if (dropActivity == null || dragSource == null)
                    {
                        return;
                    }
                    var sourceNodeData = dragSource.Node.Data as IExplorerItemViewModel;
                    if (sourceNodeData != null && sourceNodeData.ResourceType == "WorkflowService")
                    {
                        var dragData = new DataObject();
                        var context = dragSource.DataContext as XamDataTreeNodeDataContext;
                        if (context != null)
                        {
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
                            DragDrop.DoDragDrop(e.DragSource, dragData, DragDropEffects.Copy);
                        }
                    }
                //}
            }
        }

        void DragSourceDrop(object sender, DropEventArgs e)
        {
            // Disconnect the events for memory reasons.
            //var src = DragDropManager.GetDragSource(e.DragSource);
            //src.DragLeave -= DragSourceDragLeave;
            //src.DragOver -= DragSourceDragOver;
            //src.Drop -= DragSourceDrop;

            //if (!_exceptionThrown && !_isDropOntoNode)
            //{
            //    var dropTarget = e.DropTarget as XamDataTreeNodeControl;
            //    var dragSource = e.DragSource as XamDataTreeNodeControl;
            //    if (dropTarget != null && dragSource != null)
            //    {
            //        var dropNodeData = dropTarget.Node.Data;
            //        var sourceNodeData = dragSource.Node.Data;
            //        if (dropNodeData.GetType() == typeof(ExplorerItemViewModel))
            //        {
            //            var destination = dropNodeData as IExplorerItemViewModel;
            //            var source = sourceNodeData as IExplorerItemViewModel;
            //            if (destination != null && source != null)
            //            {
            //                IEnvironmentViewModel vmDestination = GetEnv(destination);

            //                if (!ValidateDragDrop(source, vmDestination) && destination.IsFolder)
            //                {
            //                    source.Move(destination);
            //                }
            //                else
            //                {
            //                    e.OperationType = OperationType.DropNotAllowed;
            //                }
            //            }
            //            ResetDragDropTemplate(e);
            //        }
            //        else if (dropNodeData.GetType() == typeof(EnvironmentViewModel))
            //        {
            //            var destination = dropNodeData as IEnvironmentViewModel;
            //            var source = sourceNodeData as IExplorerItemViewModel;

            //            if (destination != null && source != null)
            //            {
            //                if (!ValidateDragDrop(source, destination))
            //                {
            //                    source.Move(destination);
            //                }
            //                else
            //                {
            //                    e.OperationType = OperationType.DropNotAllowed;
            //                }
            //            }
            //            ResetDragDropTemplate(e);
            //        }
            //    }
            //}
        }

        void DragSourceDragLeave(object sender, DragDropEventArgs e)
        {
            // Reset the cursor template
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                StopDragging();
            }
            ResetDragDropTemplate(e);
        }

        private void DragSourceDragEnd(object sender, DragDropEventArgs e)
        {
            var dropTarget = e.DropTarget as XamDataTreeNodeControl;
            var dragSource = e.DragSource as XamDataTreeNodeControl;
            if (dropTarget != null && dragSource != null)
            {
                var dropNodeData = dropTarget.Node.Data;
                var sourceNodeData = dragSource.Node.Data;
                if (dropNodeData != null && dropNodeData.GetType() == typeof(ExplorerItemViewModel))
                {
                    var source = sourceNodeData as IExplorerItemViewModel;
                    if (source != null && _exceptionThrown)
                    {
                        e.OperationType = OperationType.DropNotAllowed;
                        source.ShowErrorMessage(_errorMessage, "Move not allowed");
                        _exceptionThrown = false;
                        _errorMessage = "";
                        ResetDragDropTemplate(e);
                    }
                    ExplorerTree.ScrollNodeIntoView(dragSource.Node);
                }
            }
        }

        private static void ResetDragDropTemplate(DragDropEventArgs e)
        {
            e.DropNotAllowedCursorTemplate = null;
            e.CopyCursorTemplate = null;
            e.MoveCursorTemplate = null;
            DropNotAllowedStyle(e.DropTarget);
        }

        private void ClearException(DragDropMoveEventArgs e)
        {
            e.OperationType = OperationType.Move;
            _exceptionThrown = false;
            _errorMessage = "";
        }
        private void SetException(DragDropMoveEventArgs e)
        {
            e.OperationType = OperationType.DropNotAllowed;
            _exceptionThrown = true;
            _errorMessage = "The destination folder has a resource with the same name";
        }

        private bool ValidateDragDrop(IExplorerItemViewModel source, IEnvironmentViewModel vmDestination)
        {
            CancelDrag = false;
            IEnvironmentViewModel vmSource = GetEnv(source);
            
            if (!Equals(vmSource.ResourceName, vmDestination.ResourceName))
            {
                CancelDrag = true;
            }
            if ((source.ResourceType == "ServerSource" || source.IsServer) &&
                string.IsNullOrWhiteSpace(source.ResourcePath))
            {
                CancelDrag = true;
            }
            return CancelDrag;
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ResetDragEvents();
        }

        void ResetDragEvents()
        {
            CancelDrag = false;
            Mouse.SetCursor(Cursors.Arrow);
        }

        IEnvironmentViewModel GetEnv(IExplorerTreeItem source)
        {
            var x = source;
            var env = source as IEnvironmentViewModel;
            if (env != null)
                return env;
            return GetEnv(x.Parent);
        }

        private static void DropNotAllowedStyle(UIElement element)
        {
            if (element != null)
            {
                Rectangle rect = (Rectangle)Utilities.GetDescendantFromName((XamDataTreeNodeControl)element, "DropOntoElem");
                if (rect != null)
                {
                    rect.Stroke = Application.Current.TryFindResource("TransparentBrush") as SolidColorBrush;
                    rect.Opacity = 0.0;
                }
                element.AllowDrop = false;
            }
        }

        private static void DropAllowedStyle(UIElement element)
        {
            if (element != null)
            {
                Rectangle rect = (Rectangle)Utilities.GetDescendantFromName((XamDataTreeNodeControl)element, "DropOntoElem");
                if (rect != null)
                {
                    rect.Stroke = Application.Current.TryFindResource("WareWolfButtonBrush") as SolidColorBrush;
                    rect.Opacity = 0.5;
                }
                element.AllowDrop = true;
            }
        }

        void StopDragging()
        {
            CancelDrag = true;
            try
            {
                DragDropManager.EndDrag(true);
            }
            // ReSharper disable once UnusedVariable
            catch (Exception)
            {
                //aaa
            }
        }
    }
}
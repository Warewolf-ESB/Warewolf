using System;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
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

        void ResetDragEvents()
        {
            CancelDrag = false;
            Mouse.SetCursor(Cursors.Arrow);
        }

        private void ExplorerTree_OnNodeDraggingStart(object sender, DragDropStartEventArgs e)
        {
            // Only need to perform this workaround if we're dragging license nodes.
            var xamDataTreeNodeControl = e.DragSource as XamDataTreeNodeControl;
            if (xamDataTreeNodeControl != null && xamDataTreeNodeControl.Node.Data.GetType() == typeof(ExplorerItemViewModel))
            {
                DragSource src = DragDropManager.GetDragSource(e.DragSource);
                //src.DragChannels.Add("1");
                src.DragEnter += src_DragEnter;
                src.DragLeave += src_DragLeave;
                src.Drop += src_Drop;
                src.DragStart += SrcOnDragStart;
                src.DragOver += SrcOnDragOver;
            }
        }

        private void SrcOnDragOver(object sender, DragDropMoveEventArgs e)
        {
            var xamDataTreeNodeControl = e.DropTarget as XamDataTreeNodeControl;
            if (xamDataTreeNodeControl != null)
            {
                var dataType = xamDataTreeNodeControl.Node.Data.GetType();
                if (dataType == typeof(ExplorerItemViewModel))
                {
                    Rectangle rect =
                        (Rectangle)
                            Utilities.GetDescendantFromName((XamDataTreeNodeControl) e.DropTarget, "DropOntoElem");
                    var data = xamDataTreeNodeControl.Node.Data as IExplorerItemViewModel;
                    if (data != null)
                    {
                        if (!data.CanDrop || !data.IsFolder)
                        {
                            e.DropNotAllowedCursorTemplate = DragDropManager.CurrentDropNotAllowedCursorTemplate;
                            rect.Stroke = Application.Current.TryFindResource("TransparentBrush") as SolidColorBrush;
                            rect.Opacity = 0.0;
                        }
                        else
                        {
                            e.DropNotAllowedCursorTemplate = DragDropManager.CurrentMoveCursorTemplate;
                            rect.Opacity = 0.5;
                        }
                    }
                }
                else if (dataType == typeof(EnvironmentViewModel))
                {
                    var data = xamDataTreeNodeControl.Node.Data as IExplorerItemViewModel;
                    if (data != null)
                    {
                        if (!data.CanDrag)
                            return;
                    }

                    e.DropNotAllowedCursorTemplate = DragDropManager.CurrentMoveCursorTemplate;
                    Rectangle rect =
                        (Rectangle)
                            Utilities.GetDescendantFromName((XamDataTreeNodeControl) e.DropTarget, "DropOntoElem");
                    rect.Opacity = 0.5;
                }
            }
            else
            {
                var dropActivity = Utilities.GetAncestorFromType(e.DropTarget, typeof(ContentControl), false) as ContentControl;
                var dragSource = e.DragSource as XamDataTreeNodeControl;
                if (dropActivity == null || dragSource == null)
                {
                    return;
                }
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
                    _dragData = dragData;
                    DragDrop.DoDragDrop(e.DragSource, dragData, DragDropEffects.Copy);
                }
            }
        }

        private void SrcOnDragStart(object sender, DragDropStartEventArgs e)
        {
            var xamDataTreeNodeControl = e.DropTarget as XamDataTreeNodeControl;
            if (xamDataTreeNodeControl != null)
            {
                var dataType = xamDataTreeNodeControl.Node.Data.GetType();
                if (dataType == typeof(ExplorerItemViewModel))
                {
                    Rectangle rect =
                        (Rectangle)
                            Utilities.GetDescendantFromName((XamDataTreeNodeControl) e.DropTarget, "DropOntoElem");
                    var data = xamDataTreeNodeControl.Node.Data as IExplorerItemViewModel;
                    if (data != null)
                    {
                        if (!data.CanDrag)
                        {
                            e.DropNotAllowedCursorTemplate = DragDropManager.CurrentDropNotAllowedCursorTemplate;
                            rect.Opacity = 0.0;
                            return;
                        }
                    }

                    e.DropNotAllowedCursorTemplate = DragDropManager.CurrentMoveCursorTemplate;
                    rect.Opacity = 0.5;
                }
            }
        }

        void src_DragEnter(object sender, DragDropCancelEventArgs e)
        {
            var xamDataTreeNodeControl = e.DropTarget as XamDataTreeNodeControl;
            if (xamDataTreeNodeControl != null)
            {
                var dataType = xamDataTreeNodeControl.Node.Data.GetType();
                if (dataType == typeof(ExplorerItemViewModel))
                {
                    Rectangle rect =
                        (Rectangle)
                            Utilities.GetDescendantFromName((XamDataTreeNodeControl)e.DropTarget, "DropOntoElem");
                    var data = xamDataTreeNodeControl.Node.Data as IExplorerItemViewModel;
                    if (data != null)
                    {
                        if (!data.CanDrop || !data.IsFolder)
                        {
                            e.DropNotAllowedCursorTemplate = DragDropManager.CurrentDropNotAllowedCursorTemplate;
                            rect.Stroke = Application.Current.TryFindResource("TransparentBrush") as SolidColorBrush;
                            rect.Opacity = 0.0;
                        }
                        else
                        {
                            e.DropNotAllowedCursorTemplate = DragDropManager.CurrentMoveCursorTemplate;
                            rect.Opacity = 0.5;
                        }
                    }
                }
                else if (dataType == typeof(EnvironmentViewModel))
                {
                    var data = xamDataTreeNodeControl.Node.Data as IExplorerItemViewModel;
                    if (data != null)
                    {
                        if (!data.CanDrag)
                            return;
                    }

                    e.DropNotAllowedCursorTemplate = DragDropManager.CurrentMoveCursorTemplate;
                    Rectangle rect =
                        (Rectangle)
                            Utilities.GetDescendantFromName((XamDataTreeNodeControl)e.DropTarget, "DropOntoElem");
                    rect.Opacity = 0.5;
                }
            }
        }

        void src_DragLeave(object sender, DragDropEventArgs e)
        {
            // Reset the cursor template if the node we're over is a Group node
            var xamDataTreeNodeControl = e.DropTarget as XamDataTreeNodeControl;
            if (xamDataTreeNodeControl != null)
            {
                Rectangle rect =
                        (Rectangle)
                            Utilities.GetDescendantFromName((XamDataTreeNodeControl)e.DropTarget, "DropOntoElem");
                var dataType = xamDataTreeNodeControl.Node.Data.GetType();
                if (dataType == typeof(ExplorerItemViewModel))
                {
                    e.DropNotAllowedCursorTemplate = null;
                    rect.Opacity = 0.0;
                }
                else if (dataType == typeof(EnvironmentViewModel))
                {
                    e.DropNotAllowedCursorTemplate = null;
                    rect.Opacity = 0.0;
                }
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

        void src_Drop(object sender, DropEventArgs e)
        {
            // Disconnect the events for memory reasons.
            var src = DragDropManager.GetDragSource(e.DragSource);
            src.DragEnter -= src_DragEnter;
            src.DragLeave -= src_DragLeave;
            src.Drop -= src_Drop;

            var dropTarget = e.DropTarget as XamDataTreeNodeControl;
            var dragSource = e.DragSource as XamDataTreeNodeControl;
            if (dropTarget != null && dragSource != null)
            {
                var dropNodeData = dropTarget.Node.Data;
                var sourceNodeData = dragSource.Node.Data;
                if (dropNodeData.GetType() == typeof(ExplorerItemViewModel))
                {
                    var destination = dropNodeData as IExplorerItemViewModel;
                    var source = sourceNodeData as IExplorerItemViewModel;

                    if (destination != null && destination.Children.Count >= 1 && source != null)
                    {
                        if (destination.IsFolder)
                        {
                            source.Move(destination);
                        }
                    }
                    else
                    {
                        IEnvironmentViewModel vmDestination = GetEnv(destination);

                        if (destination != null && Equals(destination.Parent, vmDestination) && source != null)
                        {
                            source.Move(destination);
                        }
                    }

                    e.DropNotAllowedCursorTemplate = null;
                    var rect = (Rectangle)Utilities.GetDescendantFromName((XamDataTreeNodeControl)e.DropTarget, "DropOntoElem");
                    rect.Stroke = Application.Current.TryFindResource("TransparentBrush") as SolidColorBrush;
                    rect.Opacity = 0.0;
                }
                else if (dropNodeData.GetType() == typeof(EnvironmentViewModel))
                {
                    var destination = dropNodeData as IEnvironmentViewModel;
                    var source = sourceNodeData as IExplorerItemViewModel;

                    if (destination != null && destination.Children.Count >= 1 && source != null)
                    {
                        source.Move(destination);
                    }

                    e.DropNotAllowedCursorTemplate = null;
                    var rect = (Rectangle)Utilities.GetDescendantFromName((XamDataTreeNodeControl)e.DropTarget, "DropOntoElem");
                    rect.Stroke = Application.Current.TryFindResource("TransparentBrush") as SolidColorBrush;
                    rect.Opacity = 0.0;
                }
            }
        }
    }
}
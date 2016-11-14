using System;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Interfaces;
using Infragistics.Controls.Menus;
using Infragistics.DragDrop;
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

        int flag = 0;
        private Point _startPoint;

        private void Tree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock)
            {
                // Store the mouse position
                _startPoint = e.GetPosition(null);
                flag = 1;
            }
        }

        private void Tree_MouseMove(object sender, MouseEventArgs e)
        {
            //if (flag == 1) //Begin Drag
            //{
            //    // Get the current mouse position
            //    Point mousePos = e.GetPosition(null);
            //    Vector diff = _startPoint - mousePos;

            //    if (e.LeftButton == MouseButtonState.Pressed &&
            //        Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
            //        Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            //    {
            //        // Get the dragged ListViewItem
            //        TreeView treeView = sender as TreeView;
            //        TreeViewItem treeViewItem = FindAncestor<TreeViewItem>((DependencyObject) e.OriginalSource);
            //        if (treeViewItem != null && treeView != null)
            //        {
            //            // Find the data behind the ListViewItem
            //            ExplorerItemViewModel contact = (ExplorerItemViewModel) treeView.SelectedItem;

            //            // Initialize the drag & drop operation
            //            DataObject dragData = new DataObject("myFormat", contact);
            //            DragDrop.DoDragDrop(treeViewItem, dragData, DragDropEffects.Move);
            //        }
            //    }
            //}

            if (e.LeftButton == MouseButtonState.Pressed && flag == 1)
            {
                var mousePos = e.GetPosition(null);
                var diff = _startPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var treeView = sender as TreeView;
                    var treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);

                    if (treeView == null || treeViewItem == null)
                        return;

                    var folderViewModel = treeView.SelectedItem as IExplorerItemViewModel;
                    if (folderViewModel == null)
                        return;

                    var dragData = new DataObject(folderViewModel);
                    DragDrop.DoDragDrop(treeViewItem, dragData, DragDropEffects.Move);
                }
            }
        }

        private void DropTree_DragEnter(object sender, DragEventArgs e)
        {
            //var explorerItemViewModel = e.Data.GetData(typeof(ExplorerItemViewModel)) as ExplorerItemViewModel;
            //if (explorerItemViewModel != null && !explorerItemViewModel.IsFolder)
            //{
            //    e.Effects = DragDropEffects.None;
            //}
            if (!e.Data.GetDataPresent("myFormat") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void DropTree_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ExplorerItemViewModel)))
            {
                var explorerItemViewModel = e.Data.GetData(typeof (ExplorerItemViewModel)) as ExplorerItemViewModel;
                var destination = FindAncestor<TreeViewItem>((DependencyObject) e.OriginalSource);

                var dropTarget = destination.DataContext as IExplorerItemViewModel;

                if (dropTarget == null || explorerItemViewModel == null || !dropTarget.IsFolder)
                    return;


                var itemViewModel = (IExplorerItemViewModel) explorerItemViewModel;

                itemViewModel.Parent = dropTarget;
                itemViewModel.Move(dropTarget);

                TreeViewItem itemNode;
                FindDropTarget((TreeView) sender, out itemNode, e);
                ExplorerTree.Items.Remove(explorerItemViewModel);
                //MyData.Insert(MyData.IndexOf(dropItem) + 1, dragItem);
                ExplorerTree.Items.Insert(ExplorerTree.Items.IndexOf(explorerItemViewModel) >= 1 ? ExplorerTree.Items.IndexOf(explorerItemViewModel) : 0, explorerItemViewModel);

                flag = 0; //Release Drag Operation

                //destination.Items.Add(e.Data);
                destination.Background = Brushes.Transparent;
            }
        }


        private void FindDropTarget(TreeView tv, out TreeViewItem pItemNode, DragEventArgs pDragEventArgs)
        {
            pItemNode = null;

            DependencyObject k = VisualTreeHelper.HitTest(tv, pDragEventArgs.GetPosition(tv)).VisualHit;

            while (k != null)
            {
                if (k is TreeViewItem)
                {
                    TreeViewItem treeNode = k as TreeViewItem;
                    if (treeNode.DataContext is ExplorerViewModel)
                    {
                        pItemNode = treeNode;
                    }
                }
                else if (k == tv)
                {
                    Console.WriteLine("Found treeview instance");
                    return;
                }

                k = VisualTreeHelper.GetParent(k);
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
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

        private void ExplorerTree_OnDragOver(object sender, DragEventArgs e)
        {
            TreeViewItem treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
            if (treeViewItem != null)
            {
                treeViewItem.Background = Brushes.OrangeRed;
            }
        }

        private void ExplorerTree_OnDragLeave(object sender, DragEventArgs e)
        {
            TreeViewItem treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
            if (treeViewItem != null)
            {
                treeViewItem.Background = Brushes.Transparent;
            }
        }

        private void ExplorerTree_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            flag = 0;
        }

        private void ExplorerTree_OnPreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myFormat"))
            {
                TreeViewItem itemNode;
                FindDropTarget((TreeView)sender, out itemNode, e);
                ExplorerItemViewModel dropItem = (itemNode != null && itemNode.IsVisible ? itemNode.DataContext as ExplorerItemViewModel : null);
                ExplorerItemViewModel dragItem = e.Data.GetData("myFormat") as ExplorerItemViewModel;
                if (dropItem != null)
                {
                    TreeView treeView = sender as TreeView;
                    Console.WriteLine("Index: " + (ExplorerTree.Items.IndexOf(dropItem) + 1).ToString());
                    ExplorerTree.Items.Remove(dragItem);
                    //MyData.Insert(MyData.IndexOf(dropItem) + 1, dragItem);
                    ExplorerTree.Items.Insert(ExplorerTree.Items.IndexOf(dropItem) >= 1 ? ExplorerTree.Items.IndexOf(dropItem) : 0, dragItem);
                }
                flag = 0;//Release Drag Operation
            }
        }
    }
}
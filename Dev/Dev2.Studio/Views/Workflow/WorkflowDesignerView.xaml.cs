#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Studio.ViewModels.Workflow;


namespace Dev2.Studio.Views.Workflow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WorkflowDesignerView : IWorkflowDesignerView
    {
        readonly DragDropHelpers _dragDropHelpers;

        public WorkflowDesignerView()
        {
            InitializeComponent();
            PreviewDrop += DropPointOnDragEnter;
            PreviewDragOver += DropPointOnDragEnter;
            PreviewMouseDown += WorkflowDesignerViewPreviewMouseDown;
            _dragDropHelpers = new DragDropHelpers(this);
            
        }

        void WorkflowDesignerViewPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof (System.Windows.Documents.Run))
            {
                return;
            }
            if (e.ChangedButton == MouseButton.Right)
            {
                var node = e.OriginalSource as DependencyObject;
                while (node != null)
                {
                    if (node is ActivityDesigner)
                    {
                        break;
                    }
                    if (node.GetType().Name.Contains("StartSymbol"))
                    {
                        RightClickStartNode(e);
                        break;
                    }
                    node = VisualTreeHelper.GetParent(node);
                }
            }
            OnPreviewMouseDown(e);
        }

        private void RightClickStartNode(MouseButtonEventArgs e)
        {
            var rect = e.OriginalSource as System.Windows.Shapes.Rectangle;
            if (e.OriginalSource is Grid grid)
            {
                grid.ContextMenu = WorkflowDesigner.Resources["StartNodeContextMenu"] as ContextMenu;
                if (grid.ContextMenu != null)
                {
                    grid.ContextMenu.IsOpen = true;
                    grid.ContextMenu.DataContext = DataContext;

                }
            }
            else
            {
                if (rect != null)
                {
                    rect.ContextMenu = WorkflowDesigner.Resources["StartNodeContextMenu"] as ContextMenu;
                    if (rect.ContextMenu != null)
                    {
                        rect.ContextMenu.IsOpen = true;
                        rect.ContextMenu.DataContext = DataContext;
                    }
                }
            }
        }

        //a return from here without settings handled to true and DragDropEffects.None implies that the item drop is allowed
        void DropPointOnDragEnter(object sender, DragEventArgs e)
        {
            var dataObject = e.Data;
            var workSurfaceServiceId = ((Core.Models.ResourceModel)
                             ((WorkflowDesignerViewModel)
                             ((FrameworkElement)e.Source)
                             .DataContext).ResourceModel).ID;
            var data = dataObject.GetData("Warewolf.Studio.ViewModels.ExplorerItemViewModel");
            var itemBeingDraggedOntoTheSurface = data as Warewolf.Studio.ViewModels.ExplorerItemViewModel;

            if (e.OriginalSource.GetType() == typeof(ScrollViewer))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
            else if (e.OriginalSource.GetType() == typeof(Border))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
            else if (e.OriginalSource.GetType() == typeof(Grid))
            {
                if (e.OriginalSource is Grid grid && grid.DataContext.GetType() == typeof(WorkflowDesignerViewModel))
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                }
            }
            else if (itemBeingDraggedOntoTheSurface != null && workSurfaceServiceId == itemBeingDraggedOntoTheSurface.ResourceId)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
            else
            {
                if (_dragDropHelpers.PreventDrop(dataObject))
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                }
            }
        }
    }
    public interface IWorkflowDesignerView
    {
        object DataContext { get; set; }
    }
}
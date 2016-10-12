/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// ReSharper disable CheckNamespace
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
            StartNodeContextMenu.Visibility = Visibility.Collapsed;

            if (e.RightButton == MouseButtonState.Pressed)
            {
                var grid = e.OriginalSource as Grid;
                var rect = e.OriginalSource as System.Windows.Shapes.Rectangle;
                if (grid != null)
                {
                    StartNodeContextMenu.Visibility = grid.DataContext.ToString() == "System.Activities.Core.Presentation.StartSymbol"
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
                else if (rect != null)
                {
                    StartNodeContextMenu.Visibility = rect.DataContext.ToString() == "System.Activities.Core.Presentation.StartSymbol"
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            }
        }
        //a return from here without settings handled to true and DragDropEffects.None implies that the item drop is allowed
        void DropPointOnDragEnter(object sender, DragEventArgs e)
        {
            var dataObject = e.Data;
            if (_dragDropHelpers.PreventDrop(dataObject))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }
    }
    public interface IWorkflowDesignerView
    {
        object DataContext { get; set; }
    }
}
#pragma warning disable
using System;
using System.Activities.Presentation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Common.Interfaces;
using Dev2.Studio.Interfaces;
using Dev2.UI;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    public partial class ServiceTestView : IView
    {
        public ServiceTestView()
        {
            InitializeComponent();
            WorkflowControl.PreviewMouseLeftButtonUp += WorkflowDesignerViewPreviewMouseUp;
            PreviewDragOver += DropPointOnDragEnter;
            PreviewDrop += DropPointOnDragEnter;
        }

        void DropPointOnDragEnter(object sender, DragEventArgs e)
        {
            if (sender != null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        void WorkflowDesignerViewPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                InvokeParentModelItem(e.OriginalSource as DependencyObject);
            }
        }

        void InvokeParentModelItem(DependencyObject node)
        {
            while (node != null)
            {
                if (node is WorkflowViewElement)
                {
                    var dt = DataContext as ServiceTestViewModel;
                    var wd = dt?.WorkflowDesignerViewModel;
                    var designer = node as WorkflowViewElement;
                    var modelItem = designer.ModelItem;
                    if (wd != null && wd.IsTestView && modelItem != null)
                    {
                        wd.ItemSelectedAction?.Invoke(modelItem);
                    }
                    break;
                }
                node = node is Visual ? VisualTreeHelper.GetParent(node) : null;
            }
        }

        void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {

            if (sender is ToggleButton control)
            {
                RefreshCommands(e);
            }
        }

        void RefreshCommands(RoutedEventArgs e)
        {
            var serviceTestViewModel = DataContext as IServiceTestViewModel;
            serviceTestViewModel?.RefreshCommands();
            e.Handled = true;
        }

        void SelectedTestCheckBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is CheckBox cb)
            {
                var item = cb.DataContext;
                TestsListbox.SelectedItem = item;
            }
        }

        void SelectedTestRunTestButton_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button btn)
            {
                var item = btn.DataContext;
                TestsListbox.SelectedItem = item;
            }
        }

        void MainGrid_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var serviceTestViewModel = DataContext as IServiceTestViewModel;
            serviceTestViewModel?.UpdateHelpDescriptor(Studio.Resources.Languages.HelpText.ServiceTestGenericHelpText);
            e.Handled = true;
        }

        void ListBoxItemGrid_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var serviceTestViewModel = DataContext as IServiceTestViewModel;
            serviceTestViewModel?.UpdateHelpDescriptor(Studio.Resources.Languages.HelpText.ServiceTestSelectedTestHelpText);
            e.Handled = true;
        }

        void AutoCompleteBox_OnTextChanged(object sender, RoutedEventArgs e)
        {
            var textBox = sender as IntellisenseTextBox;
            if (textBox != null)
            {
                RefreshCommands(e);
            }

            if (textBox == null && sender is TextBox box)
            {
                RefreshCommands(e);
            }
        }

        void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var routedEventArgs = new RoutedEventArgs(e.RoutedEvent);
            RefreshCommands(routedEventArgs);
            e.Handled = true;
        }
        
        #region DragDrop
        
        Point _lastMouseDown;
        IServiceTestStep draggedItem, _target;


        private void TreeView_After_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _lastMouseDown = e.GetPosition(StepTestDataTree);
            }

        }


        private bool CheckGridSplitter(UIElement element)
        {
            if (element is GridSplitter)
            {
                return true;
            }

            GridSplitter GridSplitter = FindParent<GridSplitter>(element);

            if (GridSplitter != null)
            {
                return true;
            }
            return false;

        }


        private void TreeView_After_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    UIElement element = e.OriginalSource as UIElement;
                    bool bGridSplitter = CheckGridSplitter(element);


                    Point currentPosition = e.GetPosition(StepTestDataTree);


                    if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                        (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                    {
                        draggedItem = (IServiceTestStep)StepTestDataTree.SelectedItem;

                        if ((draggedItem != null) && !bGridSplitter)
                        {
                            DragDropEffects finalDropEffect = DragDrop.DoDragDrop(StepTestDataTree, StepTestDataTree.SelectedValue,
                                DragDropEffects.Move);
                            //Checking target is not null and item is dragging(moving)
                            if ((finalDropEffect == DragDropEffects.Move) && (_target != null))
                            {

                                // A Move drop was accepted
                                if (draggedItem.ActivityID != _target.ActivityID)
                                {
                                    CopyItem(draggedItem, _target);
                                    _target = null;
                                    draggedItem = null;
                                }


                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        private void TreeView_After_DragOver(object sender, DragEventArgs e)
        {
            try
            {

                Point currentPosition = e.GetPosition(StepTestDataTree);


                if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                    (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                {
                    // Verify that this is a valid drop and then store the drop target
                    IServiceTestStep item = GetNearestContainer(e.OriginalSource as UIElement);
                    if (CheckDropTarget(draggedItem, item))
                    {
                        e.Effects = DragDropEffects.Move;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                    }
                }
                e.Handled = true;
            }
            catch (Exception)
            {
            }
        }
        private void TreeView_After_Drop(object sender, DragEventArgs e)
        {
            try
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;

                // Verify that this is a valid drop and then store the drop target
                IServiceTestStep TargetItem = GetNearestContainer(e.OriginalSource as UIElement);
                if (TargetItem != null && draggedItem != null)
                {
                    _target = TargetItem;
                    e.Effects = DragDropEffects.Move;

                }
            }
            catch (Exception)
            {
            }



        }

        private bool CheckDropTarget(IServiceTestStep _sourceItem, IServiceTestStep _targetItem)
        {
            //Check whether the target item is meeting your condition
            bool _isEqual = false;

            if (_sourceItem.ActivityID != _targetItem.ActivityID)
            {
                _isEqual = true;
            }

            return _isEqual;

        }


        private void CopyItem(IServiceTestStep _sourceItem, IServiceTestStep _targetItem)
        {

            //Asking user wether he want to drop the dragged TreeViewItem here or not
            if (MessageBox.Show("Would you like to drop " + _sourceItem.StepDescription + " into " + _targetItem.StepDescription + "", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    _targetItem.Children.Add(_sourceItem);
                }
                catch (Exception)
                {

                }
            }

        }


        private IServiceTestStep GetNearestContainer(UIElement element)
        {
            // Walk up the element tree to the nearest tree view item.
            TreeViewItem UIContainer = FindParent<TreeViewItem>(element);
            IServiceTestStep NVContainer = null;

            if (UIContainer != null)
            {
                NVContainer = UIContainer.DataContext as IServiceTestStep;
            }
            return NVContainer;
        }


        private static Parent FindParent<Parent>(DependencyObject child)
                where Parent : DependencyObject
        {
            DependencyObject parentObject = child;
            parentObject = VisualTreeHelper.GetParent(parentObject);

            //check if the parent matches the type we're looking for
            if (parentObject is Parent || parentObject == null)
            {
                return parentObject as Parent;
            }
            else
            {
                //use recursion to proceed with next level
                return FindParent<Parent>(parentObject);
            }
        }
        
        #endregion
    }
}

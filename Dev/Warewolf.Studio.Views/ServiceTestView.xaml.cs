﻿using System.Activities.Presentation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Studio.Interfaces;
using Dev2.UI;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ServiceTestView.xaml
    /// </summary>
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
                var node = e.OriginalSource as DependencyObject;
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
    }
}

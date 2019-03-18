#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
    }
}

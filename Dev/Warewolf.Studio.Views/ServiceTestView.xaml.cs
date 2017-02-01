using System.Activities.Presentation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Studio.Core.Interfaces;
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

        private void DropPointOnDragEnter(object sender, DragEventArgs e)
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
                DependencyObject node = e.OriginalSource as DependencyObject;
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
                    if (node is Visual)
                    {
                        node = VisualTreeHelper.GetParent(node);
                    }
                    else
                    {
                        node = null;
                    }
                }
            }
        }
        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            var control = sender as ToggleButton;

            if (control != null)
            {
                RefreshCommands(e);
            }
        }

        private void RefreshCommands(RoutedEventArgs e)
        {
            var serviceTestViewModel = DataContext as IServiceTestViewModel;
            serviceTestViewModel?.RefreshCommands();
            e.Handled = true;
        }

        private void SelectedTestCheckBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var cb = sender as CheckBox;
            if (cb != null)
            {
                var item = cb.DataContext;
                TestsListbox.SelectedItem = item;
            }
        }

        private void SelectedTestRunTestButton_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                var item = btn.DataContext;
                TestsListbox.SelectedItem = item;
            }
        }

        private void MainGrid_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var serviceTestViewModel = DataContext as IServiceTestViewModel;
            serviceTestViewModel?.UpdateHelpDescriptor(Studio.Resources.Languages.HelpText.ServiceTestGenericHelpText);
            e.Handled = true;
        }

        private void ListBoxItemGrid_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var serviceTestViewModel = DataContext as IServiceTestViewModel;
            serviceTestViewModel?.UpdateHelpDescriptor(Studio.Resources.Languages.HelpText.ServiceTestSelectedTestHelpText);
            e.Handled = true;
        }

        private void AutoCompleteBox_OnTextChanged(object sender, RoutedEventArgs e)
        {
            var textBox = sender as IntellisenseTextBox;
            if (textBox != null)
                RefreshCommands(e);
            if(textBox == null)
            {
                var box = sender as TextBox;
                if (box != null)
                {
                    RefreshCommands(e);
                }
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var routedEventArgs = new RoutedEventArgs(e.RoutedEvent);
            RefreshCommands(routedEventArgs);
            e.Handled = true;
        }
    }
}

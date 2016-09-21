using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Studio.Core.Interfaces;
using Dev2.UI;
using Microsoft.Practices.Prism.Mvvm;

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
            

        }

        private static IList<Control> GetControls(DependencyObject parent)
        {
            var result = new List<Control>();
            for (int x = 0; x < VisualTreeHelper.GetChildrenCount(parent); x++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, x);
                var instance = child as Control;

                if (null != instance)
                    result.Add(instance);

                result.AddRange(GetControls(child));
            }
            return result;
        }

        private void TxtValue_OnTextChanged(object sender, RoutedEventArgs e)
        {
            var textBox = sender as IntellisenseTextBox;
            if(textBox != null)
            {
                RefreshCommands(e);
            }
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            var textBox = sender as CheckBox;
            if (textBox != null)
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
            serviceTestViewModel?.UpdateHelpDescriptor(Studio.Resources.Languages.Core.ServiceTestGenericHelpText);
            e.Handled = true;
        }

        private void ListBoxItemGrid_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var serviceTestViewModel = DataContext as IServiceTestViewModel;
            serviceTestViewModel?.UpdateHelpDescriptor(Studio.Resources.Languages.Core.ServiceTestSelectedTestHelpText);
            e.Handled = true;
        }

        private void WorkflowControl_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //var elements = GetControls(WorkflowControl);

            //var toggleButtons = elements.Where(a => a.GetType() == typeof (System.Windows.Controls.Primitives.ToggleButton));
            //foreach (var toggleButton in toggleButtons)
            //{
            //    if (toggleButton.Name == "expandAllButton" || toggleButton.Name == "collapseAllButton")
            //    {
            //        toggleButton.Visibility = Visibility.Collapsed;
            //    }
            //}
            ////var activityToggleButtons = elements.Where(a => a.GetType() == typeof (CustomControls.ActivityDesignerToggleButton));
            ////foreach (var toggleButton in activityToggleButtons)
            ////{
            ////    toggleButton.Visibility = Visibility.Collapsed;
            ////}
            ////var dev2Grids = elements.Where(a => a.GetType() == typeof (Dev2DataGrid));
            ////foreach (var toggleButton in dev2Grids)
            ////{
            ////    toggleButton.IsEnabled = false;
            ////}

            //var multiAssigns = elements.Where(a => a.GetType() == typeof (MultiAssignDesigner));
            //foreach (var assign in multiAssigns)
            //{
            //    //assign.IsEnabled = false;
            //    //var assignVm = assign.DataContext as MultiAssignDesignerViewModel;
            //    //if (assignVm != null)
            //    //{
            //    //    assignVm.HasLargeView = false;
            //    //    assignVm.ThumbVisibility = Visibility.Collapsed;
            //    //}
            //}

            //var thumbs = elements.Where(a => a.GetType() == typeof (System.Windows.Controls.Primitives.Thumb));
            //foreach (var thumb in thumbs)
            //{
            //    thumb.Visibility = Visibility.Collapsed;
            //}
        }
    }
}

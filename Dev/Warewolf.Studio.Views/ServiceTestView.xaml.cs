using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Interfaces;
using Dev2.UI;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Grids.Primitives;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ServiceTestView.xaml
    /// </summary>
    public partial class ServiceTestView : UserControl,IView
    {
        public ServiceTestView()
        {
            InitializeComponent();
        }

        private void GridPreviewKeyDown(object sender, KeyEventArgs e)
        {
            UIElement keyboardFocus = Keyboard.FocusedElement as TextBox;
            if (e.KeyboardDevice.IsKeyDown(Key.LeftShift) && e.KeyboardDevice.IsKeyDown(Key.Tab))
            {
                keyboardFocus?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
            }
            if (e.KeyboardDevice.IsKeyDown(Key.Tab))
            {
                var vm = DataContext as IServiceTestViewModel;
                //var itemToSelect = vm?.GetNextRow(TestInputs.ActiveItem as IServiceTestModel);
                //if (itemToSelect != null)
                //{
                //    TestInputs.ActiveItem = itemToSelect;
                //    FocusOnAddition();
                //}
            }
        }

        private void TestsListbox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null)
            {
                var viewModel = listBox.DataContext as IServiceTestViewModel;

                var frameworkElement = e.OriginalSource as FrameworkElement;
                if (frameworkElement != null)
                {
                    if (viewModel != null)
                    {
                        viewModel.SelectedServiceTest = null;
                        var model = frameworkElement.DataContext as IServiceTestModel;

                        if (model != null)
                        {
                            viewModel.SelectedServiceTest = model;
                        }
                    }
                }
            }
        }
    }
}

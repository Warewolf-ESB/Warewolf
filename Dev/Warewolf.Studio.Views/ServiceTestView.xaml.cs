using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        private void TestInputs_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (TestInputs?.Rows != null && TestInputs.Rows.Count > 0)
            {
                var cellBaseCollection = TestInputs.Rows[0].Cells;
                if (cellBaseCollection != null)
                {
                    var selectedCell = (Cell)cellBaseCollection[1];
                    TestInputs.ActiveCell = selectedCell;
                }
            }
            FocusOnAddition();
        }

        private void FocusOnAddition()
        {
            try
            {
                var row = GetSelectedRow(TestInputs);
                if (row != null)
                {
                    var intelbox = FindByName("txtValue", row) as IntellisenseTextBox;
                    intelbox?.Focus();
                }
            }
            catch (Exception)
            {
                //
            }
        }

        static CellsPanel GetSelectedRow(XamGrid grid)
        {
            var row = grid.ActiveCell?.Row;
            return row?.Control;
        }

        private static FrameworkElement FindByName(string name, FrameworkElement root)
        {
            if (root != null)
            {
                var tree = new Stack<FrameworkElement>();
                tree.Push(root);
                while (tree.Count > 0)
                {
                    var current = tree.Pop();
                    if (current.Name == name)
                        return current;

                    var count = VisualTreeHelper.GetChildrenCount(current);
                    for (var supplierCounter = 0; supplierCounter < count; ++supplierCounter)
                    {
                        var child = VisualTreeHelper.GetChild(current, supplierCounter);
                        var item = child as FrameworkElement;
                        if (item != null)
                            tree.Push(item);
                    }
                }
            }
            return null;
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
    }
}

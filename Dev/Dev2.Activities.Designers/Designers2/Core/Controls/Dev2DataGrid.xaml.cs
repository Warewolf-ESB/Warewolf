/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Dev2.Interfaces;
using Dev2.UI;

namespace Dev2.Activities.Designers2.Core.Controls
{
    /// <summary>
    /// Interaction logic for Dev2DataGrid.xaml
    /// </summary>
    public partial class Dev2DataGrid
    {
        readonly Func<Visual, FrameworkElement> _getVisualChild;
        static int _skipNumber;

        public Dev2DataGrid()
            : this(GetVisualChild<IntellisenseTextBox>)
        {

        }

        public Dev2DataGrid(Func<Visual, FrameworkElement> getVisualChild)
        {
            VerifyArgument.IsNotNull("getVisualChild", getVisualChild);
            _getVisualChild = getVisualChild;

            InitializeComponent();
        }

        public DataGridColumnHeadersPresenter GetColumnHeadersPresenter()
        {
            return GetVisualChild<DataGridColumnHeadersPresenter>(this);
        }

        public bool SetFocusToInserted(DataGridRow row)
        {
            var modelItem = row.DataContext as ModelItem;
            var toFn = modelItem?.GetCurrentValue() as IDev2TOFn;
            if(toFn != null && toFn.Inserted)
            {
                return SetFocus(row);
            }
            return false;
        }

        public IInputElement GetFocusElement(int rowIndex, int inputsToSkip=0)
        {
            if(rowIndex >= 0 && rowIndex < Items.Count)
            {
                var row = GetRow(rowIndex);
                return GetFocusElement(row,inputsToSkip);
            }
            return null;
        }

        public IInputElement GetFocusElement(DataGridRow row, int inputsToSkip = 0)
        {
            return GetVisualChild(row,inputsToSkip);
        }

        bool SetFocus(Visual row)
        {
            // Wait for the UI to be fully rendered BEFORE trying to set the focus
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
            {
                var visualChild = GetVisualChild(row);
                if(visualChild != null)
                {
                    Keyboard.Focus(visualChild);
                }
            }));
            return true;
        }

        FrameworkElement GetVisualChild(Visual row, int inputsToSkip = 0)
        {
            _skipNumber = inputsToSkip;
            return row != null ? _getVisualChild(row) : null;
        }

        public DataGridRow GetRow(int rowIndex)
        {
            return (DataGridRow)ItemContainerGenerator.ContainerFromItem(Items[rowIndex]);
        }

        void OnDataGridRowLoaded(object sender, RoutedEventArgs e)
        {
            SetFocusToInserted(sender as DataGridRow);
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            var child = default(T);
            var numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < numVisuals; i++)
            {
                var v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T ?? GetVisualChild<T>(v);
                if(child != null)
                {
                    if (_skipNumber == 0)
                    {
                        break;
                    }
                    _skipNumber--;
                    child = null;
                }                
            }
            return child;
        }
    }
}

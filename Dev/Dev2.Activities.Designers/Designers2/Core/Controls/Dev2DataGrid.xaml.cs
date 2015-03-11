
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Dev2.Interfaces;
using Dev2.UI;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.Core.Controls
{
    /// <summary>
    /// Interaction logic for Dev2DataGrid.xaml
    /// </summary>
    public partial class Dev2DataGrid
    {
        readonly Func<Visual, FrameworkElement> _getVisualChild;

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

        public void RemoveFirstDuplicateBlankRow()
        {
            var blankCount = new List<int>();
            var itemList = Items.SourceCollection as ModelItemCollection;
            var modelItem = SelectedValue as ModelItem;
            if(itemList == null || modelItem == null)
            {
                return;
            }

            if(itemList.Count > 2)
            {
                foreach(dynamic item in itemList)
                {
                    var currentVal = item.GetCurrentValue();
                    if(currentVal != modelItem.GetCurrentValue())
                    {
                        if(currentVal.CanRemove())
                        {
                            blankCount.Add(item.IndexNumber);
                        }
                    }
                }
                if(blankCount.Count > 1)
                {
                    itemList.Remove(Items[blankCount[0] - 1]);
                    for(var i = blankCount[0] - 1; i < itemList.Count; i++)
                    {
                        dynamic tmp = itemList[i];
                        tmp.IndexNumber = i + 1;
                    }
                }
            }
        }

        public void RemoveRow(int indexNum)
        {
            dynamic itemList = Items.SourceCollection as ModelItemCollection;

            if(itemList == null)
            {
                return;
            }

            if(itemList.Count > 2)
            {
                itemList.RemoveAt(indexNum);
                for(var i = indexNum; i < itemList.Count; i++)
                {
                    dynamic tmp = itemList[i];
                    tmp.IndexNumber--;
                }
            }
            else
            {
                itemList.RemoveAt(indexNum);

                var newVal = DTOFactory.CreateNewDTO(itemList[0].GetCurrentValue());
                newVal.IndexNumber = indexNum + 1;
                itemList.Insert(indexNum, newVal);
            }
        }

        public void AddRow()
        {
            var canAdd = true;
            dynamic itemList = Items.SourceCollection;
            foreach(var item in itemList)
            {
                var currentVal = item.GetCurrentValue();
                if(!currentVal.CanAdd())
                {
                    canAdd = false;
                }
            }
            if(canAdd)
            {
                var newVal = DTOFactory.CreateNewDTO(itemList[0].GetCurrentValue());
                newVal.IndexNumber = itemList.Count + 1;
                itemList.Add(newVal);
            }
        }

        public void InsertRow(int index)
        {
            index++;
            dynamic itemList = Items.SourceCollection;
            var newVal = DTOFactory.CreateNewDTO(itemList[0].GetCurrentValue(), 0, true);
            foreach(dynamic item in itemList)
            {
                int i = item.IndexNumber;
                if(i >= index)
                {
                    item.IndexNumber++;
                }
            }
            newVal.IndexNumber = index;
            itemList.Insert(index - 1, newVal);
            SelectedIndex = index - 1;
        }

        public int CountRows()
        {
            return Items.SourceCollection.Cast<ModelItem>().Count();
        }

        public bool SetFocusToInserted(DataGridRow row)
        {
            var modelItem = row.DataContext as ModelItem;
            if(modelItem != null)
            {
                var toFn = modelItem.GetCurrentValue() as IDev2TOFn;
                if(toFn != null && toFn.Inserted)
                {
                    return SetFocus(row);
                }
            }
            return false;
        }

        public IInputElement GetFocusElement(int rowIndex)
        {
            if(rowIndex >= 0 && rowIndex < Items.Count)
            {
                var row = GetRow(rowIndex);
                return GetFocusElement(row);
            }
            return null;
        }

        public IInputElement GetFocusElement(DataGridRow row)
        {
            return GetVisualChild(row);
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
            //var visualChild = GetVisualChild(row);
            //if(visualChild != null)
            //{
            //    Keyboard.Focus(visualChild);
            //    return true;
            //}
            //return false;
        }

        FrameworkElement GetVisualChild(Visual row)
        {
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
            for(var i = 0; i < numVisuals; i++)
            {
                var v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T ?? GetVisualChild<T>(v);
                if(child != null)
                {
                    break;
                }
            }
            return child;
        }
    }
}

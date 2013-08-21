using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Interfaces;
using Dev2.UI;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.UI
{
    /// <summary>
    /// Interaction logic for Dev2DataGrid.xaml
    /// </summary>
    public partial class Dev2DataGrid
    {

        public Dev2DataGrid()
        {
            InitializeComponent();
        }

        public void RemoveRow()
        {
            List<int> BlankCount = new List<int>();
            ModelItemCollection itemList = Items.SourceCollection as ModelItemCollection;
            foreach (dynamic item in itemList)
            {
                var currentVal = item.GetCurrentValue();
                ModelItem modelItem = SelectedValue as ModelItem;
                if (modelItem != null && currentVal != modelItem.GetCurrentValue())
                {
                    if (currentVal.CanRemove())
                    {
                        BlankCount.Add(item.IndexNumber);
                    }
                }
            }
            if (BlankCount.Count > 1 && itemList.Count > 2)
            {
                itemList.Remove(Items[BlankCount[0] - 1]);
                for (int i = BlankCount[0] - 1; i < itemList.Count; i++)
                {
                    dynamic tmp = itemList[i];
                    tmp.IndexNumber = i + 1;
                }
            }
        }

        public void RemoveRow(int indexNum)
        {
            dynamic itemList = Items.SourceCollection as ModelItemCollection;

            if (itemList == null)
            {
                return;
            }

            if (itemList.Count > 2)
            {
                itemList.RemoveAt(indexNum);
                for (var i = indexNum; i < itemList.Count; i++)
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
            foreach (var item in itemList)
            {
                var currentVal = item.GetCurrentValue();
                if (!currentVal.CanAdd())
                {
                    canAdd = false;
                }
            }
            if (canAdd)
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
            foreach (dynamic item in itemList)
            {
                int i = item.IndexNumber;
                if (i >= index)
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

        void EventSetter_OnHandler(object sender, RoutedEventArgs e)
        {
            DataGridRow dgr = sender as DataGridRow;

            if (dgr != null)
            {
                ModelItem modelItem = dgr.DataContext as ModelItem;

                if (modelItem != null)
                {
                    IDev2TOFn toFn = modelItem.GetCurrentValue() as IDev2TOFn;
                    if (toFn != null && toFn.Inserted)
                    {
                        IntellisenseTextBox intellisenseTextBox = GetVisualChild<IntellisenseTextBox>(dgr);
                        if (intellisenseTextBox != null)
                        {
                            intellisenseTextBox.Focus();
                        }
                    }
                }
            }
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
    }
}

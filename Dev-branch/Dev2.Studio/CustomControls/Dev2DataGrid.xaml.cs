using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.UI
{
    /// <summary>
    /// Interaction logic for Dev2DataGrid.xaml
    /// </summary>
    public partial class Dev2DataGrid : DataGrid, IDisposable, INotifyPropertyChanged
    {
        DTOFactory dtoFac = new DTOFactory();
        public Dev2DataGrid()
        {
            InitializeComponent();
            CanUserDeleteRows = false;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public void RemoveRow()
        {
            List<int> BlankCount = new List<int>();
            ModelItemCollection itemList = Items.SourceCollection as ModelItemCollection;
            foreach (dynamic item in itemList)
            {
                var currentVal = item.GetCurrentValue();
                if (currentVal.CanRemove())
                {
                    BlankCount.Add(item.IndexNumber);
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
            if (itemList.Count > 2)
            {
                itemList.RemoveAt(indexNum);
                for (int i = indexNum; i < itemList.Count; i++)
                {
                    dynamic tmp = itemList[i];
                    tmp.IndexNumber--;
                }
            }
            else
            {
                itemList.RemoveAt(indexNum);

                var newVal = dtoFac.CreateNewDTO(itemList[0].GetCurrentValue());
                newVal.IndexNumber = indexNum + 1;
                itemList.Insert(indexNum, newVal);
            }
        }

        public void AddRow()
        {
            bool canAdd = true;
            dynamic itemList = Items.SourceCollection;
            foreach (dynamic item in itemList)
            {
                var currentVal = item.GetCurrentValue();
                if (!currentVal.CanAdd())
                {
                    canAdd = false;
                }
            }
            if (canAdd)
            {
                var newVal = dtoFac.CreateNewDTO(itemList[0].GetCurrentValue());
                newVal.IndexNumber = itemList.Count + 1;
                itemList.Add(newVal);
            }


        }

        public int CountRows()
        {
            return Items.SourceCollection.Cast<ModelItem>().Count();
        }
    }
}

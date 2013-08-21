using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio
{
    public class BaseActivityDesigner : ActivityDesigner
    {
        readonly DTOFactory _dtoFac = new DTOFactory();

        private const Int32 MinSize = 2;
        private const Int32 MinBlanks = 1;


        private IList<ModelItem> ItemList
        {
            get
            {
                //use dynamics to get strongly typed list of items
                return ((dynamic)ModelItem).FieldsCollection as ModelItemCollection;
            }
        }

        private IEnumerable<int> BlankIndexes
        {
            get
            {
                var blankList = (from ModelItem dto in ItemList
                                 let currentVal = dto.GetCurrentValue() as IDev2TOFn
                                 where currentVal != null
                                 where currentVal.CanRemove()
                                 select currentVal.IndexNumber).ToList();
                return blankList;
            }
        }

        public void RemoveRow()
        {
            //do nothing if smaller or equal than 2 (which is minimum size)
            if (ItemList == null || ItemList.Count() <= MinSize ||  
                 //never remove the last blank item
                BlankIndexes == null || BlankIndexes.Count() <= MinBlanks)
            {
                return;
            }

            //remove all the other blank items
            var firstIdxToRemove = BlankIndexes.First() - 1;
            ItemList.RemoveAt(firstIdxToRemove);
            for (var i = firstIdxToRemove; i < ItemList.Count; i++)
            {
                dynamic tmp = ItemList[i];
                tmp.IndexNumber = i + 1;
            }
        }

        //public void RemoveRow(int indexNum)
        //{   
        //    //do nothing if smaller or equal than 2 (which is minimum size)
        //    if (ItemList == null || ItemList.Count() <= MinSize)
        //    {
        //        return;
        //    }

        //    ItemList.RemoveAt(indexNum);

        //    for (int i = indexNum; i < ItemList.Count; i++)
        //    {
        //        var tmp = ItemList[i];
        //        tmp.IndexNumber--;
        //    }
        //    else
        //    {
        //        ItemList.RemoveAt(indexNum);

        //        var newVal = _dtoFac.CreateNewDTO(ItemList[0].GetCurrentValue());
        //        newVal.IndexNumber = indexNum + 1;
        //        itemList.Insert(indexNum, newVal);
        //    }
        //}

        //public void AddRow()
        //{
        //    bool canAdd = true;
        //    dynamic itemList = Items.SourceCollection;
        //    foreach (dynamic item in itemList)
        //    {
        //        var currentVal = item.GetCurrentValue();
        //        if (!currentVal.CanAdd())
        //        {
        //            canAdd = false;
        //        }
        //    }
        //    if (canAdd)
        //    {
        //        var newVal = _dtoFac.CreateNewDTO(itemList[0].GetCurrentValue());
        //        newVal.IndexNumber = itemList.Count + 1;
        //        itemList.Add(newVal);
        //    }
        //}

        //public void InsertRow(int index)
        //{
        //    index++;
        //    dynamic itemList = Items.SourceCollection;
        //    var newVal = _dtoFac.CreateNewDTO(itemList[0].GetCurrentValue());
        //    foreach (dynamic item in itemList)
        //    {
        //        int i = item.IndexNumber;
        //        if (i >= index)
        //        {
        //            item.IndexNumber++;
        //        }
        //    }
        //    newVal.IndexNumber = index;
        //    itemList.Insert(index - 1, newVal);
        //}
    }
}

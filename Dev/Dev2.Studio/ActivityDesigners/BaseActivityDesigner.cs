using Dev2.Interfaces;
using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Studio
{
    public class BaseActivityDesigner : ActivityDesigner
    {
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
    }
}

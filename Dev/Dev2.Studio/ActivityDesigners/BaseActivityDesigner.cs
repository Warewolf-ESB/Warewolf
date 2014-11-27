
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


//using System.Activities.Presentation;
//using System.Activities.Presentation.Model;
//using System.Collections.Generic;
//using System.Linq;
//using Dev2.Interfaces;

//namespace Dev2.Studio
//{
//    public class BaseActivityDesigner : ActivityDesigner
//    {
//        private IEnumerable<ModelItem> ItemList
//        {
//            get
//            {
//                //use dynamics to get strongly typed list of items
//                return ((dynamic)ModelItem).FieldsCollection as ModelItemCollection;
//            }
//        }

//        // ReSharper disable UnusedMember.Local
//        private IEnumerable<int> BlankIndexes
//        // ReSharper restore UnusedMember.Local
//        {
//            get
//            {
//                var blankList = (from ModelItem dto in ItemList
//                                 let currentVal = dto.GetCurrentValue() as IDev2TOFn
//                                 where currentVal != null
//                                 where currentVal.CanRemove()
//                                 select currentVal.IndexNumber).ToList();
//                return blankList;
//            }
//        }
//    }
//}

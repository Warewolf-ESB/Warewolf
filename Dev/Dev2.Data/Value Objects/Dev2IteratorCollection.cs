
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList.Contract.Value_Objects
{
    public class Dev2IteratorCollection : IDev2IteratorCollection
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        private IList<IDev2DataListEvaluateIterator> _itrCollection = new List<IDev2DataListEvaluateIterator>();
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        internal Dev2IteratorCollection() { }

        public void AddIterator(IDev2DataListEvaluateIterator itr)
        {
            _itrCollection.Add(itr);
        }



        public IBinaryDataListItem FetchNextRow(IDev2DataListEvaluateIterator itr)
        {
            IBinaryDataListItem result = null;
            IList<IBinaryDataListItem> tmp;

            int idx = _itrCollection.IndexOf(itr);
            if(idx >= 0)
            {
                if(_itrCollection[idx].HasMoreRecords())
                {
                    tmp = _itrCollection[idx].FetchNextRowData();
                    if(tmp != null)
                    {
                        if(tmp.Count == 1)
                        {
                            result = tmp[0];
                        }
                        else if(tmp.Count == 0)
                        {
                            result = Dev2BinaryDataListFactory.CreateBinaryItem(string.Empty, string.Empty);
                        }
                        else if(tmp.Count > 1)
                        {
                            throw new Exception("Invalid use, iterator collection may only return a single column per row");
                        }
                    }
                }
                else
                {
                    tmp = _itrCollection[idx].FetchNextRowData();
                    if(tmp != null)
                    {
                        result = tmp[0];
                    }
                }
            }

            return result;
        }

        public bool HasMoreData()
        {
            bool result = false;

            foreach(IDev2DataListEvaluateIterator itr in _itrCollection)
            {
                if(itr.HasMoreRecords())
                {
                    result = true;
                }
            }

            return result;
        }
    }
}

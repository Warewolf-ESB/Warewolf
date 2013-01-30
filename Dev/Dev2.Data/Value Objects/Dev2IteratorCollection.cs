using Dev2.DataList.Contract.Binary_Objects;
using System;
using System.Collections.Generic;

namespace Dev2.DataList.Contract.Value_Objects
{
    public class Dev2IteratorCollection : IDev2IteratorCollection
    {
        private IList<IDev2DataListEvaluateIterator> _itrCollection = new List<IDev2DataListEvaluateIterator>();

        internal Dev2IteratorCollection() { }

        public void AddIterator(IDev2DataListEvaluateIterator itr)
        {
            _itrCollection.Add(itr);
        }



        public IBinaryDataListItem FetchNextRow(IDev2DataListEvaluateIterator itr)
        {
            IBinaryDataListItem result = null;
            IList<IBinaryDataListItem> tmp = new List<IBinaryDataListItem>();

            int idx = _itrCollection.IndexOf(itr);
            if (idx >= 0)
            {
                if (_itrCollection[idx].HasMoreRecords())
                {
                    tmp = _itrCollection[idx].FetchNextRowData();
                    if (tmp.Count == 1)
                    {
                        result = tmp[0];
                    }
                    else if (tmp.Count == 0)
                    {
                        result = Dev2BinaryDataListFactory.CreateBinaryItem(string.Empty, string.Empty);
                    }
                    else if (tmp.Count > 1)
                    {
                        throw new Exception("Invalid use, iterator collection may only return a single column per row");
                    }
                }
                else
                {
                    tmp = _itrCollection[idx].FetchNextRowData();
                    result = tmp[0];
                }
            }

            return result;
        }

        public bool HasMoreData()
        {
            bool result = false;

            foreach (IDev2DataListEvaluateIterator itr in _itrCollection)
            {
                if (itr.HasMoreRecords())
                {
                    result = true;
                }
            }

            return result;
        }
    }
}

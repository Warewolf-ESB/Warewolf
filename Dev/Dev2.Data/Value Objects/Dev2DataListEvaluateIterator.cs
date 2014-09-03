using System;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract.Binary_Objects;

// ReSharper disable once CheckNamespace
namespace Dev2.DataList.Contract.Value_Objects
{
    public class Dev2DataListEvaluateIterator : IDev2DataListEvaluateIterator
    {

        private readonly IBinaryDataListEntry _entry;
        private int _iterIdx;
        private readonly IIndexIterator _idxItr;

        internal Dev2DataListEvaluateIterator(IBinaryDataListEntry entry)
        {
            _entry = entry;
            if (_entry.IsRecordset)
            {
                _idxItr = entry.FetchRecordsetIndexes();
            }
        }

        /// <summary>
        /// Fetches the next row data.
        /// If scalar repeat
        /// If RS append notation repeat
        /// </summary>
        /// <returns></returns>
        public IList<IBinaryDataListItem> FetchNextRowData()
        {
            IList<IBinaryDataListItem> result = new List<IBinaryDataListItem>();

            if (_entry.IsRecordset)
            {
                string error;
                if (_idxItr.HasMore())
                {
                    var idx = _idxItr.FetchNextIndex();
                    result = _entry.FetchRecordAt(idx, out error);
                }
                else
                {
                    result = _entry.FetchRecordAt(_idxItr.MaxIndex(), out error);
                }
            }
            else
            {
                try
                {
                    result = new List<IBinaryDataListItem> { _entry.FetchScalar() };
                }
                catch(Exception ex)
                {
                    Dev2Logger.Log.Error(ex);
                    // We trap because we want the result if successful else default to empty list
                }
                _iterIdx++;
            }

            return result;
        }

        public bool HasMoreRecords()
        {
            bool result = false;
            if (_entry.IsRecordset)
            {
                result = _idxItr.HasMore();
            }
            else
            {
                if (_iterIdx == 0)
                {
                    result = true;
                }
            }

            return result;
        }

        public int FetchCurrentIndex()
        {
            int result = 1;

            if (_entry.IsRecordset)
            {
                result = _iterIdx;
            }

            return result;
        }

        public string FetchRecordset()
        {
            return _entry.Namespace;
        }

        public IBinaryDataListEntry FetchEntry()
        {
            return _entry;
        }


    }
}

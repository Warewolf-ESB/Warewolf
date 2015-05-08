
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract.Binary_Objects;

// ReSharper disable once CheckNamespace
namespace Dev2.DataList.Contract.Value_Objects
{
    public class Dev2DataListEvaluateIterator : IDev2DataListEvaluateIterator
    {

        private readonly IBinaryDataListEntry _entry;
        private readonly IIndexIterator _idxItr;

        internal Dev2DataListEvaluateIterator(IBinaryDataListEntry entry)
        {
            _entry = entry;
            if (_entry.IsRecordset)
            {
                _idxItr = entry.FetchRecordsetIndexes();
            }
        }

        public bool HasMoreRecords()
        {
            bool result = false;
            if (_entry.IsRecordset)
            {
                result = _idxItr.HasMore();
            }

            return result;
        }

        public int FetchCurrentIndex()
        {
            int result = 1;


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

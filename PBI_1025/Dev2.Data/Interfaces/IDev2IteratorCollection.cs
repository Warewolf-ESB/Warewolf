using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList.Contract
{
    public interface IDev2IteratorCollection
    {

        void AddIterator(IDev2DataListEvaluateIterator itr);

        IBinaryDataListItem FetchNextRow(IDev2DataListEvaluateIterator itr);

        bool HasMoreData();
    }
}

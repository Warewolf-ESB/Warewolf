using Dev2.Common.Interfaces.DataList.Contract;

namespace Dev2.DataList.Contract
{
    public interface IDev2IteratorCollection
    {

        void AddIterator(IDev2DataListEvaluateIterator itr);

        IBinaryDataListItem FetchNextRow(IDev2DataListEvaluateIterator itr);

        bool HasMoreData();
    }
}

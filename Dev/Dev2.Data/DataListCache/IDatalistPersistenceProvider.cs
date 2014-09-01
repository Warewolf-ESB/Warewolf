using System;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.DataListCache
{
    public interface IDataListPersistenceProvider
    {
        bool WriteDataList(Guid datalistID, IBinaryDataList datalist, ErrorResultTO errors);
        IBinaryDataList ReadDatalist(Guid datalistID, ErrorResultTO errors);
        bool DeleteDataList(Guid id, bool onlyIfNotPersisted);
    }
}

using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using System;

namespace Dev2.Data.DataListCache
{
    public interface IDataListPersistenceProvider
    {
        bool WriteDataList(Guid datalistID, IBinaryDataList datalist, ErrorResultTO errors);
        IBinaryDataList ReadDatalist(Guid datalistID, ErrorResultTO errors);
        void DeleteDataList(Guid id, bool onlyIfNotPersisted);
        bool PersistChildChain(Guid id);
        void InitPersistence();
    }
}

using System.Collections.Generic;
using Dev2.Data.Interfaces;

namespace Dev2.Studio.Core.Interfaces.DataList
{
    interface IMissingDataList
    {
        IEnumerable<IDataListVerifyPart> MissingRecordsets(IList<IDataListVerifyPart> partsToVerify, bool excludeUnusedItems);

        IEnumerable<IDataListVerifyPart> MissingScalars(IEnumerable<IDataListVerifyPart> partsToVerify, bool excludeUnusedItems);
    }
}
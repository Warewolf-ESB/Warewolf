using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList.Contract
{
    /// <summary>
    /// Interface for all the recordset search operation
    /// </summary>
    public interface IFindRecsetOptions
    {
        Func<IList<string>> BuildSearchExpression(IBinaryDataList scopingObj, IRecsetSearch to);

        string HandlesType();

        Func<IList<RecordSetSearchPayload>> GenerateInputRange(IRecsetSearch to, IBinaryDataList bdl, out ErrorResultTO errors); 
    }
}

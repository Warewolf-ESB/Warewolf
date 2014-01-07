using System;
using System.Collections.Generic;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList
{
    /// <summary>
    /// Class responsible for interrogating and retrieving information about a recordset
    /// </summary>
    public static class RecordsetInterrogator
    {

        public static IList<string> FindRecords(IBinaryDataList scopingObj, IRecsetSearch searchTO, out ErrorResultTO errors)
        {
            IFindRecsetOptions searchOp = FindRecsetOptions.FindMatch(searchTO.SearchType);
            IList<string> result = new List<string>();
            errors = new ErrorResultTO();

            if(searchOp != null)
            {
                Func<IList<string>> searchFn = searchOp.BuildSearchExpression(scopingObj, searchTO);
                if(searchFn != null)
                {
                    result = searchFn.Invoke();
                }
            }

            return result;
        }
    }
}

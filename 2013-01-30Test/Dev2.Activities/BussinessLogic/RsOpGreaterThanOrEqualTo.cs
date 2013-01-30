using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "greater than or equal to symbol" recordset search option 
    /// </summary>
    public class RsOpGreaterThanOrEqualTo : AbstractRecsetSearchValidation
    {
        public RsOpGreaterThanOrEqualTo()
        {

        }

        public override Func<IList<string>> BuildSearchExpression(IBinaryDataList scopingObj, IRecsetSearch to)
        {
            // Default to a null function result
            Func<IList<string>> result = () => { return null; };

            result = () => {
                ErrorResultTO err = new ErrorResultTO();

                IList<RecordSetSearchPayload> operationRange = GenerateInputRange(to, scopingObj, out err).Invoke();
                IList<string> fnResult = new List<string>();
                int search = -1;

                if (Int32.TryParse(to.SearchCriteria, out search)) {
                    foreach (RecordSetSearchPayload p in operationRange) {
                        int tmp;

                        if (Int32.TryParse(p.Payload, out tmp) && tmp >= search) {

                            fnResult.Add(p.Index.ToString());
                        }
                    }
                }


                return fnResult.Distinct().ToList();
            };


            return result;
        }

        public override string HandlesType()
        {
            return ">=";
        }
    }
}

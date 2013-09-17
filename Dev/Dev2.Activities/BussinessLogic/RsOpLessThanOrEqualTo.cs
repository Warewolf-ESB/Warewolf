using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "less than or equal to symbol" recordset search option 
    /// </summary>
    public class RsOpLessThanOrEqualTo : AbstractRecsetSearchValidation
    {
        public RsOpLessThanOrEqualTo()
        {

        }

        // Bug 8725 - Fixed to be double rather than int
        public override Func<IList<string>> BuildSearchExpression(IBinaryDataList scopingObj, IRecsetSearch to)
        {
            // Default to a null function result
            Func<IList<string>> result = () => { return null; };

            result = () => {
                ErrorResultTO err = new ErrorResultTO();
                IList<RecordSetSearchPayload> operationRange = GenerateInputRange(to, scopingObj, out err).Invoke();
                IList<string> fnResult = new List<string>();
                double search = -1;

                if (double.TryParse(to.SearchCriteria, out search)) {
                    foreach (RecordSetSearchPayload p in operationRange) {
                        double tmp;

                        if (double.TryParse(p.Payload, out tmp) && tmp <= search)
                        {

                            fnResult.Add(p.Index.ToString());
                        }
                        else
                        {
                            if(to.RequireAllFieldsToMatch)
                            {
                                return new List<string>();
                            }
                        }
                    }

                }
                return fnResult.Distinct().ToList();
            };


            return result;
        }

        public override string HandlesType()
        {
            return "<=";
        }
    }
}

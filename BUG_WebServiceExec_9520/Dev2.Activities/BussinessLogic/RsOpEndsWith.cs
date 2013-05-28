using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "ends with" recordset search option 
    /// </summary>
    public class RsOpEndsWith : AbstractRecsetSearchValidation
    {
        public RsOpEndsWith()
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

                foreach (RecordSetSearchPayload p in operationRange) {
                    if (to.MatchCase)
                    {
                        if (p.Payload.EndsWith(to.SearchCriteria))
                        {
                            fnResult.Add(p.Index.ToString());
                        }
                    }
                    else
                    {
                        if (p.Payload.ToLower().EndsWith(to.SearchCriteria.ToLower()))
                        {
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
            return "Ends With";
        }
    }
}

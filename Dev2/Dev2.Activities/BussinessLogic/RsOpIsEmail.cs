using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.ExtMethods;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2;

namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "is email" recordset search option 
    /// </summary>
    public class RsOpIsEmail : AbstractRecsetSearchValidation
    {
        public RsOpIsEmail()
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

                    if (p.Payload.IsEmail()) {
                        fnResult.Add(p.Index.ToString());
                    }
                }

                return fnResult.Distinct().ToList();
            };


            return result;
        }

        public override string HandlesType()
        {
            return "Is Email";
        }
    }
}

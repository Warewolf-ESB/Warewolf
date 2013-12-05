using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.ExtMethods;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.BussinessLogic
{
    class RsOpIsBase64: AbstractRecsetSearchValidation
    {
        public RsOpIsBase64()
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

                    if (p.Payload.IsBase64()) {
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

                return fnResult.Distinct().ToList();
            };


            return result;
        }

        public override string HandlesType()
        {
            return "Is Base64";
        }
    }
}


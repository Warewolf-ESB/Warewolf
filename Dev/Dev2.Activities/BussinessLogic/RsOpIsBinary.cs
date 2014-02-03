using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Common.ExtMethods;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.BussinessLogic
{
    public class RsOpIsBinary : AbstractRecsetSearchValidation
    {
        public override Func<IList<string>> BuildSearchExpression(IBinaryDataList scopingObj, IRecsetSearch to)
        {
            // Default to a null function result
            // ReSharper disable RedundantAssignment
            Func<IList<string>> result = () => null;
            // ReSharper restore RedundantAssignment

            result = () =>
            {
                ErrorResultTO err;
                IList<RecordSetSearchPayload> operationRange = GenerateInputRange(to, scopingObj, out err).Invoke();
                IList<string> fnResult = new List<string>();

                foreach(RecordSetSearchPayload p in operationRange)
                {

                    if(p.Payload.IsBinary())
                    {
                        fnResult.Add(p.Index.ToString(CultureInfo.InvariantCulture));
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
            return "Is Binary";
        }
    }
}
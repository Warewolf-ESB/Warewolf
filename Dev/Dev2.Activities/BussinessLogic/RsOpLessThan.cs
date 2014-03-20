using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "less then symbol" recordset search option 
    /// </summary>
    public class RsOpLessThan : AbstractRecsetSearchValidation
    {
        // Bug 8725 - Fixed to be double rather than int
        public override Func<IList<string>> BuildSearchExpression(IBinaryDataList scopingObj, IRecsetSearch to)
        {
            // Default to a null function result
            Func<IList<string>> result = () => { return null; };

            result = () =>
            {
                ErrorResultTO err;

                IList<RecordSetSearchPayload> operationRange = GenerateInputRange(to, scopingObj, out err).Invoke();
                IList<string> fnResult = new List<string>();
                double search;

                if(double.TryParse(to.SearchCriteria, out search))
                {
                    foreach(RecordSetSearchPayload p in operationRange)
                    {
                        double tmp;

                        if(double.TryParse(p.Payload, out tmp) && tmp < search)
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

                }
                return fnResult.Distinct().ToList();
            };


            return result;
        }

        public override string HandlesType()
        {
            return "<";
        }
    }
}

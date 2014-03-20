using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "contains" recordset search option 
    /// </summary>
    public class RsOpContains : AbstractRecsetSearchValidation
    {
        public override Func<IList<string>> BuildSearchExpression(IBinaryDataList binaryDataList, IRecsetSearch to)
        {
            // Default to a null function result
            // ReSharper disable RedundantAssignment
            // ReSharper disable ConvertToLambdaExpression
            Func<IList<string>> result = () => { return null; };
            // ReSharper restore ConvertToLambdaExpression
            // ReSharper restore RedundantAssignment

            result = () =>
            {
                ErrorResultTO err;
                IList<RecordSetSearchPayload> operationRange = GenerateInputRange(to, binaryDataList, out err).Invoke();

                IList<string> fnResult = new List<string>();

                foreach(RecordSetSearchPayload p in operationRange)
                {
                    if(to.MatchCase)
                    {
                        if(p.Payload.Contains(to.SearchCriteria))
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
                    else
                    {
                        if(p.Payload.ToLower().Contains(to.SearchCriteria.ToLower()))
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
            return "Contains";
        }
    }
}

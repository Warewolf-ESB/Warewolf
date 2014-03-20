using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "╪" recordset search option 
    /// </summary>
    public class RsOpNotEqual : AbstractRecsetSearchValidation
    {

        public override Func<IList<string>> BuildSearchExpression(IBinaryDataList scopingObj, IRecsetSearch to)
        {
            // Default to a null function result
            Func<IList<string>> result = () => { return null; };

            result = () =>
            {
                ErrorResultTO err;
                IList<RecordSetSearchPayload> operationRange = GenerateInputRange(to, scopingObj, out err).Invoke();
                IList<string> fnResult = new List<string>();

                foreach(RecordSetSearchPayload p in operationRange)
                {
                    if(to.MatchCase)
                    {
                        if(!p.Payload.Equals(to.SearchCriteria, StringComparison.CurrentCulture))
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
                    else
                    {
                        if(!p.Payload.ToLower().Equals(to.SearchCriteria.ToLower(), StringComparison.CurrentCulture))
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
            return "<> (Not Equal)";
        }
    }
}

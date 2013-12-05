using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.BussinessLogic
{
    class RsOpIsBetween: AbstractRecsetSearchValidation
    {
        public RsOpIsBetween()
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
                    double[] dVal = new double[3];
                    DateTime[] dtVal = new DateTime[3];

                    int pos = 0;

                    if(!double.TryParse(p.Payload, out dVal[pos]))
                    {
                        try
                        {
                            DateTime dt = new DateTime();
                            if(DateTime.TryParse(p.Payload, out dt))
                            {
                                dtVal[pos] = dt;
                            }
                        }
                        catch(Exception ex)
                        {
                            ServerLogger.LogError(ex);
                            // Best effort ;)
                        }
                    }

                    pos++;

                    double left = 0.0;
                    double right = 0.0;

                    if(dVal.Length == 3)
                    {

                        left = dVal[0] - dVal[1];
                        right = dVal[0] - dVal[2];


                    }
                    else if(dtVal.Length == 3)
                    {
                        left = dtVal[0].Ticks - dtVal[1].Ticks;
                        right = dtVal[0].Ticks - dtVal[2].Ticks;

                    }
                    else
                    {
                        throw new InvalidDataException("IsBetween Numeric and DateTime mis-match");
                    }

                    if (left > 0 && right < 0)
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

                return fnResult.Distinct().ToList();
            };


            return result;
        }

        public override string HandlesType()
        {
            return "Is Between";
        }
    }
}


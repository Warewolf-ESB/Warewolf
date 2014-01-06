using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.BussinessLogic
{
    public class RsOpIsBetween : AbstractRecsetSearchValidation
    {
        public RsOpIsBetween()
        {

        }

        public override Func<IList<string>> BuildSearchExpression(IBinaryDataList scopingObj, IRecsetSearch to)
        {
            // Default to a null function result
            Func<IList<string>> result = () => { return null; };

            result = () =>
            {
                ErrorResultTO err = new ErrorResultTO();
                IList<RecordSetSearchPayload> operationRange = GenerateInputRange(to, scopingObj, out err).Invoke();

                DateTime fromDt = new DateTime();
                DateTime toDt = new DateTime();
                double fromNum = new double();
                double toNum = new double();
                if(DateTime.TryParse(to.From, out fromDt))
                {
                    if(!DateTime.TryParse(to.To, out toDt))
                    {
                        throw new InvalidDataException("IsBetween Numeric and DateTime mis-match");
                    }
                    return FindRecordIndexForDateTime(operationRange, to,fromDt,toDt).Distinct().ToList();
                }
                if(double.TryParse(to.From, out fromNum))
                {
                    if(!double.TryParse(to.To, out toNum))
                    {
                        throw new InvalidDataException("IsBetween Numeric and DateTime mis-match");
                    }
                    return FindRecordIndexForNumeric(operationRange, to,fromNum,toNum).Distinct().ToList();
                }
                return new List<string>();
            };


            return result;
        }

        private IList<string> FindRecordIndexForDateTime(IList<RecordSetSearchPayload> operationRange, IRecsetSearch to, DateTime fromDateTime, DateTime toDateTime)
        {
            IList<string> fnResult = new List<string>();
            foreach(RecordSetSearchPayload p in operationRange)
            {
                DateTime recDateTime = new DateTime();
                if(DateTime.TryParse(p.Payload, out recDateTime))
                {
                    if(recDateTime > fromDateTime && recDateTime < toDateTime)
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
            return fnResult;
        }

        private IList<string> FindRecordIndexForNumeric(IList<RecordSetSearchPayload> operationRange, IRecsetSearch to,double fromNum,double toNum)
        {
            IList<string> fnResult = new List<string>();
            foreach(RecordSetSearchPayload p in operationRange)
            {
                double recNum = new double();
                if(double.TryParse(p.Payload, out recNum))
                {
                    if(recNum > fromNum && recNum < toNum)
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
            return fnResult;
        }


        public override string HandlesType()
        {
            return "Is Between";
        }
    }
}


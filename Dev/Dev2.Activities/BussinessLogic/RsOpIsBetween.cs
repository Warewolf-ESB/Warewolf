using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.BussinessLogic
{
    public class RsOpIsBetween : AbstractRecsetSearchValidation
    {
        public override Func<IList<string>> BuildSearchExpression(IBinaryDataList scopingObj, IRecsetSearch to)
        {
            // Default to a null function result

            Func<IList<string>> result = () =>
                {
                    ErrorResultTO err;
                    IList<RecordSetSearchPayload> operationRange = GenerateInputRange(to, scopingObj, out err).Invoke();

                    DateTime fromDt;
                    if(DateTime.TryParse(to.From, out fromDt))
                    {
                        DateTime toDt;
                        if(!DateTime.TryParse(to.To, out toDt))
                        {
                            throw new InvalidDataException("IsBetween Numeric and DateTime mis-match");
                        }
                        return FindRecordIndexForDateTime(operationRange, to, fromDt, toDt).Distinct().ToList();
                    }
                    double fromNum;
                    if(double.TryParse(to.From, out fromNum))
                    {
                        double toNum;
                        if(!double.TryParse(to.To, out toNum))
                        {
                            throw new InvalidDataException("IsBetween Numeric and DateTime mis-match");
                        }
                        return FindRecordIndexForNumeric(operationRange, to, fromNum, toNum).Distinct().ToList();
                    }
                    return new List<string>();
                };

            return result;
        }

        private IEnumerable<string> FindRecordIndexForDateTime(IEnumerable<RecordSetSearchPayload> operationRange, IRecsetSearch to, DateTime fromDateTime, DateTime toDateTime)
        {
            IList<string> fnResult = new List<string>();
            foreach(RecordSetSearchPayload p in operationRange)
            {
                DateTime recDateTime;
                if(DateTime.TryParse(p.Payload, out recDateTime))
                {
                    if(recDateTime > fromDateTime && recDateTime < toDateTime)
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
            return fnResult;
        }

        private IEnumerable<string> FindRecordIndexForNumeric(IEnumerable<RecordSetSearchPayload> operationRange, IRecsetSearch to, double fromNum, double toNum)
        {
            IList<string> fnResult = new List<string>();
            foreach(RecordSetSearchPayload p in operationRange)
            {
                double recNum;
                if(!double.TryParse(p.Payload, out recNum))
                {
                    continue;
                }
                if(recNum > fromNum && recNum < toNum)
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
            return fnResult;
        }


        public override string HandlesType()
        {
            return "Is Between";
        }
    }
}


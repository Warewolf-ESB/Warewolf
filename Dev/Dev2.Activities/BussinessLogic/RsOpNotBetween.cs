
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Dev2.Common;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Warewolf.Storage;

namespace Dev2.BussinessLogic
{
    public class RsOpNotBetween : AbstractRecsetSearchValidation
    {
        public override Func<IList<string>> BuildSearchExpression(IList<RecordSetSearchPayload> operationRange, IRecsetSearch to)
        {
            // Default to a null function result

            Func<IList<string>> result = () =>
                {
              
                     

                    DateTime fromDt;
                    double fromNum;
                    if(DateTime.TryParse(to.From, out fromDt))
                    {
                        DateTime toDt;
                        if(!DateTime.TryParse(to.To, out toDt))
                        {
                            throw new InvalidDataException("IsBetween Numeric and DateTime mis-match");
                        }
                        return FindRecordIndexForDateTime(operationRange, to, fromDt, toDt).Distinct().ToList();
                    }
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

        private static IEnumerable<string> FindRecordIndexForDateTime(IEnumerable<RecordSetSearchPayload> operationRange, IRecsetSearch to, DateTime fromDateTime, DateTime toDateTime)
        {
            IList<string> fnResult = new List<string>();
            foreach(RecordSetSearchPayload p in operationRange)
            {
                DateTime recDateTime;
                if(DateTime.TryParse(p.Payload, out recDateTime))
                {
                    if(!(recDateTime > fromDateTime && recDateTime < toDateTime))
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

        private static IEnumerable<string> FindRecordIndexForNumeric(IEnumerable<RecordSetSearchPayload> operationRange, IRecsetSearch to, double fromNum, double toNum)
        {
            IList<string> fnResult = new List<string>();
            foreach(RecordSetSearchPayload p in operationRange)
            {
                double recNum;
                if(double.TryParse(p.Payload, out recNum))
                {
                    if(!(recNum > fromNum && recNum < toNum))
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


        public override Func<DataASTMutable.WarewolfAtom, bool> CreateFunc(IEnumerable<DataASTMutable.WarewolfAtom> values, IEnumerable<DataASTMutable.WarewolfAtom> warewolfAtoms, IEnumerable<DataASTMutable.WarewolfAtom> tovals, bool all)
        {

            return (a) =>
            {
                return !RunBetween(warewolfAtoms, tovals, a);
            };

        }


        static bool RunBetween(IEnumerable<DataASTMutable.WarewolfAtom> warewolfAtoms, IEnumerable<DataASTMutable.WarewolfAtom> tovals, DataASTMutable.WarewolfAtom a)
        {
            WarewolfListIterator iterator = new WarewolfListIterator();
            var from = new WarewolfAtomIterator(warewolfAtoms);
            var to = new WarewolfAtomIterator(tovals);
            iterator.AddVariableToIterateOn(@from);
            iterator.AddVariableToIterateOn(to);
            while (iterator.HasMoreData())
            {
                var fromval = iterator.FetchNextValue(@from);
                var toVal = iterator.FetchNextValue(to);

                DateTime fromDt;
                if (DateTime.TryParse(fromval, out fromDt))
                {
                    DateTime toDt;
                    if (!DateTime.TryParse(toVal, out toDt))
                    {
                        throw new InvalidDataException("IsBetween Numeric and DateTime mis-match");
                    }
                    DateTime recDateTime;
                    if (DateTime.TryParse(a.ToString(), out recDateTime))
                    {
                        if (recDateTime > fromDt && recDateTime < toDt)
                        {
                            return true;
                        }
                    }
                }
                double fromNum;
                if (double.TryParse(fromval, out fromNum))
                {
                    double toNum;
                    if (!double.TryParse(toVal, out toNum))
                    {
                        return false;
                    }
                    double recNum;
                    if (!double.TryParse(a.ToString(), out recNum))
                    {
                        continue;
                    }
                    if (recNum > fromNum && recNum < toNum)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public override string HandlesType()
        {
            return "Not Between";
        }
    }
}


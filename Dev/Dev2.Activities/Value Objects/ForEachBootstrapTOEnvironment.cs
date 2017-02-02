using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using Dev2.Data;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Enums;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

// ReSharper disable once CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities.Value_Objects
{
    /// <summary>
    /// Used with the ForEach Activity
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class ForEachBootstrapTO : DynamicObject
    {
        public enForEachExecutionType ExeType { get; set; }
        public int MaxExecutions { get; set; }
        public int IterationCount { get; set; }
        public ForEachInnerActivityTO InnerActivity { get; set; }
        public IIndexIterator IndexIterator { get; set; }
        public enForEachType ForEachType { get; private set; }


        //MO - Changed : new ctor that accepts the new arguments
        public ForEachBootstrapTO(enForEachType forEachType, string from, string to, string csvNumbers, string numberOfExecutes, string recordsetName, IExecutionEnvironment compiler, out ErrorResultTO errors, int update)
        {
            errors = new ErrorResultTO();
            ForEachType = forEachType;
            IIndexIterator localIndexIterator;

            switch(forEachType)
            {
                case enForEachType.InRecordset:
                    
                    var records = compiler.EvalRecordSetIndexes(recordsetName, update);
                    if (!compiler.HasRecordSet(recordsetName) )
                    {
                        errors.AddError("When selecting a recordset only valid recordsets can be used");
                        break;
                    }

                        localIndexIterator = new IndexListIndexIterator(records);

                    
                    IndexIterator = localIndexIterator;
                    break;

                case enForEachType.InRange:
                    if(string.IsNullOrWhiteSpace(@from))
                    {
                        errors.AddError(string.Format(ErrorResource.IsRequired, "The FROM field"));
                        break;
                    }

                    if(string.IsNullOrWhiteSpace(to))
                    {
                        errors.AddError(string.Format(ErrorResource.IsRequired, "The TO field"));
                        break;
                    }

                    if(@from.Contains("(*)"))
                    {
                        errors.AddError(string.Format(ErrorResource.StarNotationNotAllowed, "From field"));
                        break;
                    }

                    

                    var evalledFrom = ExecutionEnvironment.WarewolfEvalResultToString( compiler.Eval(@from, update));
                    int intFrom;
                    if (!int.TryParse(evalledFrom, out intFrom) || intFrom < 1)
                    {
                        errors.AddError(string.Format(ErrorResource.RangeFromOne, "FROM range"));
                        break;
                    }

                    if(to.Contains("(*)"))
                    {
                        errors.AddError(string.Format(ErrorResource.StarNotationNotAllowed, "TO field."));
                        break;
                    }

                    var evalledTo= ExecutionEnvironment.WarewolfEvalResultToString( compiler.Eval(@to, update));
               
                    int intTo;
                    if (!int.TryParse(evalledTo, out intTo) || intTo < 1)
                    {
                        errors.AddError(string.Format(ErrorResource.RangeFromOne, "TO range"));
                        break;
                    }
                    IndexList indexList;
                    if(intFrom > intTo)
                    {
                        indexList = new IndexList(new HashSet<int>(), 0) { MinValue = intFrom, MaxValue = intTo };
                        ReverseIndexIterator revIdxItr = new ReverseIndexIterator(new HashSet<int>(), 0) { IndexList = indexList };
                        IndexIterator = revIdxItr;
                    }
                    else
                    {
                        indexList = new IndexList(new HashSet<int>(), 0) { MinValue = intFrom, MaxValue = intTo };
                        localIndexIterator = new IndexIterator(new HashSet<int>(), 0) { IndexList = indexList };
                        IndexIterator = localIndexIterator;
                    }

                    break;
                case enForEachType.InCSV:
                    var csvIndexedsItr = ExecutionEnvironment.WarewolfEvalResultToString( compiler.Eval(csvNumbers, update));

                    ErrorResultTO allErrors;
                    List<int> listOfIndexes = SplitOutCsvIndexes(csvIndexedsItr, out allErrors);
                    if(allErrors.HasErrors())
                    {
                        errors.MergeErrors(allErrors);
                        break;
                    }
                    ListIndexIterator listLocalIndexIterator = new ListIndexIterator(listOfIndexes);
                    ListOfIndex listOfIndex = new ListOfIndex(listOfIndexes);
                    listLocalIndexIterator.IndexList = listOfIndex;
                    IndexIterator = listLocalIndexIterator;
                    break;
                default:

                    if(numberOfExecutes != null && numberOfExecutes.Contains("(*)"))
                    {
                        errors.AddError(string.Format(ErrorResource.StarNotationNotAllowed, "Numbers field."));
                        break;
                    }

                    int intExNum;
                    var numOfExItr = ExecutionEnvironment.WarewolfEvalResultToString( compiler.Eval(numberOfExecutes, update));

                    if (!int.TryParse(numOfExItr, out intExNum) || intExNum<1)
                    {
                        errors.AddError(string.Format(ErrorResource.RangeFromOne, "Number of executes"));
                    }
                    IndexIterator = new IndexIterator(new HashSet<int>(), intExNum);
                    break;
            }

        }

        List<int> SplitOutCsvIndexes(string csvNumbers, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            List<int> result = new List<int>();
            var splitStrings = csvNumbers.Split(',');
            foreach(var splitString in splitStrings)
            {
                if(!string.IsNullOrEmpty(splitString))
                {
                    int index;
                    if(int.TryParse(splitString, out index))
                    {
                        result.Add(index);
                    }
                    else
                    {
                        errors.AddError(ErrorResource.CSVInvalidCharecters);
                        return result;
                    }
                }
            }

            return result;
        }

        public void IncIterationCount()
        {
            IterationCount++;
        }
    }
}
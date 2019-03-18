#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System.Collections.Generic;
using System.Dynamic;
using Dev2.Data;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;


namespace Unlimited.Applications.BusinessDesignStudio.Activities.Value_Objects
{
    /// <summary>
    /// Used with the ForEach Activity
    /// </summary>

    public class ForEachBootstrapTO : DynamicObject
    {
        public enForEachExecutionType ExeType { get; set; }
        public int MaxExecutions { get; set; }
        public int IterationCount { get; set; }
        public ForEachInnerActivityTO InnerActivity { get; set; }
        public IIndexIterator IndexIterator { get; set; }
        public enForEachType ForEachType { get; private set; }




        //MO - Changed : new ctor that accepts the new arguments
#pragma warning disable S1541 // Methods and properties should not be too complex
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        public ForEachBootstrapTO(enForEachType forEachType, string from, string to, string csvNumbers, string numberOfExecutes, string recordsetName, IExecutionEnvironment compiler, out ErrorResultTO errors, int update)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            errors = new ErrorResultTO();
            ForEachType = forEachType;
            IIndexIterator localIndexIterator;

            switch (forEachType)
            {
                case enForEachType.InRecordset:

                    if (string.IsNullOrEmpty(recordsetName))
                    {
                        errors.AddError(string.Format(ErrorResource.IsRequired, "The Recordset Field"));
                        break;
                    }
                    var records = compiler.EvalRecordSetIndexes(recordsetName, update);
                    if (!compiler.HasRecordSet(recordsetName))
                    {
                        errors.AddError("When selecting a recordset only valid recordsets can be used");
                        break;
                    }

                    localIndexIterator = new IndexListIndexIterator(records);


                    IndexIterator = localIndexIterator;
                    break;

                case enForEachType.InRange:
                    if (string.IsNullOrWhiteSpace(@from))
                    {
                        errors.AddError(string.Format(ErrorResource.IsRequired, "The FROM field"));
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(to))
                    {
                        errors.AddError(string.Format(ErrorResource.IsRequired, "The TO field"));
                        break;
                    }

                    if (@from.Contains("(*)"))
                    {
                        errors.AddError(string.Format(ErrorResource.StarNotationNotAllowed, "From field"));
                        break;
                    }



                    var evalledFrom = ExecutionEnvironment.WarewolfEvalResultToString(compiler.Eval(@from, update));
                    int intFrom;
                    if (!int.TryParse(evalledFrom, out intFrom) || intFrom < 1)
                    {
                        errors.AddError(string.Format(ErrorResource.RangeFromOne, "FROM range"));
                        break;
                    }

                    if (to.Contains("(*)"))
                    {
                        errors.AddError(string.Format(ErrorResource.StarNotationNotAllowed, "TO field."));
                        break;
                    }

                    var evalledTo = ExecutionEnvironment.WarewolfEvalResultToString(compiler.Eval(@to, update));

                    int intTo;
                    if (!int.TryParse(evalledTo, out intTo) || intTo < 1)
                    {
                        errors.AddError(string.Format(ErrorResource.RangeFromOne, "TO range"));
                        break;
                    }
                    IndexList indexList;
                    if (intFrom > intTo)
                    {
                        indexList = new IndexList(new HashSet<int>(), 0) { MinValue = intFrom, MaxValue = intTo };
                        var revIdxItr = new ReverseIndexIterator(new HashSet<int>(), 0) { IndexList = indexList };
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
                    if (string.IsNullOrEmpty(csvNumbers))
                    {
                        errors.AddError(string.Format(ErrorResource.IsRequired, "The CSV Field"));
                        break;
                    }
                    var csvIndexedsItr = ExecutionEnvironment.WarewolfEvalResultToString(compiler.Eval(csvNumbers, update));

                    ErrorResultTO allErrors;
                    var listOfIndexes = SplitOutCsvIndexes(csvIndexedsItr, out allErrors);
                    if (allErrors.HasErrors())
                    {
                        errors.MergeErrors(allErrors);
                        break;
                    }
                    var listLocalIndexIterator = new ListIndexIterator(listOfIndexes);
                    var listOfIndex = new ListOfIndex(listOfIndexes);
                    listLocalIndexIterator.IndexList = listOfIndex;
                    IndexIterator = listLocalIndexIterator;
                    break;
                default:

                    if (numberOfExecutes != null && numberOfExecutes.Contains("(*)"))
                    {
                        errors.AddError(string.Format(ErrorResource.StarNotationNotAllowed, "Numbers field."));
                        break;
                    }

                    int intExNum;
                    var numOfExItr = ExecutionEnvironment.WarewolfEvalResultToString(compiler.Eval(numberOfExecutes, update));

                    if (!int.TryParse(numOfExItr, out intExNum) || intExNum < 1)
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
            var result = new List<int>();
            var splitStrings = csvNumbers.Split(',');
            foreach(var splitString in splitStrings)
            {
                if(!string.IsNullOrEmpty(splitString))
                {
                    if (int.TryParse(splitString, out int index))
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
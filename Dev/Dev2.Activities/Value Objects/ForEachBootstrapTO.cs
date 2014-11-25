
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
using System.Dynamic;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Value_Objects;

namespace Unlimited.Applications.BusinessDesignStudio.Activities.Value_Objects
{
    /// <summary>
    /// Used with the ForEach Activity
    /// </summary>
    public class ForEachBootstrapTO : DynamicObject
    {
        public enForEachExecutionType ExeType { get; private set; }
        public int MaxExecutions { get; private set; }
        public int IterationCount { get; private set; }
        public IDev2DataListEvaluateIterator DataIterator { get; private set; }
        public ForEachInnerActivityTO InnerActivity { get; set; }
        public IIndexIterator IndexIterator { get; set; }
        public enForEachType ForEachType { get; private set; }


        public ForEachBootstrapTO(enForEachExecutionType typeOf, int maxExe, IBinaryDataListEntry data)
        {
            ExeType = typeOf;
            MaxExecutions = maxExe;
            if(data != null)
            {
                DataIterator = Dev2ValueObjectFactory.CreateEvaluateIterator(data);
            }
            else
            {
                DataIterator = null;
            }
        }

        //MO - Changed : new ctor that accepts the new arguments
        public ForEachBootstrapTO(enForEachType forEachType, string from, string to, string csvNumbers, string numberOfExecutes, string recordsetName, Guid dlID, IDataListCompiler compiler, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            ForEachType = forEachType;
            IDev2IteratorCollection colItr = Dev2ValueObjectFactory.CreateIteratorCollection();
            IndexIterator localIndexIterator;
            IndexList indexList;

            switch(forEachType)
            {
                case enForEachType.InRecordset:
                    IBinaryDataListEntry recordset = compiler.Evaluate(dlID, enActionType.User, recordsetName, false, out errors);

                    if(recordset == null || !recordset.IsRecordset)
                    {
                        errors.AddError("When selecting a recordset only valid recordsets can be used");
                        break;
                    }


                    var isEmpty = recordset.IsEmpty();
                    if(isEmpty)
                    {
                        indexList = new IndexList(new HashSet<int> { 1 }, 0);
                        localIndexIterator = new IndexIterator(new HashSet<int> { 1 }, 0);
                    }
                    else
                    {
                        indexList = new IndexList(new HashSet<int>(), 0) { MinValue = 1, MaxValue = recordset.FetchLastRecordsetIndex() };
                        localIndexIterator = new IndexIterator(new HashSet<int>(), 0);
                    }

                    localIndexIterator.IndexList = indexList;
                    IndexIterator = localIndexIterator;
                    break;

                case enForEachType.InRange:
                    if(string.IsNullOrWhiteSpace(from))
                    {
                        errors.AddError("The from field can not be left empty.");
                        break;
                    }

                    if(string.IsNullOrWhiteSpace(to))
                    {
                        errors.AddError("The to field can not be left empty.");
                        break;
                    }

                    if(from.Contains("(*)"))
                    {
                        errors.AddError("The Star notation is not accepted in the From field.");
                        break;
                    }

                    var fromItr = CreateDataListEvaluateIterator(from, dlID, compiler, colItr, errors);
                    colItr.AddIterator(fromItr);

                    int intFrom;
                    if(!int.TryParse(colItr.FetchNextRow(fromItr).TheValue, out intFrom) || intFrom < 1)
                    {
                        errors.AddError("From range must be a whole number from 1 onwards.");
                        break;
                    }

                    if(to.Contains("(*)"))
                    {
                        errors.AddError("The Star notation is not accepted in the To field.");
                        break;
                    }

                    var toItr = CreateDataListEvaluateIterator(to, dlID, compiler, colItr, errors);
                    colItr.AddIterator(toItr);

                    int intTo;
                    if(!int.TryParse(colItr.FetchNextRow(toItr).TheValue, out intTo) || intTo < 1)
                    {
                        errors.AddError("To range must be a whole number from 1 onwards.");
                        break;
                    }
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
                    var csvIndexedsItr = CreateDataListEvaluateIterator(csvNumbers, dlID, compiler, colItr, errors);
                    colItr.AddIterator(csvIndexedsItr);
                    ErrorResultTO allErrors;
                    List<int> listOfIndexes = SplitOutCsvIndexes(colItr.FetchNextRow(csvIndexedsItr).TheValue, out allErrors);
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
                        errors.AddError("The Star notation is not accepted in the Numbers field.");
                        break;
                    }

                    int intExNum;
                    var numOfExItr = CreateDataListEvaluateIterator(numberOfExecutes, dlID, compiler, colItr, errors);
                    colItr.AddIterator(numOfExItr);

                    if(!int.TryParse(colItr.FetchNextRow(numOfExItr).TheValue, out intExNum))
                    {
                        errors.AddError("Number of executes must be a whole number from 1 onwards.");
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
                        errors.AddError("Invalid characters have been entered in the CSV Numbers");
                        return result;
                    }
                }
            }

            return result;
        }

        public void IncIterationCount()
        {
            IterationCount++;
            if(DataIterator != null)
            {
                DataIterator.FetchNextRowData(); // TODO : Replace this with another method?!
            }
        }

        public bool HasMoreData()
        {

            bool result = (IterationCount < MaxExecutions);

            if(ExeType == enForEachExecutionType.GhostService)
            {
                if(DataIterator != null && result)
                {
                    // check that there is still data to iterate across ;)
                    result = DataIterator.HasMoreRecords();
                }
            }

            return result;
        }

        IDev2DataListEvaluateIterator CreateDataListEvaluateIterator(string expression, Guid executionId, IDataListCompiler compiler, IDev2IteratorCollection iteratorCollection, ErrorResultTO allErrors)
        {
            ErrorResultTO errors;

            IBinaryDataListEntry expressionEntry = compiler.Evaluate(executionId, enActionType.User, expression, false, out errors);
            allErrors.MergeErrors(errors);
            IDev2DataListEvaluateIterator expressionIterator = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionEntry);
            iteratorCollection.AddIterator(expressionIterator);

            return expressionIterator;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
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


        public ForEachBootstrapTO(enForEachExecutionType typeOf, int maxExe, IBinaryDataListEntry data)
        {
            ExeType = typeOf;
            MaxExecutions = maxExe;
            if (data != null)
            {
                DataIterator = Dev2ValueObjectFactory.CreateEvaluateIterator(data);
            }
            else
            {
                DataIterator = null;
            }
        }

        public void IncIterationCount()
        {
            IterationCount++;
            if (DataIterator != null)
            {
                DataIterator.FetchNextRowData(); // TODO : Replace this with another method?!
            }
        }

        public bool HasMoreData()
        {

            bool result = (IterationCount < MaxExecutions);

            if (ExeType == enForEachExecutionType.GhostService)
            {
                if (DataIterator != null && result)
                {
                    // check that there is still data to iterate across ;)
                    result = DataIterator.HasMoreRecords();
                }
            }

            return result;
        }

    }
}

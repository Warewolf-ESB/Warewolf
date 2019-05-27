#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Runtime.ESB.Control
{
    public class EnvironmentOutputMappingManager : IEnvironmentOutputMappingManager
    {
        readonly IDataListFactory _dataListFactory;
        public EnvironmentOutputMappingManager()
            : this(DataListFactory.Instance)
        {
        }
        public EnvironmentOutputMappingManager(IDataListFactory dataListFactory)
        {
            _dataListFactory = dataListFactory;
        }

        public IExecutionEnvironment UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(IDSFDataObject dataObject, string outputDefs, int update, bool handleErrors, ErrorResultTO errors)
        {
            var innerEnvironment = dataObject.Environment;
            dataObject.PopEnvironment();
            OutputsToEnvironment(innerEnvironment, dataObject.Environment, outputDefs, update);
            if (innerEnvironment.HasErrors() && !handleErrors)
            {
                CopyErrors(dataObject, errors, innerEnvironment);
            }
            if (innerEnvironment.HasErrors() && handleErrors)
            {
                CopyErrorsAndHandleThem(dataObject, errors, innerEnvironment);
            }
            return innerEnvironment;
        }

        private static void CopyErrors(IDSFDataObject dataObject, ErrorResultTO errors, IExecutionEnvironment innerEnvironment)
        {
            foreach (var error in innerEnvironment.AllErrors)
            {
                if (!dataObject.Environment.AllErrors.Contains(error))
                {
                    dataObject.Environment.AllErrors.Add(error);
                    errors.AddError(error);
                }
            }
            foreach (var error in innerEnvironment.Errors)
            {
                if (!dataObject.Environment.AllErrors.Contains(error))
                {
                    dataObject.Environment.AllErrors.Add(error);
                    errors.AddError(error);
                }
            }
        }

        private static void CopyErrorsAndHandleThem(IDSFDataObject dataObject, ErrorResultTO errors, IExecutionEnvironment innerEnvironment)
        {
            foreach (var error in innerEnvironment.AllErrors)
            {
                if (!dataObject.Environment.AllErrors.Contains(error))
                {

                    errors.AddError(error);
                }
            }
            foreach (var error in innerEnvironment.Errors)
            {
                if (!dataObject.Environment.AllErrors.Contains(error))
                {

                    errors.AddError(error);
                }
            }
        }

        void OutputsToEnvironment(IExecutionEnvironment innerEnvironment, IExecutionEnvironment environment, string outputDefs, int update)
        {
            try
            {
                var outputs = _dataListFactory.CreateOutputParser().Parse(outputDefs);
                var outputRecSets = _dataListFactory.CreateRecordSetCollection(outputs, true);
                var outputScalarList = _dataListFactory.CreateScalarList(outputs, true);
                var outputComplexObjectList = _dataListFactory.CreateObjectList(outputs);
                TryEvalAssignRecordSets(innerEnvironment, environment, update, outputRecSets, outputs);
                TryEvalAssignScalars(innerEnvironment, environment, update, outputScalarList);
                TryEvalAssignComplexObjects(innerEnvironment, environment, outputComplexObjectList);
            }
            finally
            {
                environment.CommitAssign();
            }

        }

        static void TryEvalAssignComplexObjects(IExecutionEnvironment innerEnvironment, IExecutionEnvironment environment, IEnumerable<IDev2Definition> outputComplexObjectList)
        {
            foreach (var dev2Definition in outputComplexObjectList)
            {
                if (dev2Definition.IsObject)
                {
                    EvalAssignComplexObjects(innerEnvironment, environment, dev2Definition);
                }
            }
        }

        static void TryEvalAssignScalars(IExecutionEnvironment innerEnvironment, IExecutionEnvironment environment, int update, IEnumerable<IDev2Definition> outputScalarList)
        {
            foreach (var dev2Definition in outputScalarList)
            {
                if (!dev2Definition.IsRecordSet && !dev2Definition.IsObject)
                {
                    EvalAssignScalars(innerEnvironment, environment, update, dev2Definition);
                }
            }
        }

        static void TryEvalAssignRecordSets(IExecutionEnvironment innerEnvironment, IExecutionEnvironment environment, int update, IRecordSetCollection outputRecSets, IList<IDev2Definition> outputs)
        {
            foreach (var recordSetDefinition in outputRecSets.RecordSets)
            {
                var outPutRecSet = outputs.FirstOrDefault(definition => definition.IsRecordSet && definition.RecordSetName == recordSetDefinition.SetName);
                if (outPutRecSet != null)
                {
                    foreach (var outputColumnDefinitions in recordSetDefinition.Columns)
                    {
                        EvalAssignRecordSets(innerEnvironment, environment, update, outPutRecSet, outputColumnDefinitions);
                    }
                }
            }
        }

        static void EvalAssignComplexObjects(IExecutionEnvironment innerEnvironment, IExecutionEnvironment environment, IDev2Definition dev2Definition)
        {
            var warewolfEvalResult = innerEnvironment.EvalJContainer(DataListUtil.AddBracketsToValueIfNotExist(dev2Definition.Name));
            if (warewolfEvalResult != null)
            {
                environment.AddToJsonObjects(DataListUtil.AddBracketsToValueIfNotExist(dev2Definition.Value), warewolfEvalResult);
            }
        }

        static void EvalAssignScalars(IExecutionEnvironment innerEnvironment, IExecutionEnvironment environment, int update, IDev2Definition dev2Definition)
        {
            var warewolfEvalResult = innerEnvironment.Eval(DataListUtil.AddBracketsToValueIfNotExist(dev2Definition.Name), update);
            if (warewolfEvalResult.IsWarewolfAtomListresult)
            {
                if (warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult data && data.Item.Any())
                {
                    environment.Assign("[[" + dev2Definition.Value + "]]", ExecutionEnvironment.WarewolfAtomToString(data.Item.Last()), update);
                }
            }
            else
            {
                if (warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult data)
                {
                    environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(dev2Definition.Value), ExecutionEnvironment.WarewolfAtomToString(data.Item), update);
                }
            }
        }

        static void EvalAssignRecordSets(IExecutionEnvironment innerEnvironment, IExecutionEnvironment environment, int update, IDev2Definition outPutRecSet, IDev2Definition outputColumnDefinitions)
        {
            var correctRecSet = "[[" + outputColumnDefinitions.RecordSetName + "(*)." + outputColumnDefinitions.Name + "]]";
            var warewolfEvalResult = innerEnvironment.Eval(correctRecSet, 0);
            if (warewolfEvalResult.IsWarewolfAtomListresult)
            {
                var recsetResult = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                if (outPutRecSet.IsRecordSet)
                {
                    var enRecordsetIndexType = DataListUtil.GetRecordsetIndexType(outputColumnDefinitions.RawValue);
                    if (enRecordsetIndexType == enRecordsetIndexType.Star && recsetResult != null)
                    {
                        environment.EvalAssignFromNestedStar(outputColumnDefinitions.RawValue, recsetResult, update);
                    }

                    if (enRecordsetIndexType == enRecordsetIndexType.Blank && recsetResult != null)
                    {
                        environment.EvalAssignFromNestedLast(outputColumnDefinitions.RawValue, recsetResult, 0);
                    }

                    if (enRecordsetIndexType == enRecordsetIndexType.Numeric && recsetResult != null)
                    {
                        environment.EvalAssignFromNestedNumeric(outputColumnDefinitions.RawValue, recsetResult, 0);
                    }
                }
            }
        }
    }
}
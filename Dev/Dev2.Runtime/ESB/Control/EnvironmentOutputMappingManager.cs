using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Warewolf.Storage;

namespace Dev2.Runtime.ESB.Control
{
    public class EnvironmentOutputMappingManager : IEnvironmentOutputMappingManager
    {

        public IExecutionEnvironment UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(IDSFDataObject dataObject, string outputDefs, int update, bool handleErrors, ErrorResultTO errors)
        {
            var innerEnvironment = dataObject.Environment;
            dataObject.PopEnvironment();
            OutputsToEnvironment(innerEnvironment, dataObject.Environment, outputDefs, update);
            if (innerEnvironment.HasErrors() && !handleErrors)
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
            if (innerEnvironment.HasErrors() && handleErrors)
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
            return innerEnvironment;
        }

        private static void OutputsToEnvironment(IExecutionEnvironment innerEnvironment, IExecutionEnvironment environment, string outputDefs, int update)
        {
            try
            {
                var outputs = DataListFactory.CreateOutputParser().Parse(outputDefs);
                var outputRecSets = DataListFactory.CreateRecordSetCollection(outputs, true);
                var outputScalarList = DataListFactory.CreateScalarList(outputs, true);
                var outputComplexObjectList = DataListFactory.CreateObjectList(outputs);
                EvalAssignRecordSets(innerEnvironment, environment, update, outputRecSets, outputs);
                EvalAssignScalars(innerEnvironment, environment, update, outputScalarList);
                EvalAssignComplexObjects(innerEnvironment, environment, outputComplexObjectList);
            }
            finally
            {
                environment.CommitAssign();
            }

        }

        private static void EvalAssignComplexObjects(IExecutionEnvironment innerEnvironment, IExecutionEnvironment environment, IEnumerable<IDev2Definition> outputComplexObjectList)
        {
            foreach (var dev2Definition in outputComplexObjectList)
            {
                if (dev2Definition.IsObject)
                {
                    var warewolfEvalResult = innerEnvironment.EvalJContainer(DataListUtil.AddBracketsToValueIfNotExist(dev2Definition.Name));
                    if (warewolfEvalResult != null)
                    {
                        environment.AddToJsonObjects(DataListUtil.AddBracketsToValueIfNotExist(dev2Definition.Value), warewolfEvalResult);
                    }
                }
            }
        }

        private static void EvalAssignScalars(IExecutionEnvironment innerEnvironment, IExecutionEnvironment environment, int update, IEnumerable<IDev2Definition> outputScalarList)
        {
            foreach (var dev2Definition in outputScalarList)
            {
                if (!dev2Definition.IsRecordSet && !dev2Definition.IsObject)
                {
                    var warewolfEvalResult = innerEnvironment.Eval(DataListUtil.AddBracketsToValueIfNotExist(dev2Definition.Name), update);
                    if (warewolfEvalResult.IsWarewolfAtomListresult)
                    {
                        var data = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                        if (data != null && data.Item.Any())
                        {
                            environment.Assign("[[" + dev2Definition.Value + "]]", ExecutionEnvironment.WarewolfAtomToString(data.Item.Last()), update);
                        }
                    }
                    else
                    {
                        var data = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                        if (data != null)
                        {
                            environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(dev2Definition.Value), ExecutionEnvironment.WarewolfAtomToString(data.Item), update);
                        }
                    }
                }
            }
        }

        private static void EvalAssignRecordSets(IExecutionEnvironment innerEnvironment, IExecutionEnvironment environment, int update, IRecordSetCollection outputRecSets, IList<IDev2Definition> outputs)
        {
            foreach (var recordSetDefinition in outputRecSets.RecordSets)
            {
                var outPutRecSet = outputs.FirstOrDefault(definition => definition.IsRecordSet && definition.RecordSetName == recordSetDefinition.SetName);
                if (outPutRecSet != null)
                {
                    foreach (var outputColumnDefinitions in recordSetDefinition.Columns)
                    {
                        var correctRecSet = "[[" + outputColumnDefinitions.RecordSetName + "(*)." + outputColumnDefinitions.Name + "]]";
                        var warewolfEvalResult = innerEnvironment.Eval(correctRecSet, 0);
                        if (warewolfEvalResult.IsWarewolfAtomListresult)
                        {
                            var recsetResult = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                            if (outPutRecSet.IsRecordSet)
                            {
                                var enRecordsetIndexType = DataListUtil.GetRecordsetIndexType(outputColumnDefinitions.RawValue);
                                if (enRecordsetIndexType == enRecordsetIndexType.Star)
                                {
                                    if (recsetResult != null)
                                    {
                                        environment.EvalAssignFromNestedStar(outputColumnDefinitions.RawValue, recsetResult, update);
                                    }
                                }
                                if (enRecordsetIndexType == enRecordsetIndexType.Blank)
                                {
                                    if (recsetResult != null)
                                    {
                                        environment.EvalAssignFromNestedLast(outputColumnDefinitions.RawValue, recsetResult, 0);
                                    }
                                }
                                if (enRecordsetIndexType == enRecordsetIndexType.Numeric)
                                {
                                    if (recsetResult != null)
                                    {
                                        environment.EvalAssignFromNestedNumeric(outputColumnDefinitions.RawValue, recsetResult, 0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
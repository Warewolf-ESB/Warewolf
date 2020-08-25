/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


#pragma warning disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Comparer;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Interfaces;
using Dev2.Runtime;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;
using Newtonsoft.Json.Linq;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Resource.Messages;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using static DataStorage;

namespace Dev2.Activities
{
    public interface IDsfEnhancedDotNetDllActivity
    {
        IPluginConstructor Constructor { get; set; }
        List<IServiceInput> ConstructorInputs { get; set; }
        List<IPluginAction> MethodsToRun { get; set; }
        INamespaceItem Namespace { get; set; }
        string ObjectName { get; }
        string UniqueID { get; }
        string DisplayName { get; }

        enFindMissingType GetFindMissingType();
        int GetHashCode();
    }

    [ToolDescriptorInfo("DotNetDll", "DotNet DLL", ToolType.Native, "6AEB1038-6332-46F9-8BDD-641DE4EA038D", "Dev2.Activities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Resources_Dot_net_DLL")]
    public class DsfEnhancedDotNetDllActivityNew : DsfMethodBasedActivity, IEquatable<DsfEnhancedDotNetDllActivityNew>, IEnhancedPlugin, IDsfEnhancedDotNetDllActivity
    {
        List<IDebugState> _childStatesToDispatch;

        public INamespaceItem Namespace { get; set; }
        public IPluginConstructor Constructor { get; set; }
        public List<IPluginAction> MethodsToRun { get; set; }
        public List<IServiceInput> ConstructorInputs { get; set; }
        public DsfEnhancedDotNetDllActivityNew()
        {
            Type = "DotNet DLL Connector";
            DisplayName = "DotNet DLL";
            MethodsToRun = new List<IPluginAction>();
            ConstructorInputs = new List<IServiceInput>();
            Outputs = new List<IServiceOutputMapping>();
            ObjectName = string.Empty;
            ObjectResult = string.Empty;
        }


        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            tmpErrors = new ErrorResultTO();
            if (Namespace == null)
            {
                tmpErrors.AddError(ErrorResource.NoNamespaceSelected);
                return;
            }

            if (Constructor == null)
            {
                Constructor = new PluginConstructor();
            }

            ExecuteService(update, out tmpErrors, Constructor, Namespace, dataObject);
        }

        void ExecuteService(int update, out ErrorResultTO errors, IPluginConstructor constructor, INamespaceItem namespaceItem, IDSFDataObject dataObject)
        {
            _childStatesToDispatch = new List<IDebugState>();
            errors = new ErrorResultTO();
            PluginExecutionDto pluginExecutionDto;
            if (Constructor.IsExistingObject)
            {
                var existingObj = DataListUtil.AddBracketsToValueIfNotExist(Constructor.ConstructorName);
                var warewolfEvalResult = dataObject.Environment.EvalForJson(existingObj);
                var existingObject = ExecutionEnvironment.WarewolfEvalResultToString(warewolfEvalResult);
                pluginExecutionDto = new PluginExecutionDto(existingObject);
                ObjectResult = pluginExecutionDto.ObjectString;
            }
            else
            {
                pluginExecutionDto = new PluginExecutionDto(string.Empty);

                constructor.Inputs = new List<IConstructorParameter>();
                foreach (var parameter in ConstructorInputs)
                {
                    var resultToString = GetEvaluatedResult(dataObject, parameter.Value, parameter.EmptyIsNull, update);
                    constructor.Inputs.Add(new ConstructorParameter
                    {
                        TypeName = parameter.TypeName,
                        Name = parameter.Name,
                        Value = resultToString,
                        EmptyToNull = parameter.EmptyIsNull,
                        IsRequired = parameter.RequiredField
                    });
                }
            }

            var args = BuidlPluginInvokeArgs(update, constructor, namespaceItem, dataObject);
            pluginExecutionDto.Args = args;
            try
            {
                TryExecuteService(update, dataObject, pluginExecutionDto, args);
            }
            catch (Exception e)
            {
                errors.AddError(e.Message.Contains("Cannot convert given JSON to target type") ? ErrorResource.JSONIncompatibleConversionError + Environment.NewLine + e.Message : e.Message);
            }
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        void TryExecuteService(int update, IDSFDataObject dataObject, PluginExecutionDto pluginExecutionDto, PluginInvokeArgs args)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            using (var appDomain = PluginServiceExecutionFactory.CreateAppDomain())
            {
                if (dataObject.IsServiceTestExecution)
                {
                    var serviceTestStep = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children)?.FirstOrDefault(step => step.ActivityID == pluginExecutionDto.Args.PluginConstructor.ID);
                    if (serviceTestStep != null && serviceTestStep.Type == StepType.Mock)
                    {
                        var start = DateTime.Now;
                        MockConstructorExecution(dataObject, serviceTestStep, ref pluginExecutionDto);
                        DebugStateForConstructorInputsOutputs(dataObject, update, true, start);
                    }
                    else
                    {
                        var start = DateTime.Now;
                        RegularConstructorExecution(dataObject, appDomain, ref pluginExecutionDto);
                        DebugStateForConstructorInputsOutputs(dataObject, update, false, start);

                    }
                }
                else
                {
                    var start = DateTime.Now;
                    RegularConstructorExecution(dataObject, appDomain, ref pluginExecutionDto);
                    DebugStateForConstructorInputsOutputs(dataObject, update, false, start);
                }

                var index = 0;
                foreach (var dev2MethodInfo in args.MethodsToRun)
                {
                    if (dataObject.IsServiceTestExecution)
                    {
                        var serviceTestStep = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children)?.FirstOrDefault(step => step.ActivityID == dev2MethodInfo.ID);
                        MethodExecution(update, dataObject, pluginExecutionDto, appDomain, index, dev2MethodInfo, serviceTestStep);
                    }
                    else
                    {
                        RegularMethodExecution(appDomain, pluginExecutionDto, dev2MethodInfo, index, update, dataObject);
                    }
                    if (dev2MethodInfo.HasError)
                    {
                        break;
                    }

                    index++;
                }
            }
        }

        private void MethodExecution(int update, IDSFDataObject dataObject, PluginExecutionDto pluginExecutionDto, Isolated<PluginRuntimeHandler> appDomain, int index, IDev2MethodInfo dev2MethodInfo, IServiceTestStep serviceTestStep)
        {
            if (serviceTestStep != null)
            {
                if (serviceTestStep.Type == StepType.Mock)
                {
                    MockMethodExecution(dataObject, serviceTestStep, dev2MethodInfo, index);
                }
                else
                {
                    RegularMethodExecution(appDomain, pluginExecutionDto, dev2MethodInfo, index, update, dataObject);
                }
            }
            else
            {
                RegularMethodExecution(appDomain, pluginExecutionDto, dev2MethodInfo, index, update, dataObject);
            }
        }

        static void GetFinalTestRunResult(IServiceTestStep serviceTestStep, TestRunResult testRunResult)
        {
            var resultList = new ObservableCollection<TestRunResult>();
            foreach (var testStep in serviceTestStep.Children)
            {
                if (testStep.Result != null)
                {
                    resultList.Add(testStep.Result);
                }
            }

            if (resultList.Count == 0)
            {
                testRunResult.RunTestResult = RunResult.TestPassed;
            }
            else
            {
                testRunResult.RunTestResult = RunResult.TestInvalid;

                var testRunResults = resultList.Where(runResult => runResult.RunTestResult == RunResult.TestInvalid).ToList();
                if (testRunResults.Count > 0)
                {
                    testRunResult.Message = string.Join(Environment.NewLine, testRunResults.Select(result => result.Message));
                    testRunResult.RunTestResult = RunResult.TestInvalid;
                }
                else
                {
                    var failed = resultList.Any(runResult => runResult.RunTestResult == RunResult.TestFailed);
                    if (failed)
                    {
                        testRunResult.Message = string.Join(Environment.NewLine, testRunResults.Select(result => result.Message));
                        testRunResult.RunTestResult = RunResult.TestFailed;
                    }
                    else
                    {
                        testRunResult.Message = Messages.Test_PassedResult;
                        testRunResult.RunTestResult = RunResult.TestPassed;

                    }
                }
            }
        }

        void UpdateDebugWithAssertions(IDSFDataObject dataObject, List<IServiceTestStep> serviceTestTestSteps, Guid childId)
        {
            if (dataObject.IsDebugMode())
            {
                ServiceTestHelper.UpdateDebugStateWithAssertions(dataObject, serviceTestTestSteps, childId.ToString());
            }
        }

        void MockMethodExecution(IDSFDataObject dataObject, IServiceTestStep serviceTestStep, IDev2MethodInfo dev2MethodInfo, int index)
        {
            if (serviceTestStep.StepOutputs != null)
            {
                foreach (var serviceTestOutput in serviceTestStep.StepOutputs)
                {
                    DispatchDebugState(dataObject, dev2MethodInfo, index, serviceTestOutput);
                }
            }
        }

        private void DispatchDebugState(IDSFDataObject dataObject, IDev2MethodInfo dev2MethodInfo, int index, IServiceTestOutput serviceTestOutput)
        {
            var start = DateTime.Now;
            if (dev2MethodInfo.IsObject)
            {
                var jContainer = JToken.Parse(serviceTestOutput.Value) as JContainer
                                 ?? serviceTestOutput.Value.DeserializeToObject();
                if (!string.IsNullOrEmpty(serviceTestOutput.Variable))
                {
                    dataObject.Environment.AddToJsonObjects(serviceTestOutput.Variable, jContainer);
                }
            }
            else
            {
                dataObject.Environment.Assign(serviceTestOutput.Variable, serviceTestOutput.Value, 0);
            }
            dev2MethodInfo.MethodResult = serviceTestOutput.Value;
            MethodsToRun[index].MethodResult = serviceTestOutput.Value;
            DispatchDebugStateForMethod(MethodsToRun[index], dataObject, 0, true, start);
        }

        static void MockConstructorExecution(IDSFDataObject dataObject, IServiceTestStep serviceTestStep, ref PluginExecutionDto pluginExecutionDto)
        {
            if (!string.IsNullOrEmpty(serviceTestStep.StepOutputs?[0].Variable))
            {
                try
                {
                    var languageExpression = EvaluationFunctions.parseLanguageExpression(serviceTestStep.StepOutputs?[0].Variable, 0, ShouldTypeCast.Yes);
                    if (languageExpression.IsJsonIdentifierExpression)
                    {
                        var jToken = JToken.Parse(serviceTestStep.StepOutputs?[0].Value) as JContainer ?? serviceTestStep.StepOutputs?[0].Value.DeserializeToObject();
                        dataObject.Environment.AddToJsonObjects(serviceTestStep.StepOutputs[0].Variable, jToken);
                        pluginExecutionDto.ObjectString = serviceTestStep.StepOutputs[0].Value;
                    }
                }
                catch (Exception e)
                {
                    dataObject.Environment.Errors.Add(e.Message);
                }
            }
        }

        void RegularMethodExecution(Isolated<PluginRuntimeHandler> appDomain, PluginExecutionDto pluginExecutionDto, IDev2MethodInfo dev2MethodInfo, int i, int update, IDSFDataObject dataObject)
        {
            var start = DateTime.Now;
            pluginExecutionDto.ObjectString = ObjectResult;
            var result = PluginServiceExecutionFactory.InvokePlugin(appDomain, pluginExecutionDto, dev2MethodInfo, out string objString);

            pluginExecutionDto.ObjectString = objString;
            ObjectResult = objString;
            var pluginAction = MethodsToRun[i];
            pluginAction.MethodResult = result.MethodResult;
            pluginAction.HasError = result.HasError;
            pluginAction.ErrorMessage = result.ErrorMessage;
            dev2MethodInfo.HasError = result.HasError;
            dev2MethodInfo.ErrorMessage = result.ErrorMessage;
            if (result.HasError)
            {
                DispatchDebugStateForMethod(pluginAction, dataObject, update, false, start);
                return;
            }
            AssignMethodResult(pluginAction, update, dataObject, start);
            if (!string.IsNullOrEmpty(ObjectName) && !pluginExecutionDto.IsStatic)
            {
                var jToken = JToken.Parse(ObjectResult) as JContainer ?? ObjectResult.DeserializeToObject();
                dataObject.Environment.AddToJsonObjects(ObjectName, jToken);
            }
        }

        void RegularConstructorExecution(IDSFDataObject dataObject, Isolated<PluginRuntimeHandler> appDomain, ref PluginExecutionDto pluginExecutionDto)
        {
            pluginExecutionDto = PluginServiceExecutionFactory.ExecuteConstructor(appDomain, pluginExecutionDto);
            ObjectResult = pluginExecutionDto.ObjectString;
            if (!string.IsNullOrEmpty(ObjectName) && !pluginExecutionDto.IsStatic)
            {
                var jToken = JToken.Parse(ObjectResult) as JContainer ?? ObjectResult.DeserializeToObject();
                dataObject.Environment.AddToJsonObjects(ObjectName, jToken);
            }
        }

        void AssignMethodResult(IPluginAction pluginAction, int update, IDSFDataObject dataObject, DateTime start)
        {
            var methodResult = pluginAction.MethodResult;
            var outputVariable = pluginAction.OutputVariable;
            if (pluginAction.IsObject)
            {
                var jContainer = JToken.Parse(methodResult) as JContainer
                                    ?? methodResult.DeserializeToObject();
                if (!string.IsNullOrEmpty(outputVariable))
                {
                    dataObject.Environment.AddToJsonObjects(outputVariable, jContainer);
                }
            }
            else
            {
                if (methodResult != null)
                {
                    UpdateEnvironment(pluginAction, update, dataObject, methodResult, outputVariable);
                }
            }
            DispatchDebugStateForMethod(pluginAction, dataObject, update, false, start);
        }

        private static void UpdateEnvironment(IPluginAction pluginAction, int update, IDSFDataObject dataObject, string methodResult, string outputVariable)
        {
            if (!pluginAction.IsVoid)
            {
                var jObj = JToken.Parse(methodResult) ?? methodResult.DeserializeToObject();
                if (!methodResult.IsJSON() && !pluginAction.IsObject)
                {
                    pluginAction.MethodResult = methodResult.TrimEnd('\"').TrimStart('\"');
                }
                if (jObj != null)
                {
                    AddJObjToEnvironment(pluginAction, update, dataObject, outputVariable, jObj);
                }
            }
        }

        private static void AddJObjToEnvironment(IPluginAction pluginAction, int update, IDSFDataObject dataObject, string outputVariable, JToken jObj)
        {
            if (jObj.IsEnumerableOfPrimitives())
            {
                var values = jObj.Children().Select(token => token.ToString()).ToList();
                if (DataListUtil.IsValueScalar(outputVariable))
                {
                    var valueString = string.Join(",", values);
                    dataObject.Environment.Assign(outputVariable, valueString, update);
                }
                else
                {
                    foreach (var value in values)
                    {
                        dataObject.Environment.Assign(outputVariable, value, update);
                    }
                }
            }
            else
            {
                if (jObj.IsPrimitive())
                {
                    var value = jObj.ToString();
                    if (!value.IsJSON() && !pluginAction.IsObject)
                    {
                        value = value.TrimEnd('\"').TrimStart('\"');
                    }
                    if (!string.IsNullOrEmpty(outputVariable))
                    {
                        dataObject.Environment.Assign(outputVariable, value, update);
                    }
                }
            }
        }

        PluginInvokeArgs BuidlPluginInvokeArgs(int update, IPluginConstructor constructor, INamespaceItem namespaceItem, IDSFDataObject dataObject)
        {
            var pluginSource = ResourceCatalog.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, SourceId);
            return new PluginInvokeArgs
            {
                AssemblyLocation = pluginSource.AssemblyLocation,
                AssemblyName = Namespace.AssemblyName,
                Fullname = namespaceItem.FullName,
                PluginConstructor = constructor,
                MethodsToRun = MethodsToRun?.Select(action =>
                {
                    if (action != null)
                    {
                        return new Dev2MethodInfo
                        {
                            Method = action.Method,
                            ID = action.ID,
                            Parameters = action.Inputs?.Select(p => new MethodParameter
                            {
                                Name = p.Name,
                                Value = GetEvaluatedResult(dataObject, p.Value, p.EmptyIsNull, update),
                                TypeName = p.TypeName,
                                EmptyToNull = p.EmptyIsNull,
                                IsRequired = p.RequiredField
                            } as IMethodParameter).ToList() ?? new List<IMethodParameter>(),
                            IsObject = action.IsObject,
                            MethodResult = action.MethodResult,
                            OutputVariable = action.OutputVariable,
                            IsVoid = action.IsVoid
                        } as IDev2MethodInfo;
                    }
                    return new Dev2MethodInfo();
                }).Where(info => !string.IsNullOrEmpty(info.Method)).ToList() ?? new List<IDev2MethodInfo>()
            };
        }

        #region Overrides of DsfActivity
        [ExcludeFromCodeCoverage] //This does not appear to be used anywhere
#pragma warning disable S1541 // Methods and properties should not be too complex
        protected override void ChildDebugStateDispatch(IDSFDataObject dataObject)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (dataObject.IsDebugMode())
            {
                foreach (var debugState in _childStatesToDispatch)
                {
                    DispatchDebugState(debugState, dataObject);
                    if (dataObject.IsServiceTestExecution)
                    {
                        var serviceTestStep = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children)?.FirstOrDefault(step => step.ActivityID == Guid.Parse(UniqueID));
                        var serviceTestSteps = serviceTestStep?.Children;
                        dataObject.ResourceID = debugState.SourceResourceID;
                        UpdateDebugWithAssertions(dataObject, serviceTestSteps?.ToList(), debugState.ID);
                    }
                }
            }
            if (dataObject.IsServiceTestExecution)
            {
                var serviceTestStep = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children)?.FirstOrDefault(step => step.ActivityID == Guid.Parse(UniqueID));
                if (serviceTestStep != null)
                {
                    UpdateTestStep(dataObject, serviceTestStep);
                }
            }
        }

        private void UpdateTestStep(IDSFDataObject dataObject, IServiceTestStep serviceTestStep)
        {
            if (!dataObject.IsDebugMode())
            {
                var serviceTestSteps = serviceTestStep.Children;
                foreach (var serviceTestTestStep in serviceTestSteps)
                {
                    UpdateForRegularActivity(dataObject, serviceTestTestStep);
                }
            }
            var testRunResult = new TestRunResult();
            GetFinalTestRunResult(serviceTestStep, testRunResult);
            serviceTestStep.Result = testRunResult;
            if (dataObject.IsDebugMode())
            {
                var states = TestDebugMessageRepo.Instance.GetDebugItems(dataObject.ResourceID, dataObject.TestName);
                if (states != null)
                {
                    states = states.Where(state => state.ID == Guid.Parse(UniqueID)).ToList();
                    var debugState = states.FirstOrDefault();
                    if (debugState != null)
                    {
                        AddDebugItem(testRunResult, debugState);
                    }
                }
            }
        }

        static void AddDebugItem(TestRunResult testRunResult, IDebugState debugState)
        {
            var msg = testRunResult.Message;
            if (testRunResult.RunTestResult == RunResult.TestPassed)
            {
                msg = Messages.Test_PassedResult;
            }

            var hasError = testRunResult.RunTestResult == RunResult.TestFailed;

            var debugItemStaticDataParams = new DebugItemServiceTestStaticDataParams(msg, hasError);
            var itemToAdd = new DebugItem();
            itemToAdd.AddRange(debugItemStaticDataParams.GetDebugItemResult());

            if (debugState.AssertResultList != null)
            {
                var addItem = debugState.AssertResultList.Select(debugItem => debugItem.ResultsList.Where(debugItemResult => debugItemResult.Value == Messages.Test_PassedResult)).All(debugItemResults => !debugItemResults.Any());

                if (addItem)
                {
                    debugState.AssertResultList.Add(itemToAdd);
                }
            }
        }

        #endregion

        void DispatchDebugStateForMethod(IPluginAction action, IDSFDataObject dataObject, int update, bool isMock, DateTime start)
        {
            var debugState = PopulateDebugStateWithDefaultValues(dataObject);
            debugState.ID = action.ID;
            debugState.StartTime = start;
            debugState.EndTime = DateTime.Now;
            debugState.DisplayName = action.Method;
            debugState.IsSimulation = false;
            debugState.HasError = action.HasError;
            debugState.ErrorMessage = action.ErrorMessage;
            debugState.Name = action.IsProperty ? "Property" : "Method";
            debugState.Inputs.AddRange(BuildMethodInputs(dataObject.Environment, action, update, isMock));
            var buildMethodOutputs = BuildMethodOutputs(dataObject.Environment, action, update, isMock).ToList();
            debugState.Outputs.AddRange(buildMethodOutputs);
            if (isMock)
            {
                debugState.AssertResultList.AddRange(buildMethodOutputs);
            }
            _childStatesToDispatch.Add(debugState);
        }

        DebugState PopulateDebugStateWithDefaultValues(IDSFDataObject dataObject)
        {
            var debugState = new DebugState
            {
                ParentID = UniqueID.ToGuid(),
                WorkSurfaceMappingId = WorkSurfaceMappingId,
                WorkspaceID = dataObject.WorkspaceID,
                StateType = StateType.All,
                ActualType = GetType().Name,
                ActivityType = ActivityType.Step,
                DisplayName = DisplayName,
                IsSimulation = false,
                ServerID = dataObject.ServerID,
                ClientID = dataObject.ClientID,
                OriginatingResourceID = dataObject.ResourceID,
                SourceResourceID = dataObject.SourceResourceID,
                OriginalInstanceID = dataObject.OriginalInstanceID,
                Version = string.Empty,
                HasError = false,
                Server = GetServerName() ?? "",
                EnvironmentID = dataObject.DebugEnvironmentId,
                SessionID = dataObject.DebugSessionID,

            };
            return debugState;
        }

        void DebugStateForConstructorInputsOutputs(IDSFDataObject dataObject, int update, bool isMock, DateTime start)
        {
            var debugState = PopulateDebugStateWithDefaultValues(dataObject);
            debugState.StartTime = start;
            debugState.EndTime = DateTime.Now;
            debugState.Name = "Constructor";
            if (Constructor != null)
            {
                debugState.DisplayName = Constructor.ConstructorName;
                debugState.ID = Constructor.ID;
            }
            debugState.Inputs.AddRange(BuildConstructorInputs(dataObject.Environment, update, isMock));
            var buildConstructorOutput = BuildConstructorOutput(dataObject.Environment, update, isMock).ToList();
            debugState.Outputs.AddRange(buildConstructorOutput);
            if (isMock)
            {
                debugState.AssertResultList.AddRange(buildConstructorOutput);
            }
            _childStatesToDispatch.Add(debugState);
        }

        string GetEvaluatedResult(IDSFDataObject dataObject, string value, bool emptyToNull, int update)
        {
            var wareWolfNothing = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing);
            var warewolfEvalResult = dataObject.Environment.Eval(value, update);

            if (emptyToNull && (Equals(warewolfEvalResult, wareWolfNothing) || warewolfEvalResult == null))
            {
                return null;
            }
            if (!emptyToNull && (Equals(warewolfEvalResult, wareWolfNothing) || warewolfEvalResult == null))
            {
                return string.Empty;
            }

            return ExecutionEnvironment.WarewolfEvalResultToString(warewolfEvalResult);

        }

        #region Overrides of DsfActivity

        #region Overrides of DsfActivity

        IEnumerable<DebugItem> BuildConstructorInputs(IExecutionEnvironment env, int update, bool isMock)
        {
            var inputs = new List<DebugItem>();
            if (Constructor != null && ConstructorInputs != null && ConstructorInputs.Any())
            {
                foreach (var constructorInput in ConstructorInputs)
                {
                    var debugItem = new DebugItem();
                    AddDebugItem(new DebugEvalResult(constructorInput.Value, constructorInput.Name, env, update, false, false, isMock), debugItem);
                    inputs.Add(debugItem);
                }
            }


            return inputs;
        }
        IEnumerable<DebugItem> BuildConstructorOutput(IExecutionEnvironment env, int update, bool isMock)
        {
            var debugItems = new List<DebugItem>();
            if (!string.IsNullOrEmpty(ObjectName))
            {
                var debugItem = new DebugItem();
                AddDebugItem(new DebugEvalResult(ObjectName, "", env, update, false, false, isMock), debugItem);
                debugItems.Add(debugItem);
            }

            if (string.IsNullOrEmpty(ObjectName) && Constructor.IsExistingObject)
            {
                var debugItem = new DebugItem();
                var constructorValue = DataListUtil.AddBracketsToValueIfNotExist(Constructor.ConstructorName);
                AddDebugItem(new DebugEvalResult(constructorValue, "", env, update, false, false, isMock), debugItem);
                debugItems.Add(debugItem);
            }
            return debugItems;
        }



        IEnumerable<DebugItem> BuildMethodInputs(IExecutionEnvironment env, IPluginAction action, int update, bool isMock)
        {
            var inputs = new List<DebugItem>();
            if (action.Inputs?.Any() ?? false)
            {
                foreach (var methodParameter in action.Inputs)
                {
                    var debugItem = new DebugItem();
                    AddDebugItem(new DebugEvalResult(methodParameter.Value ?? "", methodParameter.Name, env, update, false, false, isMock), debugItem);
                    inputs.Add(debugItem);
                }
            }
            return inputs;
        }
        IEnumerable<DebugItem> BuildMethodOutputs(IExecutionEnvironment env, IPluginAction action, int update, bool isMock)
        {
            var debugOutputs = new List<DebugItem>();
            if (!string.IsNullOrEmpty(action.MethodResult))
            {
                var debugItem = new DebugItem();
                if (action.IsVoid)
                {
                    AddDebugItem(new DebugItemStaticDataParams("None", ""), debugItem);
                }
                else
                {
                    AddDebugItem(new DebugEvalResult(string.IsNullOrEmpty(action.OutputVariable) ? action.MethodResult : action.OutputVariable, "", env, update, false, false, isMock), debugItem);
                }

                debugOutputs.Add(debugItem);
            }
            return debugOutputs;
        }

        #endregion

        #endregion

        public override enFindMissingType GetFindMissingType() => enFindMissingType.DataGridActivity;

        public bool Equals(DsfEnhancedDotNetDllActivityNew other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var comparer = new EnhancedPluginComparer();
            var @equals = comparer.Equals(this, other);
            return base.Equals(other) && @equals;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DsfEnhancedDotNetDllActivityNew)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Namespace != null ? Namespace.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Constructor != null ? Constructor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MethodsToRun != null ? MethodsToRun.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ConstructorInputs != null ? ConstructorInputs.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
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

namespace Dev2.Activities
{
    [ToolDescriptorInfo("DotNetDll", "DotNet DLL", ToolType.Native, "6AEB1038-6332-46F9-8BDD-641DE4EA038D", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Resources_Dot_net_DLL")]
    public class DsfEnhancedDotNetDllActivity : DsfMethodBasedActivity
    {
        private List<IDebugState> _childStatesToDispatch;
        // ReSharper disable once MemberCanBePrivate.Global
        public INamespaceItem Namespace { get; set; }
        public IPluginConstructor Constructor { get; set; }
        public List<IPluginAction> MethodsToRun { get; set; }
        public List<IServiceInput> ConstructorInputs { get; set; }
        public DsfEnhancedDotNetDllActivity()
        {
            Type = "DotNet DLL Connector";
            DisplayName = "DotNet DLL";
            MethodsToRun = new List<IPluginAction>();
            ConstructorInputs = new List<IServiceInput>();
            Outputs = new List<IServiceOutputMapping>();
            ObjectName = string.Empty;
            ObjectResult = string.Empty;
        }


        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO errors, int update)
        {
            errors = new ErrorResultTO();
            if (Namespace == null)
            {
                errors.AddError(ErrorResource.NoNamespaceSelected);
                return;
            }

            if (Constructor == null)
            {
                Constructor = new PluginConstructor();
            }

            ExecuteService(update, out errors, Constructor, Namespace, dataObject);
        }

        private void ExecuteService(int update, out ErrorResultTO errors, IPluginConstructor constructor, INamespaceItem namespaceItem, IDSFDataObject dataObject)
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
            }
            constructor.Inputs = new List<IConstructorParameter>();
            foreach (var parameter in ConstructorInputs)
            {
                var resultToString = GetEvaluatedResult(dataObject, parameter.Value, parameter.EmptyIsNull, update);
                constructor.Inputs.Add(new ConstructorParameter()
                {
                    TypeName = parameter.TypeName,
                    Name = parameter.Name,
                    Value = resultToString,
                    EmptyToNull = parameter.EmptyIsNull,
                    IsRequired = parameter.RequiredField
                });
            }

            var args = BuidlPluginInvokeArgs(update, constructor, namespaceItem, dataObject);
            pluginExecutionDto.Args = args;
            try
            {
                using (var appDomain = PluginServiceExecutionFactory.CreateAppDomain())
                {
                    if (dataObject.IsServiceTestExecution)
                    {
                        var serviceTestStep = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children)?.FirstOrDefault(step => step.UniqueId == pluginExecutionDto.Args.PluginConstructor.ID);
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
                            var serviceTestStep = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children)?.FirstOrDefault(step => step.UniqueId == dev2MethodInfo.ID);
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
                        else
                        {
                            RegularMethodExecution(appDomain, pluginExecutionDto, dev2MethodInfo, index, update, dataObject);

                        }
                        if (dev2MethodInfo.HasError) break;
                        index++;
                     
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Cannot convert given JSON to target type"))
                {
                    errors.AddError(ErrorResource.JSONIncompatibleConversionError + Environment.NewLine + e.Message);
                }
                else
                {
                    errors.AddError(e.Message);
                }
            }
        }

        private static void GetFinalTestRunResult(IServiceTestStep serviceTestStep, TestRunResult testRunResult)
        {
            ObservableCollection<TestRunResult> resultList = new ObservableCollection<TestRunResult>();
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

        private void UpdateDebugWithAssertions(IDSFDataObject dataObject, List<IServiceTestStep> serviceTestTestSteps, Guid childId)
        {
            if (dataObject.IsDebugMode())
            {
                ServiceTestHelper.UpdateDebugStateWithAssertions(dataObject, serviceTestTestSteps, childId.ToString());
            }
        }

        private void MockMethodExecution(IDSFDataObject dataObject, IServiceTestStep serviceTestStep, IDev2MethodInfo dev2MethodInfo, int index)
        {
            if (serviceTestStep.StepOutputs != null)
            {
                foreach (var serviceTestOutput in serviceTestStep.StepOutputs)
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
            }
        }

        private static void MockConstructorExecution(IDSFDataObject dataObject, IServiceTestStep serviceTestStep, ref PluginExecutionDto pluginExecutionDto)
        {
            if (!string.IsNullOrEmpty(serviceTestStep.StepOutputs?[0].Variable))
            {
                try
                {
                    var languageExpression = EvaluationFunctions.parseLanguageExpression(serviceTestStep.StepOutputs?[0].Variable, 0);
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

        private void RegularMethodExecution(Isolated<PluginRuntimeHandler> appDomain, PluginExecutionDto pluginExecutionDto, IDev2MethodInfo dev2MethodInfo, int i, int update, IDSFDataObject dataObject)
        {
            var start = DateTime.Now;
            pluginExecutionDto.ObjectString = ObjectResult;
            string objString;
            IDev2MethodInfo result = PluginServiceExecutionFactory.InvokePlugin(appDomain, pluginExecutionDto, dev2MethodInfo, out objString);

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

        private void RegularConstructorExecution(IDSFDataObject dataObject, Isolated<PluginRuntimeHandler> appDomain, ref PluginExecutionDto pluginExecutionDto)
        {
            pluginExecutionDto = PluginServiceExecutionFactory.ExecuteConstructor(appDomain, pluginExecutionDto);
            ObjectResult = pluginExecutionDto.ObjectString;
            if (!string.IsNullOrEmpty(ObjectName) && !pluginExecutionDto.IsStatic)
            {
                var jToken = JToken.Parse(ObjectResult) as JContainer ?? ObjectResult.DeserializeToObject();
                dataObject.Environment.AddToJsonObjects(ObjectName, jToken);
            }
        }

        private void AssignMethodResult(IPluginAction pluginAction, int update, IDSFDataObject dataObject, DateTime start)
        {
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
                    if (!pluginAction.IsVoid)
                    {
                        JToken jObj = JToken.Parse(methodResult) ?? methodResult.DeserializeToObject();
                        if (!methodResult.IsJSON() && !pluginAction.IsObject)
                        {
                            pluginAction.MethodResult = methodResult.TrimEnd('\"').TrimStart('\"');
                        }
                        if (jObj != null)
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
                            else if (jObj.IsPrimitive())
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
                }
                DispatchDebugStateForMethod(pluginAction, dataObject, update, false, start);
            }
        }

        private PluginInvokeArgs BuidlPluginInvokeArgs(int update, IPluginConstructor constructor, INamespaceItem namespaceItem, IDSFDataObject dataObject)
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

        protected override void ChildDebugStateDispatch(IDSFDataObject dataObject)
        {
            if (dataObject.IsDebugMode())
            {
                foreach (var debugState in _childStatesToDispatch)
                {
                    DispatchDebugState(debugState, dataObject);
                    if (dataObject.IsServiceTestExecution)
                    {
                        var serviceTestStep = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children)?.FirstOrDefault(step => step.UniqueId == Guid.Parse(UniqueID));
                        var serviceTestSteps = serviceTestStep?.Children;
                        dataObject.ResourceID = debugState.SourceResourceID;
                        UpdateDebugWithAssertions(dataObject, serviceTestSteps?.ToList(), debugState.ID);
                    }
                }
            }
            if (dataObject.IsServiceTestExecution)
            {
                var serviceTestStep = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children)?.FirstOrDefault(step => step.UniqueId == Guid.Parse(UniqueID));
                if (serviceTestStep != null)
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
                                var msg = testRunResult.Message;
                                if (testRunResult.RunTestResult == RunResult.TestPassed)
                                {
                                    msg = Messages.Test_PassedResult;
                                }

                                var hasError = testRunResult.RunTestResult == RunResult.TestFailed;

                                var debugItemStaticDataParams = new DebugItemServiceTestStaticDataParams(msg, hasError);
                                DebugItem itemToAdd = new DebugItem();
                                itemToAdd.AddRange(debugItemStaticDataParams.GetDebugItemResult());

                                if (debugState.AssertResultList != null)
                                {
                                    bool addItem = debugState.AssertResultList.Select(debugItem => debugItem.ResultsList.Where(debugItemResult => debugItemResult.Value == Messages.Test_PassedResult)).All(debugItemResults => !debugItemResults.Any());

                                    if (addItem)
                                    {
                                        debugState.AssertResultList.Add(itemToAdd);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        private void DispatchDebugStateForMethod(IPluginAction action, IDSFDataObject dataObject, int update, bool isMock, DateTime start)
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

        private DebugState PopulateDebugStateWithDefaultValues(IDSFDataObject dataObject)
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

        private void DebugStateForConstructorInputsOutputs(IDSFDataObject dataObject, int update, bool isMock, DateTime start)
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

        private string GetEvaluatedResult(IDSFDataObject dataObject, string value, bool emptyToNull, int update)
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

        private IEnumerable<DebugItem> BuildConstructorInputs(IExecutionEnvironment env, int update, bool isMock)
        {
            var inputs = new List<DebugItem>();
            if (Constructor != null)
            {
                if (ConstructorInputs != null && ConstructorInputs.Any())
                {
                    foreach (var constructorInput in ConstructorInputs)
                    {
                        var debugItem = new DebugItem();
                        AddDebugItem(new DebugEvalResult(constructorInput.Value, constructorInput.Name, env, update, false, false, isMock), debugItem);
                        inputs.Add(debugItem);
                    }
                }
            }

            return inputs;
        }
        private IEnumerable<DebugItem> BuildConstructorOutput(IExecutionEnvironment env, int update, bool isMock)
        {
            var debugItems = new List<DebugItem>();
            if (!string.IsNullOrEmpty(ObjectName))
            {
                DebugItem debugItem = new DebugItem();
                AddDebugItem(new DebugEvalResult(ObjectName, "", env, update, false, false, isMock), debugItem);
                debugItems.Add(debugItem);
            }

            if (string.IsNullOrEmpty(ObjectName) && Constructor.IsExistingObject)
            {
                DebugItem debugItem = new DebugItem();
                var constructorValue = DataListUtil.AddBracketsToValueIfNotExist(Constructor.ConstructorName);
                AddDebugItem(new DebugEvalResult(constructorValue, "", env, update, false, false, isMock), debugItem);
                debugItems.Add(debugItem);
            }
            return debugItems;
        }



        private IEnumerable<DebugItem> BuildMethodInputs(IExecutionEnvironment env, IPluginAction action, int update, bool isMock)
        {
            var inputs = new List<DebugItem>();
            if (action.Inputs.Any())
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
        private IEnumerable<DebugItem> BuildMethodOutputs(IExecutionEnvironment env, IPluginAction action, int update, bool isMock)
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

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

    }
}
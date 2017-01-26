using System;
using System.Collections.Generic;
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
using Warewolf.Storage;
using WarewolfParserInterop;

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
            if (!dataObject.IsServiceTestExecution)
            {
                if (Constructor.IsExistingObject)
                {
                    var existingObj = DataListUtil.AddBracketsToValueIfNotExist(Constructor.ConstructorName);
                    var warewolfEvalResult = dataObject.Environment.EvalForJson(existingObj);
                    var existingObject = ExecutionEnvironment.WarewolfEvalResultToString(warewolfEvalResult);
                    pluginExecutionDto = new PluginExecutionDto(existingObject);
                }
                else
                {
                    pluginExecutionDto = new PluginExecutionDto(string.Empty);
                }
                constructor.Inputs = new List<IConstructorParameter>();
                foreach (var parameter in ConstructorInputs)
                {
                    var resultToString = GetEvaluatedResult(dataObject, parameter.Value, update);
                    constructor.Inputs.Add(new ConstructorParameter()
                    {
                        TypeName = parameter.TypeName,
                        Name = parameter.Name,
                        Value = resultToString,
                        EmptyToNull = parameter.EmptyIsNull,
                        IsRequired = parameter.RequiredField
                    });
                }
            }
            else
            {
                pluginExecutionDto = new PluginExecutionDto(string.Empty);
            }

            var args = BuidlPluginInvokeArgs(update, constructor, namespaceItem, dataObject);            
            pluginExecutionDto.Args = args;
            try
            {
                using(var appDomain = PluginServiceExecutionFactory.CreateAppDomain())
                {
                    if (dataObject.IsServiceTestExecution)
                    {
                        var serviceTestStep = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children)?.FirstOrDefault(step => step.UniqueId == pluginExecutionDto.Args.PluginConstructor.ID);
                        if (serviceTestStep != null && serviceTestStep.Type == StepType.Mock)
                        {
                            if (!string.IsNullOrEmpty(serviceTestStep.StepOutputs?[0].Variable))
                            {
                                dataObject.Environment.AssignJson(new AssignValue(serviceTestStep.StepOutputs?[0].Variable, serviceTestStep.StepOutputs?[0].Value), 0);
                                pluginExecutionDto.ObjectString = serviceTestStep.StepOutputs[0].Value;
                            }
                        }
                        else
                        {
                            RegularConstructorExecution(dataObject, appDomain, pluginExecutionDto);
                        }
                    }
                    else
                    {
                        RegularConstructorExecution(dataObject, appDomain, pluginExecutionDto);
                        DebugStateForConstructorInputsOutputs(dataObject, update);
                    }
                    
                    var index = 0;
                    foreach(var dev2MethodInfo in args.MethodsToRun)
                    {
                        if (dataObject.IsServiceTestExecution)
                        {
                            var serviceTestStep = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children)?.FirstOrDefault(step => step.UniqueId == dev2MethodInfo.ID);
                            if (serviceTestStep != null && serviceTestStep.Type == StepType.Mock)
                            {
                                if (serviceTestStep.StepOutputs != null)
                                {
                                    foreach (var serviceTestOutput in serviceTestStep.StepOutputs)
                                    {
                                        if (dev2MethodInfo.IsObject)
                                        {
                                            dataObject.Environment.AssignJson(new AssignValue(serviceTestOutput.Variable, serviceTestOutput.Value), 0);
                                        }
                                        else
                                        {
                                            dataObject.Environment.Assign(serviceTestOutput.Variable, serviceTestOutput.Value, 0);
                                        }
                                        dev2MethodInfo.MethodResult = serviceTestOutput.Value;
                                        MethodsToRun[index].MethodResult = serviceTestOutput.Value;
                                    }
                                }
                            }
                            else
                            {
                                RegularMethodExecution(appDomain, pluginExecutionDto, dev2MethodInfo, index);
                            }
                        }
                        else
                        {
                            RegularMethodExecution(appDomain, pluginExecutionDto, dev2MethodInfo, index);
                        }
                        index++;
                    }
                    
                }
                AssignMethodResult(update, dataObject);
            }
            catch (Exception e)
            {
                if (e.HResult == -2146233088)
                {
                    errors.AddError(ErrorResource.JSONIncompatibleConversionError + Environment.NewLine + e.Message);
                }
                else
                {
                    errors.AddError(e.Message);
                }
            }
        }

        private void RegularMethodExecution(Isolated<PluginRuntimeHandler> appDomain, PluginExecutionDto pluginExecutionDto, IDev2MethodInfo dev2MethodInfo, int i)
        {
            PluginExecutionDto result = PluginServiceExecutionFactory.InvokePlugin(appDomain, pluginExecutionDto, dev2MethodInfo);
            ObjectResult = result.ObjectString;
            MethodsToRun[i].MethodResult = result.Args.MethodsToRun?[i].MethodResult;
        }

        private void RegularConstructorExecution(IDSFDataObject dataObject, Isolated<PluginRuntimeHandler> appDomain, PluginExecutionDto pluginExecutionDto)
        {
            PluginServiceExecutionFactory.ExecuteConstructor(appDomain, pluginExecutionDto);
            if(!string.IsNullOrEmpty(ObjectName) && !pluginExecutionDto.IsStatic)
            {
                var jToken = JToken.Parse(ObjectResult) as JContainer ?? ObjectResult.DeserializeToObject();
                dataObject.Environment.AddToJsonObjects(ObjectName, jToken);
            }
        }

        private void AssignMethodResult(int update, IDSFDataObject dataObject)
        {
            foreach (var pluginAction in MethodsToRun)
            {
                if (pluginAction.IsObject)
                {
                    var jContainer = JToken.Parse(pluginAction.MethodResult) as JContainer
                                     ?? pluginAction.MethodResult.DeserializeToObject();
                    if (!string.IsNullOrEmpty(pluginAction.OutputVariable))
                    {
                        dataObject.Environment.AddToJsonObjects(pluginAction.OutputVariable, jContainer);
                    }
                }
                else
                {
                    if (!pluginAction.IsVoid)
                    {
                        JToken jObj = JToken.Parse(pluginAction.MethodResult) ?? pluginAction.MethodResult.DeserializeToObject();

                        if (jObj != null)
                        {
                            if (jObj.IsEnumerableOfPrimitives())
                            {
                                var values = jObj.Children().Select(token => token.ToString()).ToList();
                                if (DataListUtil.IsValueScalar(pluginAction.OutputVariable))
                                {
                                    var valueString = string.Join(",", values);
                                    dataObject.Environment.Assign(pluginAction.OutputVariable, valueString, update);
                                }
                                else
                                {
                                    foreach (var value in values)
                                    {
                                        dataObject.Environment.Assign(pluginAction.OutputVariable, value, update);
                                    }
                                }
                            }
                            else if (jObj.IsPrimitive())
                            {
                                var value = jObj.ToString();
                                if (!string.IsNullOrEmpty(pluginAction.OutputVariable))
                                {
                                    dataObject.Environment.Assign(pluginAction.OutputVariable, value, update);
                                }
                            }
                        }
                    }
                }
                DispatchDebugStateForMethod(pluginAction, dataObject, update);
            }
        }

        private PluginInvokeArgs BuidlPluginInvokeArgs(int update, IPluginConstructor constructor, INamespaceItem namespaceItem, IDSFDataObject dataObject)
        {
            return new PluginInvokeArgs
            {
                AssemblyLocation = Namespace.AssemblyLocation,
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
                                Value = GetEvaluatedResult(dataObject, p.Value, update),
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
            foreach (var debugState in _childStatesToDispatch)
            {
                DispatchDebugState(debugState, dataObject);
            }
        }

        #endregion

        private void DispatchDebugStateForMethod(IPluginAction action, IDSFDataObject dataObject, int update)
        {
            var debugState = PopulateDebugStateWithDefaultValues(dataObject);
            debugState.ID = action.ID;
            debugState.DisplayName = action.Method;
            debugState.ErrorMessage = string.Empty;
            debugState.IsSimulation = false;
            debugState.HasError = false;
            debugState.Inputs.AddRange(BuildMethodInputs(dataObject.Environment, action, update));
            debugState.Outputs.AddRange(BuildMethodOutputs(dataObject.Environment, action, update));
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
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,
                ActivityType = ActivityType.Step,
                DisplayName = DisplayName,
                IsSimulation = false,
                ServerID = dataObject.ServerID,
                OriginatingResourceID = dataObject.ResourceID,
                OriginalInstanceID = dataObject.OriginalInstanceID,
                Version = string.Empty,
                Name = GetType().Name,
                HasError = false,
                Server = GetServerName() ?? "",
                EnvironmentID = dataObject.DebugEnvironmentId,
                SessionID = dataObject.DebugSessionID
            };
            return debugState;
        }

        private void DebugStateForConstructorInputsOutputs(IDSFDataObject dataObject, int update)
        {
            var debugState = PopulateDebugStateWithDefaultValues(dataObject);
            if (Constructor != null)
            {
                debugState.DisplayName = Constructor.ConstructorName;
                debugState.ID = Constructor.ID;
            }
            debugState.Inputs.AddRange(BuildConstructorInputs(dataObject.Environment, update));
            debugState.Outputs.AddRange(BuildConstructorOutput(dataObject.Environment, update));
            _childStatesToDispatch.Add(debugState);
        }

        private string GetEvaluatedResult(IDSFDataObject dataObject, string value, int update)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var warewolfEvalResult = dataObject.Environment.Eval(value, update);
                return ExecutionEnvironment.WarewolfEvalResultToString(warewolfEvalResult);
            }
            return string.Empty;
        }

        #region Overrides of DsfActivity

        #region Overrides of DsfActivity

        private IEnumerable<DebugItem> BuildConstructorInputs(IExecutionEnvironment env, int update)
        {
            var inputs = new List<DebugItem>();
            if (Constructor != null)
            {
                if (ConstructorInputs != null && ConstructorInputs.Any())
                {
                    foreach (var constructorInput in ConstructorInputs)
                    {
                        var debugItem = new DebugItem();
                        AddDebugItem(new DebugEvalResult(constructorInput.Value, constructorInput.Name, env, update), debugItem);
                        inputs.Add(debugItem);
                    }
                }
            }

            return inputs;
        }
        private IEnumerable<DebugItem> BuildConstructorOutput(IExecutionEnvironment env, int update)
        {
            var debugItems = new List<DebugItem>();
            if (!string.IsNullOrEmpty(ObjectName))
            {
                DebugItem debugItem = new DebugItem();
                AddDebugItem(new DebugEvalResult(ObjectName, "", env, update), debugItem);
                debugItems.Add(debugItem);
            }

            if (string.IsNullOrEmpty(ObjectName) && Constructor.IsExistingObject)
            {
                DebugItem debugItem = new DebugItem();
                var constructorValue = DataListUtil.AddBracketsToValueIfNotExist(Constructor.ConstructorName);
                AddDebugItem(new DebugEvalResult(constructorValue, "", env, update), debugItem);
                debugItems.Add(debugItem);
            }
            return debugItems;
        }



        private IEnumerable<DebugItem> BuildMethodInputs(IExecutionEnvironment env, IPluginAction action, int update)
        {
            var inputs = new List<DebugItem>();
            if (action.Inputs.Any())
            {
                foreach (var methodParameter in action.Inputs)
                {
                    var debugItem = new DebugItem();
                    AddDebugItem(new DebugEvalResult(methodParameter.Value, methodParameter.Name, env, update), debugItem);
                    inputs.Add(debugItem);
                }
            }
            return inputs;
        }
        private IEnumerable<DebugItem> BuildMethodOutputs(IExecutionEnvironment env, IPluginAction action, int update)
        {
            var debugOutputs = new List<DebugItem>();
            if (!string.IsNullOrEmpty(action.MethodResult))
            {
                var debugItem = new DebugItem();
                if (action.IsVoid)
                {
                    AddDebugItem(new DebugItemStaticDataParams("No output", ""), debugItem);
                }
                else
                {
                    AddDebugItem(new DebugEvalResult(string.IsNullOrEmpty(action.OutputVariable) ? action.MethodResult : action.OutputVariable, "", env, update), debugItem);
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
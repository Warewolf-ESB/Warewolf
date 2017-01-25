using System;
using System.Collections.Generic;
using System.Linq;
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
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;
using Newtonsoft.Json.Linq;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("DotNetDll", "DotNet DLL", ToolType.Native, "6AEB1038-6332-46F9-8BDD-641DE4EA038D", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Resources_Dot_net_DLL")]
    public class DsfEnhancedDotNetDllActivity : DsfMethodBasedActivity
    {
        private List<IDebugState> _childStatesToDispatch;
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

        protected void ExecuteService(int update, out ErrorResultTO errors, IPluginConstructor constructor, INamespaceItem namespaceItem, IDSFDataObject dataObject)
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
            }
            else
            {
                pluginExecutionDto = new PluginExecutionDto(string.Empty);
            }
            constructor.Inputs = new List<IConstructorParameter>();
            foreach (var parameter in ConstructorInputs)
            {
                var resultToString = GetEvaluatedResult(dataObject, parameter.Value);
                constructor.Inputs.Add(new ConstructorParameter()
                {
                    TypeName = parameter.TypeName,
                    Name = parameter.Name,
                    Value = resultToString,
                    EmptyToNull = parameter.EmptyIsNull,
                    IsRequired = parameter.RequiredField
                });
            }

            var args = new PluginInvokeArgs
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
                            Parameters = action.Inputs?.Select(p => new MethodParameter
                            {
                                Name = p.Name,
                                Value = GetEvaluatedResult(dataObject, p.Value),
                                TypeName = p.TypeName,
                                EmptyToNull = p.EmptyIsNull,
                                IsRequired = p.RequiredField
                            } as IMethodParameter).ToList() ?? new List<IMethodParameter>(),
                            IsObject = action.IsObject,
                            MethodResult = action.MethodResult,
                            OutputVariable = action.OutputVariable
                        } as IDev2MethodInfo;
                    }
                    return new Dev2MethodInfo();
                }).Where(info => !string.IsNullOrEmpty(info.Method)).ToList() ?? new List<IDev2MethodInfo>()
            };

            pluginExecutionDto.Args = args;
            try
            {
                //if (!Constructor.IsExistingObject)
                //{
                //    pluginExecutionDto = PluginServiceExecutionFactory.CreateInstance(args);
                //}

                PluginExecutionDto result = PluginServiceExecutionFactory.InvokePlugin(pluginExecutionDto);

                ObjectResult = result.ObjectString;

                if (!string.IsNullOrEmpty(ObjectName) && !pluginExecutionDto.IsStatic)
                {
                    var jToken = JToken.Parse(ObjectResult) as JContainer ?? ObjectResult.DeserializeToObject();
                    dataObject.Environment.AddToJsonObjects(ObjectName, jToken);
                }
                DebugStateForConstructorInputsOutputs(dataObject);

                MethodsToRun = result.Args.MethodsToRun?.Select(p => new PluginAction()
                {
                    Method = p.Method,
                    MethodResult = p.MethodResult,
                    IsObject = p.IsObject,
                    OutputVariable = p.OutputVariable,
                    Inputs = p.Parameters?.Select(q => new ServiceInput(q.Name, q.Value)
                    {
                        EmptyIsNull = q.EmptyToNull,
                        RequiredField = q.IsRequired,
                        TypeName = q.TypeName

                    } as IServiceInput).ToList() ?? new List<IServiceInput>()

                } as IPluginAction).ToList() ?? new List<IPluginAction>();// assign return values returned from the seperate AppDomain

                foreach (var pluginAction in MethodsToRun)
                {
                    if (pluginAction.IsVoid)
                        continue;
                    if (pluginAction.IsObject)
                    {

                        var jContainer = JToken.Parse(pluginAction.MethodResult) as JContainer
                            ?? pluginAction.MethodResult.DeserializeToObject();
                        if (!string.IsNullOrEmpty(pluginAction.OutputVariable))
                            dataObject.Environment.AddToJsonObjects(pluginAction.OutputVariable, jContainer);
                    }
                    else
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
                                    dataObject.Environment.Assign(pluginAction.OutputVariable, value, update);
                            }
                        }
                    }
                    DispatchDebugStateForMethod(pluginAction, dataObject);
                }
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

        #region Overrides of DsfActivity

        protected override void ChildDebugStateDispatch(IDSFDataObject dataObject)
        {
            foreach (var debugState in _childStatesToDispatch)
            {
                DispatchDebugState(debugState, dataObject);
            }
        }

        #endregion

        private void DispatchDebugStateForMethod(IPluginAction action, IDSFDataObject dataObject)
        {
            var debugState = PopulateDebugStateWithDefaultValues(dataObject);
            debugState.DisplayName = action.Method;

            debugState.ErrorMessage = string.Empty;
            debugState.IsSimulation = false;
            debugState.HasError = false;
            debugState.Inputs.AddRange(BuildMethodInputs(dataObject.Environment, action));
            debugState.Outputs.AddRange(BuildMethodOutputs(dataObject.Environment, action));
            _childStatesToDispatch.Add(debugState);
        }

        private DebugState PopulateDebugStateWithDefaultValues(IDSFDataObject dataObject)
        {

            var debugState = new DebugState
            {
                ID = Guid.NewGuid(),
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

        private void DebugStateForConstructorInputsOutputs(IDSFDataObject dataObject)
        {
            var debugState = PopulateDebugStateWithDefaultValues(dataObject);
            if (Constructor != null)
                debugState.DisplayName = Constructor.ConstructorName;
            debugState.Inputs.AddRange(BuildConstructorInputs(dataObject.Environment));
            debugState.Outputs.AddRange(BuildConstructorOutput(dataObject.Environment));
            _childStatesToDispatch.Add(debugState);
        }

        private string GetEvaluatedResult(IDSFDataObject dataObject, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var warewolfEvalResult = dataObject.Environment.Eval(value, 0);
                return ExecutionEnvironment.WarewolfEvalResultToString(warewolfEvalResult);
            }
            return string.Empty;
        }

        #region Overrides of DsfActivity

        #region Overrides of DsfActivity

        private IEnumerable<DebugItem> BuildConstructorInputs(IExecutionEnvironment env)
        {
            var inputs = new List<DebugItem>();
            if (Constructor != null)
            {
                if (ConstructorInputs != null && ConstructorInputs.Any())
                {
                    foreach (var constructorInput in ConstructorInputs)
                    {
                        var debugItem = new DebugItem();
                        AddDebugItem(new DebugEvalResult(constructorInput.Value, constructorInput.Name, env, 0), debugItem);
                        inputs.Add(debugItem);
                    }
                }
            }

            return inputs;
        }
        private IEnumerable<DebugItem> BuildConstructorOutput(IExecutionEnvironment env)
        {
            var debugItems = new List<DebugItem>();
            if (!string.IsNullOrEmpty(ObjectName))
            {
                DebugItem debugItem = new DebugItem();
                AddDebugItem(new DebugEvalResult(ObjectName, "", env, 0), debugItem);
                debugItems.Add(debugItem);
            }
            return debugItems;
        }



        private IEnumerable<DebugItem> BuildMethodInputs(IExecutionEnvironment env, IPluginAction action)
        {
            var inputs = new List<DebugItem>();
            if (action.Inputs.Any())
            {
                foreach (var methodParameter in action.Inputs)
                {
                    var debugItem = new DebugItem();
                    AddDebugItem(new DebugEvalResult(methodParameter.Value, methodParameter.Name, env, 0), debugItem);
                    inputs.Add(debugItem);
                }
            }
            return inputs;
        }
        private IEnumerable<DebugItem> BuildMethodOutputs(IExecutionEnvironment env, IPluginAction action)
        {
            var debugOutputs = new List<DebugItem>();
            if (!string.IsNullOrEmpty(action.MethodResult))
            {
                var debugItem = new DebugItem();
                AddDebugItem(new DebugEvalResult(action.OutputVariable, "", env, 0), debugItem);
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
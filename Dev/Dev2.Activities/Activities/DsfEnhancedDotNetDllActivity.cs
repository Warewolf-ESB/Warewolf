using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("DotNetDll", "DotNet DLL", ToolType.Native, "6AEB1038-6332-46F9-8BDD-641DE4EA038D", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Resources_Dot_net_DLL")]
    public class DsfEnhancedDotNetDllActivity : DsfMethodBasedActivity
    {
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
                var paramIterator = dataObject.Environment.Eval(parameter.Value, update);
                var resultToString = ExecutionEnvironment.WarewolfEvalResultToString(paramIterator);
                constructor.Inputs.Add(new ConstructorParameter()
                {
                    TypeName = parameter.TypeName,
                    Name = parameter.Name,
                    Value = resultToString,
                    EmptyToNull = parameter.EmptyIsNull,
                    IsRequired = parameter.RequiredField
                });
                parameter.Value = resultToString;
            }

            var args = new PluginInvokeArgs
            {
                AssemblyLocation = Namespace.AssemblyLocation,
                AssemblyName = Namespace.AssemblyName,
                Fullname = namespaceItem.FullName,
                PluginConstructor = constructor,
                MethodsToRun = MethodsToRun.Select(action => new Dev2MethodInfo
                {
                    Method = action.Method,
                    Parameters = action.Inputs.Select(p => new MethodParameter
                    {
                        Name = p.Name,
                        Value = p.Value,
                        TypeName = p.TypeName,
                        EmptyToNull = p.EmptyIsNull,
                        IsRequired = p.RequiredField
                    } as IMethodParameter).ToList(),
                    IsObject = action.IsObject,
                    MethodResult = action.MethodResult,
                    OutputVariable = action.OutputVariable
                } as IDev2MethodInfo).ToList()
            };

            pluginExecutionDto.Args = args;
            try
            {
                if (!Constructor.IsExistingObject)
                {
                    pluginExecutionDto = PluginServiceExecutionFactory.CreateInstance(args);
                }

                var result = PluginServiceExecutionFactory.InvokePlugin(pluginExecutionDto);

                MethodsToRun = result.Args.MethodsToRun.Select(p => new PluginAction()
                {
                    Method = p.Method,
                    MethodResult = p.MethodResult,
                    IsObject = p.IsObject,
                    OutputVariable = p.OutputVariable,
                    Inputs = p.Parameters.Select(q => new ServiceInput(q.Name, q.Value)
                    {
                        EmptyIsNull = q.EmptyToNull,
                        RequiredField = q.IsRequired,
                        TypeName = q.TypeName

                    } as IServiceInput).ToList()

                } as IPluginAction).ToList();// assign return values returned from the seperate AppDomain
                foreach (var dev2MethodInfo in MethodsToRun)
                {
                    if (dev2MethodInfo.IsObject)
                    {
                        var jContainer = dev2MethodInfo.MethodResult.DeserializeToObject();
                        dataObject.Environment.AddToJsonObjects(dev2MethodInfo.OutputVariable, jContainer);
                    }
                    else
                    {
                        var jObj = dev2MethodInfo.MethodResult.DeserializeToObject();
                        if (jObj.IsEnumerableOfPrimitives())
                        {
                            var values = jObj.Children().Select(token => token.ToString()).ToList();
                            if (DataListUtil.IsValueScalar(dev2MethodInfo.OutputVariable))
                            {
                                var valueString = string.Join(",", values);
                                dataObject.Environment.Assign(dev2MethodInfo.OutputVariable, valueString, 0);
                            }
                            else
                            {
                                foreach (var value in values)
                                {
                                    dataObject.Environment.Assign(dev2MethodInfo.OutputVariable, value, 0);
                                }
                            }
                        }
                        else if (jObj.IsPrimitive())
                        {
                            var value = jObj.ToString();
                            dataObject.Environment.Assign(dev2MethodInfo.OutputVariable, value, 0);
                        }
                    }
                }
                ObjectResult = result.ObjectString;

                if (!string.IsNullOrEmpty(ObjectName))
                {
                    var jContainer = ObjectResult.DeserializeToObject();
                    dataObject.Environment.AddToJsonObjects(ObjectName, jContainer);
                }

            }
            catch (Exception e)
            {
                errors.AddError(e.Message);
            }
        }

        #region Overrides of DsfActivity

        #region Overrides of DsfActivity

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            base.GetDebugOutputs(env, update);
            if (!string.IsNullOrEmpty(ObjectName))
            {
                DebugItem debugItem = new DebugItem();
                AddDebugItem(new DebugItemStaticDataParams("", "Constructor Output"), debugItem);
                AddDebugItem(new DebugEvalResult(ObjectName, "", env, update), debugItem);
            }

            if (MethodsToRun != null && MethodsToRun.Any())
            {
                foreach (var dev2MethodInfo in MethodsToRun)
                {
                    DebugItem debugItem = new DebugItem();
                    AddDebugItem(new DebugItemStaticDataParams(dev2MethodInfo.Method, "Action: "), debugItem);
                    _debugOutputs.Add(debugItem);
                    if (!string.IsNullOrEmpty(dev2MethodInfo.MethodResult))
                    {
                        debugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("", "Output"), debugItem);
                        AddDebugItem(new DebugEvalResult(dev2MethodInfo.MethodResult, "", env, update), debugItem);
                        _debugOutputs.Add(debugItem);
                    }
                }
            }

            return _debugOutputs;
        }

        #endregion

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            _debugInputs = base.GetDebugInputs(env, update);
            if (Constructor != null)
            {
                DebugItem debugItem = new DebugItem();

                AddDebugItem(new DebugItemStaticDataParams("", "Constructor"), debugItem);
                AddDebugItem(new DebugItemStaticDataParams(Constructor.ConstructorName, ""), debugItem);
                _debugInputs.Add(debugItem);
                if (ConstructorInputs != null && ConstructorInputs.Any())
                {
                    debugItem = new DebugItem();
                    AddDebugItem(new DebugItemStaticDataParams("", "Constructor Inputs"), debugItem);
                    foreach (var constructorInput in ConstructorInputs)
                    {
                        AddDebugItem(new DebugEvalResult(constructorInput.Value, constructorInput.Name, env, update), debugItem);
                    }
                    _debugInputs.Add(debugItem);
                }
            }
            if (MethodsToRun != null && MethodsToRun.Any())
            {
                foreach (var dev2MethodInfo in MethodsToRun)
                {

                    if (dev2MethodInfo.Inputs.Any())
                    {
                        DebugItem debugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams(dev2MethodInfo.Method, "Action: "), debugItem);
                        _debugInputs.Add(debugItem);
                        debugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("", "Inputs"), debugItem);
                        foreach (var methodParameter in dev2MethodInfo.Inputs)
                        {
                            AddDebugItem(new DebugEvalResult(methodParameter.Value, methodParameter.Name, env, update), debugItem);
                        }
                        _debugInputs.Add(debugItem);
                    }
                }
            }

            return _debugInputs;
        }

        #endregion

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

    }
}
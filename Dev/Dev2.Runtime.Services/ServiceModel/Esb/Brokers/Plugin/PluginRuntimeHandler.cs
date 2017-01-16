/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;

// ReSharper disable UnusedMember.Global

namespace Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin
{

    /// <summary>
    /// Handler that invokes a plugin in its own app domain
    /// </summary>
    public partial class PluginRuntimeHandler : MarshalByRefObject, IRuntime
    {

        public PluginExecutionDto CreateInstance(PluginInvokeArgs setupInfo)
        {
            VerifyArgument.IsNotNull("setupInfo", setupInfo);
            Assembly loadedAssembly;
            var tryLoadAssembly = _assemblyLoader.TryLoadAssembly(setupInfo.AssemblyLocation, setupInfo.AssemblyName, out loadedAssembly);
            if (!tryLoadAssembly)
                throw new Exception(setupInfo.AssemblyName + "Not found");

            var constructorArgs = new List<object>();
            var type = loadedAssembly.GetType(setupInfo.Fullname);
            if (type.IsAbstract)//IsStatic
            {
                return new PluginExecutionDto(string.Empty) { IsStatic = true };
            }
            if (setupInfo.PluginConstructor.Inputs != null)
            {
                foreach (var constructorArg in setupInfo.PluginConstructor.Inputs)
                {
                    constructorArgs = SetupValuesForParameters(constructorArg.Value, constructorArg.TypeName);
                }
            }

            object instance;
            if (setupInfo.PluginConstructor?.Inputs != null && (setupInfo.PluginConstructor == null || setupInfo.PluginConstructor.Inputs.Any()))
            {
                instance = Activator.CreateInstance(type, constructorArgs);
            }
            else
            {
                instance = Activator.CreateInstance(type);
            }
            var serializeToJsonString = instance.SerializeToJsonString();
            setupInfo.PluginConstructor.ReturnObject = serializeToJsonString;
            return new PluginExecutionDto(serializeToJsonString)
            {
                Args = setupInfo
            };
        }

        public PluginExecutionDto Run(PluginExecutionDto dto)
        {
            try
            {
                var args = dto.Args;
                Assembly loadedAssembly;
                var tryLoadAssembly = _assemblyLoader.TryLoadAssembly(args.AssemblyLocation, args.AssemblyName, out loadedAssembly);
                if (!tryLoadAssembly)
                    throw new Exception(args.AssemblyName + "Not found");
                ExecutePlugin(dto, args, loadedAssembly);
                return dto;
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
                throw;
            }
        }

        private void ExecutePlugin(PluginExecutionDto objectToRun, PluginInvokeArgs setupInfo, Assembly loadedAssembly)
        {
            VerifyArgument.IsNotNull("objectToRun", objectToRun);
            VerifyArgument.IsNotNull("loadedAssembly", loadedAssembly);
            VerifyArgument.IsNotNull("setupInfo", setupInfo);
            var type = loadedAssembly.GetType(setupInfo.Fullname);
            if (objectToRun.IsStatic)
            {
                RunMethods(setupInfo, type, null, InvokeMethodsAction);
                return;
            }
            var instance = objectToRun.ObjectString.DeserializeToObject(type);
            RunMethods(setupInfo, type, instance, InvokeMethodsAction);
        }

        private object InvokeMethodsAction(MethodInfo methodToRun, object instance, List<object> valuedTypeList, Type type)
        {
            if (instance != null)
            {
                var result = methodToRun.Invoke(instance, BindingFlags.InvokeMethod, null, valuedTypeList.ToArray(), CultureInfo.CurrentCulture);
                return result;
            }
            if (valuedTypeList.Count == 0)
            {
                var result = methodToRun.Invoke(null, null);
                return result;
            }
            else
            {
                var result = methodToRun.Invoke(null, BindingFlags.Static | BindingFlags.InvokeMethod, null, valuedTypeList.ToArray(), CultureInfo.CurrentCulture);
                return result;

            }
        }

        private void RunMethods(PluginInvokeArgs setupInfo, Type type, object instance, Func<MethodInfo, object, List<object>, Type, object> invokeMethodsAction)
        {
            if (setupInfo.MethodsToRun != null)
            {
                foreach (var dev2MethodInfo in setupInfo.MethodsToRun)
                {
                    if (dev2MethodInfo.Parameters != null)
                    {
                        var typeList = BuildTypeList(dev2MethodInfo.Parameters);
                        var valuedTypeList = new List<object>();
                        foreach (var methodParameter in dev2MethodInfo.Parameters)
                        {
                            valuedTypeList = SetupValuesForParameters(methodParameter.Value, methodParameter.TypeName);
                        }

                        var methodToRun = typeList.Count == 0 ? type.GetMethod(dev2MethodInfo.Method) : type.GetMethod(dev2MethodInfo.Method, typeList.ToArray());

                        var methodsAction = invokeMethodsAction(methodToRun, instance, valuedTypeList, type);
                        dev2MethodInfo.MethodResult = methodsAction.SerializeToJsonString();
                    }
                }
            }
        }

        private static List<object> SetupValuesForParameters(string value, string typeName)
        {
            var valuedTypeList = new List<object>();
            try
            {
                var anonymousType = JsonConvert.DeserializeObject(value, Type.GetType(typeName));
                if (anonymousType != null)
                {
                    valuedTypeList.Add(anonymousType);
                }
            }
            catch (Exception)
            {
                valuedTypeList.Add(value);
            }
            return valuedTypeList;
        }

        /// <summary>
        /// Lists the methods.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        public ServiceConstructorList ListConstructors(string assemblyLocation, string assemblyName, string fullName)
        {
            Assembly assembly;
            var serviceMethodList = new ServiceConstructorList();
            if (_assemblyLoader.TryLoadAssembly(assemblyLocation, assemblyName, out assembly))
            {
                var type = assembly.GetType(fullName);
                var constructors = type.GetConstructors();

                constructors.ToList().ForEach(info =>
                {
                    var serviceConstructor = new ServiceConstructor();
                    var parameterInfos = info.GetParameters().ToList();
                    parameterInfos.ForEach(parameterInfo =>
                        serviceConstructor.Parameters.Add(
                            new ConstructorParameter
                            {
                                DefaultValue = parameterInfo.DefaultValue?.ToString() ?? string.Empty,
                                EmptyToNull = false,
                                IsRequired = !parameterInfo.IsOptional,
                                Name = parameterInfo.Name,
                                TypeName = parameterInfo.ParameterType.AssemblyQualifiedName,
                                ShortTypeName = parameterInfo.ParameterType.FullName

                            }));
                    serviceMethodList.Add(serviceConstructor);
                });
            }

            return serviceMethodList;
        }
    }
}

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
using Newtonsoft.Json.Linq;
using ServiceStack.Common.Extensions;

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
                return new PluginExecutionDto(string.Empty) { IsStatic = true, Args = setupInfo };
            }
            if (setupInfo.PluginConstructor.Inputs != null)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var constructorArg in setupInfo.PluginConstructor.Inputs)
                {
                    var setupValuesForParameters = SetupValuesForParameters(constructorArg.Value, constructorArg.TypeName, loadedAssembly);
                    if (setupValuesForParameters != null && setupValuesForParameters.Any())
                    {
                        constructorArgs.Add(setupValuesForParameters.First());
                    }
                }
            }

            var instance = BuildInstance(setupInfo, type, constructorArgs, loadedAssembly);
            var serializeToJsonString = instance.SerializeToJsonString(new KnownTypesBinder() { KnownTypes = new List<Type>() { type } });
            // ReSharper disable once PossibleNullReferenceException
            setupInfo.PluginConstructor.ReturnObject = serializeToJsonString;
            return new PluginExecutionDto(serializeToJsonString)
            {
                Args = setupInfo,
            };
        }

        private static object BuildInstance(PluginInvokeArgs setupInfo, Type type, List<object> constructorArgs, Assembly loadedAssembly)
        {
            object instance = new object();
            if(setupInfo.PluginConstructor?.Inputs != null && (setupInfo.PluginConstructor == null || setupInfo.PluginConstructor.Inputs.Any()))
            {
                try
                {
                    instance = Activator.CreateInstance(type, constructorArgs);
                }
                catch(Exception)
                {
                    var types = setupInfo.PluginConstructor?.Inputs.Select(parameter => GetTypeFromLoadedAssembly(parameter.TypeName, loadedAssembly));
                    if(types != null)
                    {
                        var constructorInfo = type.GetConstructor(types.ToArray());
                        if(constructorInfo != null)
                        {
                            instance = constructorInfo.Invoke(constructorArgs.ToArray());
                        }
                    }
                }
            }
            else
            {
                instance = Activator.CreateInstance(type);
            }
            return instance;
        }

        public PluginExecutionDto Run(PluginExecutionDto dto)
        {
            try
            {
                if (!dto.Args.PluginConstructor.IsExistingObject)
                {
                    dto = CreateInstance(dto.Args);
                }
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
            var knownBinder = new KnownTypesBinder();
            loadedAssembly.ExportedTypes.ForEach(t => knownBinder.KnownTypes.Add(t));
            if (objectToRun.IsStatic)
            {
                RunMethods(setupInfo, type, null, InvokeMethodsAction, loadedAssembly);
                return;
            }
            var instance = objectToRun.ObjectString.DeserializeToObject(type, knownBinder);
            RunMethods(setupInfo, type, instance, InvokeMethodsAction, loadedAssembly);
            objectToRun.ObjectString = instance.SerializeToJsonString(knownBinder);//
        }

        private object InvokeMethodsAction(MethodInfo methodToRun, object instance, List<object> valuedTypeList, Type type)
        {
            if (instance != null)
            {

                var result = methodToRun.Invoke(instance, BindingFlags.InvokeMethod | BindingFlags.Instance, null, valuedTypeList.ToArray(), CultureInfo.CurrentCulture);
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

        private void RunMethods(PluginInvokeArgs setupInfo, Type type, object instance, Func<MethodInfo, object, List<object>, Type, object> invokeMethodsAction, Assembly loadedAssembly)
        {
            if (setupInfo.MethodsToRun != null)
            {
                foreach (var dev2MethodInfo in setupInfo.MethodsToRun)
                {
                    if (dev2MethodInfo.Parameters != null)
                    {
                        var typeList = BuildTypeList(dev2MethodInfo.Parameters, loadedAssembly);
                        var valuedTypeList = new List<object>();
                        // ReSharper disable once LoopCanBeConvertedToQuery
                        foreach (var methodParameter in dev2MethodInfo.Parameters)
                        {
                            var a = SetupValuesForParameters(methodParameter.Value, methodParameter.TypeName, loadedAssembly);
                            if (a != null)
                            {
                                var item = a.FirstOrDefault();
                                valuedTypeList.Add(item);
                            }
                        }

                        var methodToRun = typeList.Count == 0 ? type.GetMethod(dev2MethodInfo.Method) : type.GetMethod(dev2MethodInfo.Method, typeList.ToArray());

                        var methodsAction = invokeMethodsAction(methodToRun, instance, valuedTypeList, type);
                        var knownBinder = new KnownTypesBinder();
                        knownBinder.KnownTypes.Add(type);
                        knownBinder.KnownTypes.Add(methodsAction?.GetType());
                        dev2MethodInfo.MethodResult = methodsAction.SerializeToJsonString(knownBinder);
                    }
                }
            }
        }

        private static List<object> SetupValuesForParameters(string value, string typeName, Assembly loadedAssembly)
        {
            var valuedTypeList = new List<object>();
            try
            {
                Type type;
                try
                {
                    type = Type.GetType(typeName);
                    if (type == null) throw new TypeLoadException();
                }
                catch (Exception)
                {
                    type = GetTypeFromLoadedAssembly(typeName, loadedAssembly);
                }

                var anonymousType = JsonConvert.DeserializeObject(value, type);
                if (anonymousType != null)
                {
                    valuedTypeList.Add(anonymousType);
                }
                if (type != null && ((type.IsPrimitive && anonymousType == null) || type.FullName == typeof(string).FullName))
                {
                    valuedTypeList.Add(value);
                }
            }
            catch (Exception)
            {
                valuedTypeList.Add(value);
            }
            return valuedTypeList;
        }

        private static Type GetTypeFromLoadedAssembly(string typeName, Assembly loadedAssembly)
        {
            return Type.GetType(typeName) ?? loadedAssembly.ExportedTypes.FirstOrDefault(p => p.AssemblyQualifiedName != null && p.AssemblyQualifiedName.Equals(typeName, StringComparison.InvariantCultureIgnoreCase));
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

        /// <summary>
        /// Lists the methods.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        public ServiceMethodList ListMethodsWithReturns(string assemblyLocation, string assemblyName, string fullName)
        {
            Assembly assembly;
            var serviceMethodList = new ServiceMethodList();
            if (_assemblyLoader.TryLoadAssembly(assemblyLocation, assemblyName, out assembly))
            {
                var type = assembly.GetType(fullName);
                var methodInfos = type.GetMethods();
                methodInfos.ToList().ForEach(info =>
                {
                    var serviceMethod = new ServiceMethod
                    {
                        Name = info.Name
                    };
                    var returnType = info.ReturnType;

                    if (returnType.IsPrimitive || returnType == typeof(decimal) || returnType == typeof(string))
                    {
                        serviceMethod.Dev2ReturnType = GlobalConstants.PrimitiveReturnValueTag;
                        serviceMethod.IsObject = false;

                    }
                    else if (info.ReturnType == typeof(void))
                    {
                        serviceMethod.IsVoid = true;
                    }
                    else
                    {
                        var jObject = GetPropertiesJObject(returnType);
                        serviceMethod.Dev2ReturnType = jObject.ToString(Formatting.None);
                        serviceMethod.IsObject = true;
                    }
                    var parameterInfos = info.GetParameters().ToList();
                    parameterInfos.ForEach(parameterInfo =>
                        serviceMethod.Parameters.Add(
                            new MethodParameter
                            {
                                DefaultValue = parameterInfo.DefaultValue?.ToString() ?? string.Empty,
                                EmptyToNull = false,
                                IsRequired = true,
                                Name = parameterInfo.Name,
                                TypeName = parameterInfo.ParameterType.AssemblyQualifiedName
                            }));
                    serviceMethodList.Add(serviceMethod);
                });
            }

            return serviceMethodList;
        }

        private static JObject GetPropertiesJObject(Type returnType)
        {
            var properties = returnType.GetProperties()
                .Where(propertyInfo => propertyInfo.CanWrite)
                .ToList();
            var jObject = new JObject();
            foreach (var property in properties)
            {
                jObject.Add(property.Name, "");
            }
            return jObject;
        }

        /// <summary>
        /// Fetches the name space list object.
        /// </summary>
        /// <param name="pluginSource">The plugin source.</param>
        /// <returns></returns>
        public NamespaceList FetchNamespaceListObjectWithJsonObjects(PluginSource pluginSource)
        {
            var interrogatePlugin = ReadNamespacesWithJsonObjects(pluginSource.AssemblyLocation, pluginSource.AssemblyName);
            var namespacelist = new NamespaceList();
            namespacelist.AddRange(interrogatePlugin);
            return namespacelist;
        }
    }
}

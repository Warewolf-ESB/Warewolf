﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Data.Util;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using Unlimited.Framework.Converters.Graph;


namespace Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin
{

    /// <summary>
    /// Handler that invokes a plugin in its own app domain
    /// </summary>
    public partial class PluginRuntimeHandler
    {
        readonly IAssemblyLoader _assemblyLoader;


        public PluginRuntimeHandler(IAssemblyLoader assemblyLoader)
        {
            _assemblyLoader = assemblyLoader;
        }

        public PluginRuntimeHandler()
            : this(new AssemblyLoader())
        {

        }

        string _assemblyLocation = "";

        /// <summary>
        /// Runs the specified setup information.
        /// </summary>
        /// <param name="setupInfo">The setup information.</param>
        /// <returns></returns>
        public object Run(PluginInvokeArgs setupInfo)
        {
            _assemblyLocation = setupInfo.AssemblyLocation;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            if (!_assemblyLoader.TryLoadAssembly(setupInfo.AssemblyLocation, setupInfo.AssemblyName, out Assembly loadedAssembly))
            {
                return null;
            }
            var methodToRun = ExecutePlugin(setupInfo, loadedAssembly, out object pluginResult);
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            var formater = setupInfo.OutputFormatter;
            if (formater != null)
            {
                pluginResult = AdjustPluginResult(pluginResult, methodToRun);
                return formater.Format(pluginResult).ToString();
            }
            pluginResult = JsonConvert.SerializeObject(pluginResult);
            return pluginResult;
        }



        public IOutputDescription Test(PluginInvokeArgs setupInfo, out string jsonResult)
        {
            try
            {
                jsonResult = null;
                _assemblyLocation = setupInfo.AssemblyLocation;
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                if (!_assemblyLoader.TryLoadAssembly(setupInfo.AssemblyLocation, setupInfo.AssemblyName, out Assembly loadedAssembly))
                {
                    return null;
                }
                var methodToRun = ExecutePlugin(setupInfo, loadedAssembly, out object pluginResult);

                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                // do formating here to avoid object serialization issues ;)
                var dataBrowser = DataBrowserFactory.CreateDataBrowser();
                var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();

                if (pluginResult != null)
                {
                    jsonResult = JsonConvert.SerializeObject(pluginResult);
                    pluginResult = AdjustPluginResult(pluginResult, methodToRun);
                    var tmpData = dataBrowser.Map(pluginResult);
                    dataSourceShape.Paths.AddRange(tmpData);

                }

                var result = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
                result.DataSourceShapes.Add(dataSourceShape);
                return result;
            }
            catch (Exception e)
            {
                Dev2Logger.Error("IOutputDescription Test(PluginInvokeArgs setupInfo)", e, GlobalConstants.WarewolfError);
                jsonResult = null;
                return null;
            }
        }

        MethodBase ExecutePlugin(PluginInvokeArgs setupInfo, Assembly loadedAssembly, out object pluginResult)
        {
            var typeList = BuildTypeList(setupInfo.Parameters, loadedAssembly);
            var type = loadedAssembly.GetType(setupInfo.Fullname);
            var valuedTypeList = new List<object>();
            if (setupInfo.Parameters != null)
            {
                foreach (var methodParameter in setupInfo.Parameters)
                {
                    try
                    {
                        var anonymousType = JsonConvert.DeserializeObject(methodParameter.Value, Type.GetType(methodParameter.TypeName));
                        if (anonymousType != null)
                        {
                            valuedTypeList.Add(anonymousType);
                        }
                    }
                    catch (Exception)
                    {
                        valuedTypeList.Add(methodParameter.Value);
                    }
                }
            }

            var methodToRun = type.GetMethod(setupInfo.Method, typeList.ToArray());
            if (methodToRun == null && typeList.Count == 0)
            {
                methodToRun = type.GetMethod(setupInfo.Method);
            }
            var instance = Activator.CreateInstance(type);
            if (methodToRun == null)
            {
                var constructor = type.GetConstructor(typeList.ToArray());
                if (constructor != null)
                {
                    {
                        pluginResult = constructor.Invoke(instance, BindingFlags.InvokeMethod, null, valuedTypeList.ToArray(), CultureInfo.CurrentCulture);
                        return constructor;
                    }
                }
            }


            if (methodToRun != null)
            {
                pluginResult = methodToRun.Invoke(instance, BindingFlags.InvokeMethod, null, valuedTypeList.ToArray(), CultureInfo.CurrentCulture);
                return methodToRun;
            }
            pluginResult = null;
            return null;
        }


        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var tokens = args.Name.Split(",".ToCharArray());
            Dev2Logger.Debug("Resolving : " + args.Name, GlobalConstants.WarewolfDebug);
            var directoryName = Path.GetDirectoryName(_assemblyLocation);
            var path = Path.Combine(new[] { directoryName, tokens[0] + ".dll" });
            var assembly = Assembly.LoadFile(path);
            return assembly;
        }
        /// <summary>
        /// Lists the namespaces.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        public List<string> ListNamespaces(string assemblyLocation, string assemblyName)
        {
            try
            {
                var namespaces = new List<string>();
                if (_assemblyLoader.TryLoadAssembly(assemblyLocation, assemblyName, out Assembly loadedAssembly))
                {
                    // ensure we flush out the rubbish that GAC brings ;)
                    namespaces = loadedAssembly.GetTypes()
                        .Select(t => t.FullName)
                        .Distinct()
                        .Where(q => q.IndexOf("`", StringComparison.Ordinal) < 0
                                    && q.IndexOf("+", StringComparison.Ordinal) < 0
                                    && q.IndexOf("<", StringComparison.Ordinal) < 0
                                    && !q.StartsWith("_")).ToList();
                }
                return namespaces;
            }
            catch (BadImageFormatException e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                throw;
            }
        }

        /// <summary>
        /// Lists the namespaces.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        IEnumerable<KeyValuePair<string, string>> ListNamespacesWIthReturnTypes(string assemblyLocation, string assemblyName)
        {
            try
            {
                var namespaces = new List<KeyValuePair<string, string>>();
                if (_assemblyLoader.TryLoadAssembly(assemblyLocation, assemblyName, out Assembly loadedAssembly))
                {
                    // ensure we flush out the rubbish that GAC brings ;)
                    var types = loadedAssembly.GetTypes();
                    var keyValuePairs = types.Select(t => new KeyValuePair<string, string>(t.FullName, GetPropertiesJObject(t).ToString(Formatting.None)))
                        .Distinct()
                        .Where(q => q.Value.IndexOf("`", StringComparison.Ordinal) < 0
                                    && q.Value.IndexOf("+", StringComparison.Ordinal) < 0
                                    && q.Value.IndexOf("<", StringComparison.Ordinal) < 0
                                    && !q.Value.StartsWith("_")).ToList();
                    namespaces.AddRange(keyValuePairs);
                }
                return namespaces;
            }
            catch (BadImageFormatException e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                throw;
            }
        }

        /// <summary>
        /// Lists the methods.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        public ServiceMethodList ListMethods(string assemblyLocation, string assemblyName, string fullName)
        {
            var serviceMethodList = new ServiceMethodList();
            if (_assemblyLoader.TryLoadAssembly(assemblyLocation, assemblyName, out Assembly assembly))
            {
                var type = assembly.GetType(fullName);
                var methodInfos = type.GetMethods();
                var constructors = type.GetConstructors();

                constructors.ToList().ForEach(info =>
                {
                    var serviceMethod = new ServiceMethod { Name = info.Name };
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


                methodInfos.ToList().ForEach(info =>
                {
                    var serviceMethod = new ServiceMethod { Name = info.Name };
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

        /// <summary>
        /// Fetches the name space list object.
        /// </summary>
        /// <param name="pluginSource">The plugin source.</param>
        /// <returns></returns>
        public NamespaceList FetchNamespaceListObject(PluginSource pluginSource)
        {
            var interrogatePlugin = ReadNamespaces(pluginSource.AssemblyLocation, pluginSource.AssemblyName);
            var namespacelist = new NamespaceList();
            namespacelist.AddRange(interrogatePlugin);
            return namespacelist;
        }

        /// <summary>
        /// Adjusts the plugin result.
        /// </summary>
        /// <param name="pluginResult">The plugin result.</param>
        /// <param name="methodToRun">The method automatic run.</param>
        /// <returns></returns>
        object AdjustPluginResult(object pluginResult, MethodBase methodToRun)
        {
            var result = pluginResult;
            // When it returns a primitive or string and it is not XML or JSON, make it so ;)

            if ((methodToRun is MethodInfo method && (method.ReturnType.IsPrimitive || method.ReturnType.FullName == "System.String")) && !DataListUtil.IsXml(pluginResult.ToString()) && !DataListUtil.IsJson(pluginResult.ToString()))
            {
                // add our special tags ;)
                result = $"<{GlobalConstants.PrimitiveReturnValueTag}>{pluginResult}</{GlobalConstants.PrimitiveReturnValueTag}>";
            }

            return result;
        }

        /// <summary>
        /// Reads the namespaces.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        IEnumerable<NamespaceItem> ReadNamespaces(string assemblyLocation, string assemblyName)
        {
            try
            {
                var result = new List<NamespaceItem>();
                var list = ListNamespaces(assemblyLocation, assemblyName);
                list.ForEach(fullName =>
                    result.Add(new NamespaceItem
                    {
                        AssemblyLocation = assemblyLocation,
                        AssemblyName = assemblyName,
                        FullName = fullName
                    }));

                return result;
            }

            catch (BadImageFormatException)
            {
                throw;
            }
        }

        /// <summary>
        /// Reads the namespaces.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        IEnumerable<NamespaceItem> ReadNamespacesWithJsonObjects(string assemblyLocation, string assemblyName)
        {
            try
            {
                var result = new List<NamespaceItem>();
                var list = ListNamespacesWIthReturnTypes(assemblyLocation, assemblyName);



                foreach (var keyVale in list)
                {
                    result.Add(new NamespaceItem
                    {
                        AssemblyLocation = assemblyLocation,
                        AssemblyName = assemblyName,
                        FullName = keyVale.Key,
                        JsonObject = keyVale.Value
                    });
                }

                return result;
            }

            catch (BadImageFormatException)
            {
                throw;
            }
        }

        /// <summary>
        /// Builds the type list.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        List<Type> BuildTypeList(IEnumerable<IMethodParameter> parameters, Assembly assembly)
        {
            var typeList = new List<Type>();

            if (parameters != null)
            {
                foreach (var methodParameter in parameters)
                {
                    Type type;
                    try
                    {
                        type = GetTypeFromLoadedAssembly(methodParameter.TypeName, assembly);

                    }
                    catch (Exception)
                    {
                        type = Type.GetType(methodParameter.TypeName, true);
                    }

                    typeList.Add(type);
                }
            }

            return typeList;
        }
    }
}

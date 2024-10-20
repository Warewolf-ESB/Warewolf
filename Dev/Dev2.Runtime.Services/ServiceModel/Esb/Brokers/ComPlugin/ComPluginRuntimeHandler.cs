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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Data.Util;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;
using Unlimited.Framework.Converters.Graph;
using WarewolfCOMIPC.Client;




namespace Dev2.Runtime.ServiceModel.Esb.Brokers.ComPlugin
{
    public class ComPluginRuntimeHandler : MarshalByRefObject, IRuntime
    {
        readonly INamedPipeClientStreamWrapper _clientStreamWrapper;

        public ComPluginRuntimeHandler(INamedPipeClientStreamWrapper clientStreamWrapper)
        {
            _clientStreamWrapper = clientStreamWrapper;
        }

        public ComPluginRuntimeHandler()
        {
            
        }

        public object Run(ComPluginInvokeArgs setupInfo)
        {
            var methodToRun = ExecuteComPlugin(setupInfo, out object pluginResult);
            var formater = setupInfo.OutputFormatter;
            if (formater != null)
            {
                pluginResult = AdjustPluginResult(pluginResult);
                return formater.Format(pluginResult).ToString();
            }
            return pluginResult;
        }
        public IOutputDescription Test(ComPluginInvokeArgs setupInfo, out string serializedResult)
        {
            try
            {
                serializedResult = null;

                var methodToRun = ExecuteComPlugin(setupInfo, out object pluginResult);

                // do formating here to avoid object serialization issues ;)
                var dataBrowser = DataBrowserFactory.CreateDataBrowser();
                var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();

                if (pluginResult != null)
                {
                    if (pluginResult is KeyValuePair<bool, string>)
                    {
                        var pluginKeyValuePair = (KeyValuePair<bool, string>) pluginResult;
                        serializedResult = "Exception: " + pluginKeyValuePair.Value;
                    }
                    else
                    {
                        serializedResult = pluginResult.ToString();
                    }
                    pluginResult = AdjustPluginResult(pluginResult);
                    var tmpData = dataBrowser.Map(pluginResult);
                    dataSourceShape.Paths.AddRange(tmpData);
                }

                var result = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
                result.DataSourceShapes.Add(dataSourceShape);
                return result;
            }
            catch (COMException e)
            {
                Dev2Logger.Error("IOutputDescription Test(PluginInvokeArgs setupInfo)", e, GlobalConstants.WarewolfError);
                throw;
            }
            catch (Exception e)
            {
                if (e.InnerException is COMException)
                {
                    throw e.InnerException;
                }

                Dev2Logger.Error("IOutputDescription Test(PluginInvokeArgs setupInfo)", e, GlobalConstants.WarewolfError);
                serializedResult = null;
                return null;
            }

        }

        MethodInfo ExecuteComPlugin(ComPluginInvokeArgs setupInfo, out object pluginResult)
        {

            if (!string.IsNullOrEmpty(setupInfo.ClsId))
            {
                if (setupInfo.Is32Bit)
                {
                    var strings = setupInfo.Parameters.Select(parameter => new ParameterInfoTO { Name = parameter.Name, DefaultValue = parameter.Value, TypeName = parameter.TypeName }).ToArray();
                    pluginResult = IpcClient.GetIpcExecutor(_clientStreamWrapper).Invoke(setupInfo.ClsId.ToGuid(), setupInfo.Method, Execute.ExecuteSpecifiedMethod, strings);
                    return null;
                }
                var typeList = BuildTypeList(setupInfo.Parameters);
                var valuedTypeList = TryBuildValuedTypeParams(setupInfo);
                var type = GetType(setupInfo.ClsId, setupInfo.Is32Bit);
                var methodToRun = type.GetMethod(setupInfo.Method, typeList.ToArray()) ??
                                  type.GetMethod(setupInfo.Method);
                if (methodToRun == null && typeList.Count == 0)
                {
                    methodToRun = type.GetMethod(setupInfo.Method);
                }
                var instance = Activator.CreateInstance(type);

                if (methodToRun != null)
                {
                    if (methodToRun.ReturnType == typeof(void))
                    {
                        methodToRun.Invoke(instance,
                            BindingFlags.InvokeMethod | BindingFlags.IgnoreCase | BindingFlags.Public, null,
                            valuedTypeList.ToArray(), CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        pluginResult = methodToRun.Invoke(instance,
                            BindingFlags.InvokeMethod | BindingFlags.IgnoreCase | BindingFlags.Public, null,
                            valuedTypeList.ToArray(), CultureInfo.InvariantCulture);
                        return methodToRun;
                    }
                }
            }
            pluginResult = null;
            return null;
        }

        static IEnumerable<object> TryBuildValuedTypeParams(ComPluginInvokeArgs setupInfo)
        {
            var valuedTypeList = new object[setupInfo.Parameters.Count];

            for (int index = 0; index < setupInfo.Parameters.Count; index++)
            {
                var methodParameter = setupInfo.Parameters[index];
                try
                {
                    BuildValuedTypeParams(ref valuedTypeList, index, methodParameter);
                }
                catch (Exception)
                {
                    TryBuildValuedTypeParamsAgain(ref valuedTypeList, index, methodParameter);
                }
            }
            return valuedTypeList;
        }

        static void BuildValuedTypeParams(ref object[] valuedTypeList, int index, MethodParameter methodParameter)
        {
            var anonymousType = JsonConvert.DeserializeObject(methodParameter.Value, Type.GetType(methodParameter.TypeName));
            if (anonymousType != null)
            {
                valuedTypeList[index] = anonymousType;
            }
        }

        static void TryBuildValuedTypeParamsAgain(ref object[] valuedTypeList, int index, MethodParameter methodParameter)
        {
            var argType = Type.GetType(methodParameter.TypeName);
            try
            {
                if (argType != null)
                {
                    var provider = TypeDescriptor.GetConverter(argType);
                    var convertFrom = provider.ConvertFrom(methodParameter.Value);
                    valuedTypeList[index] = convertFrom;
                }
            }
            catch (Exception)
            {
                try
                {
                    var typeConverter = TypeDescriptor.GetConverter(methodParameter.Value);
                    var convertFrom = typeConverter.ConvertFrom(methodParameter.Value);
                    valuedTypeList[index] = convertFrom;
                }
                catch (Exception k)
                {
                    Dev2Logger.Error($"Failed to convert {argType?.FullName}", k, GlobalConstants.WarewolfError);
                }
            }
        }

        /// <summary>
        /// Lists the namespaces.
        /// </summary>
        /// <param name="classId">The assembly location.</param>
        /// <param name="is32Bit"></param>
        /// <returns></returns>
        List<string> ListNamespaces(string classId, bool is32Bit)
        {
            try
            {
                if (is32Bit)
                {

                    var execute = IpcClient.GetIpcExecutor(_clientStreamWrapper).Invoke(classId.ToGuid(), "", Execute.GetNamespaces, new ParameterInfoTO[] { });
                    var namespaceList = execute as List<string>;
                    return namespaceList;

                }
                var type = GetType(classId, false);
                var loadedAssembly = type.Assembly;
                // ensure we flush out the rubbish that GAC brings ;)
                var namespaces = loadedAssembly.GetTypes()
                    .Select(t => t.FullName)
                    .Distinct()
                    .Where(q => q.IndexOf("`", StringComparison.Ordinal) < 0
                                && q.IndexOf("+", StringComparison.Ordinal) < 0
                                && q.IndexOf("<", StringComparison.Ordinal) < 0
                                && !q.StartsWith("_")).ToList();

                return namespaces;
            }
            catch (BadImageFormatException e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                throw;
            }
        }

        Type GetType(string classId, bool is32Bit)
        {
            Guid.TryParse(classId, out Guid clasID);
            var is64BitProcess = Environment.Is64BitProcess;
            Type type;
            if (is64BitProcess && is32Bit)
            {

                try
                {
                    var execute = IpcClient.GetIpcExecutor(_clientStreamWrapper).Invoke(clasID, "", Execute.GetType, new ParameterInfoTO[] { });
                    type = execute as Type;
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error("GetType", ex, GlobalConstants.WarewolfError);
                    type = Type.GetTypeFromCLSID(clasID, true);

                }
            }
            else
            {
                type = Type.GetTypeFromCLSID(clasID, true);
            }
            return string.IsNullOrEmpty(classId) ? null : type;
        }

        public ServiceMethodList ListMethods(string classId, bool is32Bit)
        {
            var serviceMethodList = new List<ServiceMethod>();
            var orderMethodsList = new ServiceMethodList();
            classId = classId.Replace("{", "").Replace("}", "");
            if (is32Bit)
            {
                var execute = IpcClient.GetIpcExecutor(_clientStreamWrapper).Invoke(classId.ToGuid(), "", Execute.GetMethods, new ParameterInfoTO[] { });
                if (execute is List<MethodInfoTO> ipcMethods)
                {
                    ServiceMethodList(ref serviceMethodList, orderMethodsList, ipcMethods);
                    return orderMethodsList;
                }
            }
            if (string.IsNullOrEmpty(classId))
            {
                return new ServiceMethodList();
            }
            var type = Type.GetTypeFromCLSID(classId.ToGuid(), true);
            var methodInfos = type.GetMethods();

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
                orderMethodsList.Add(serviceMethod);
            });
            return orderMethodsList;
        }

        static void ServiceMethodList(ref List<ServiceMethod> serviceMethodList, ServiceMethodList orderMethodsList, List<MethodInfoTO> ipcMethods)
        {
            foreach (MethodInfoTO ipcMethod in ipcMethods)
            {
                var parameterInfos = ipcMethod.Parameters;
                var serviceMethod = new ServiceMethod { Name = ipcMethod.Name };
                foreach (var parameterInfo in parameterInfos)
                {
                    serviceMethod.Parameters.Add(new MethodParameter
                    {
                        DefaultValue = parameterInfo.DefaultValue?.ToString() ?? string.Empty,
                        EmptyToNull = false,
                        IsRequired = true,
                        Name = parameterInfo.Name,
                        TypeName = parameterInfo.TypeName
                    });

                }
                serviceMethodList.Add(serviceMethod);
            }

            orderMethodsList.AddRange(serviceMethodList.OrderBy(method => method.Name));
        }

        /// <summary>
        /// Fetches the name space list object.
        /// </summary>
        /// <param name="pluginSource">The plugin source.</param>
        /// <returns></returns>
        public NamespaceList FetchNamespaceListObject(ComPluginSource pluginSource)
        {
            var interrogatePlugin = ReadNamespaces(pluginSource.ClsId, pluginSource.Is32Bit);
            var namespacelist = new NamespaceList();
            namespacelist.AddRange(interrogatePlugin);
            namespacelist.Add(new NamespaceItem());
            return namespacelist;
        }


        /// <summary>
        /// Adjusts the plugin result.
        /// </summary>
        /// <param name="pluginResult">The plugin result.</param>
        /// <returns></returns>
        object AdjustPluginResult(object pluginResult)
        {
            var result = pluginResult;
            var typeConverter = TypeDescriptor.GetConverter(result);
            // When it returns a primitive or string and it is not XML or JSON, make it so ;)

            if ((typeConverter.GetType().IsPrimitive || typeConverter.GetType().FullName.ToLower().Contains("stringconverter"))
                && !DataListUtil.IsXml(pluginResult.ToString()) && !DataListUtil.IsJson(pluginResult.ToString()))
            {
                // add our special tags ;)
                result = $"<{GlobalConstants.PrimitiveReturnValueTag}>{pluginResult}</{GlobalConstants.PrimitiveReturnValueTag}>";
            }

            return result;
        }

        /// <summary>
        /// Reads the namespaces.
        /// </summary>
        /// <param name="clsId">The assembly location.</param>
        /// <param name="is32Bit"></param>
        /// <returns></returns>
        IEnumerable<NamespaceItem> ReadNamespaces(string clsId, bool is32Bit)
        {
            try
            {
                var result = new List<NamespaceItem>();
                var list = ListNamespaces(clsId, is32Bit);
                list?.ForEach(fullName =>
                    result.Add(new NamespaceItem
                    {
                        AssemblyLocation = clsId,
                        FullName = fullName,
                        AssemblyName = fullName
                    }));

                return result;
            }

            catch (BadImageFormatException)
            {
                throw;
            }
        }

        Type DeriveType(string typename)
        {
            var type = Type.GetType(typename, true);
            return type;
        }

        /// <summary>
        /// Builds the type list.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        List<Type> BuildTypeList(IEnumerable<MethodParameter> parameters)
        {
            var typeList = new List<Type>();

            foreach (var methodParameter in parameters)
            {
                var type = DeriveType(methodParameter.TypeName);

                typeList.Add(type);
            }
            return typeList;
        }
    }
}

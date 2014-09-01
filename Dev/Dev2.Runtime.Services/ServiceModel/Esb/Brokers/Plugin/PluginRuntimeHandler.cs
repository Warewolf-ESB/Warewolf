using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Data.Util;
using Dev2.Runtime.ServiceModel.Data;
using ServiceStack.Common.Extensions;
using Unlimited.Framework.Converters.Graph;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin
{
    /// <summary>
    /// Handler that invokes a plugin in its own app domain
    /// </summary>
    public class PluginRuntimeHandler : MarshalByRefObject, IRuntime
    {

        /// <summary>
        /// Runs the specified setup information.
        /// </summary>
        /// <param name="setupInfo">The setup information.</param>
        /// <returns></returns>
        public object Run(PluginInvokeArgs setupInfo)
        {
            Assembly loadedAssembly;

            if(!TryLoadAssembly(setupInfo.AssemblyLocation, setupInfo.AssemblyName, out loadedAssembly))
            {
                return null;
            }

            var parameters = BuildParameterList(setupInfo.Parameters);
            var typeList = BuildTypeList(setupInfo.Parameters);

            var type = loadedAssembly.GetType(setupInfo.Fullname);
            var methodToRun = type.GetMethod(setupInfo.Method, typeList);
            var instance = Activator.CreateInstance(type);
            var pluginResult = methodToRun.Invoke(instance, parameters);

            // do formating here to avoid object serialization issues ;)
            var formater = setupInfo.OutputFormatter;
            if(formater != null)
            {
                pluginResult = AdjustPluginResult(pluginResult, methodToRun);

                return formater.Format(pluginResult).ToString();
            }

            return pluginResult;
        }

        public IOutputDescription Test(PluginInvokeArgs setupInfo)
        {
            Assembly loadedAssembly;

            if(!TryLoadAssembly(setupInfo.AssemblyLocation, setupInfo.AssemblyName, out loadedAssembly))
            {
                return null;
            }

            var parameters = BuildParameterList(setupInfo.Parameters);
            var typeList = BuildTypeList(setupInfo.Parameters);

            var type = loadedAssembly.GetType(setupInfo.Fullname);
            var methodToRun = type.GetMethod(setupInfo.Method, typeList);
            var instance = Activator.CreateInstance(type);
            var pluginResult = methodToRun.Invoke(instance, parameters);

            // do formating here to avoid object serialization issues ;)
            var dataBrowser = DataBrowserFactory.CreateDataBrowser();
            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();

            if(pluginResult != null)
            {
                pluginResult = AdjustPluginResult(pluginResult, methodToRun);

                var tmpData = dataBrowser.Map(pluginResult);
                dataSourceShape.Paths.AddRange(tmpData);
            }

            var result = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            result.DataSourceShapes.Add(dataSourceShape);

            return result;
        }

        /// <summary>
        /// Lists the namespaces.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        public IEnumerable<string> ListNamespaces(string assemblyLocation, string assemblyName)
        {
            Assembly loadedAssembly;
            IEnumerable<string> namespaces = new string[0];
            if(TryLoadAssembly(assemblyLocation, assemblyName, out loadedAssembly))
            {
                // ensure we flush out the rubbish that GAC brings ;)
                namespaces = loadedAssembly.GetTypes()
                                         .Select(t => t.FullName)
                                         .Distinct()
                                         .Where(q => q.IndexOf("`", StringComparison.Ordinal) < 0
                                                  && q.IndexOf("+", StringComparison.Ordinal) < 0
                                                  && q.IndexOf("<", StringComparison.Ordinal) < 0
                                                  && !q.StartsWith("_"));
            }
            return namespaces;
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
            Assembly assembly;
            var serviceMethodList = new ServiceMethodList();
            if(TryLoadAssembly(assemblyLocation, assemblyName, out assembly))
            {
                var type = assembly.GetType(fullName);
                var methodInfos = type.GetMethods();

                methodInfos.ToList().ForEach(info =>
                {
                    var serviceMethod = new ServiceMethod { Name = info.Name };
                    var parameterInfos = info.GetParameters().ToList();
                    parameterInfos.ForEach(parameterInfo =>
                        serviceMethod.Parameters.Add(
                            new MethodParameter
                            {
                                DefaultValue = parameterInfo.DefaultValue.ToString(),
                                EmptyToNull = false,
                                IsRequired = true,
                                Name = parameterInfo.Name,
                                Type = parameterInfo.ParameterType
                            }));
                    serviceMethodList.Add(serviceMethod);
                });
            }

            return serviceMethodList;
        }

        public string ValidatePlugin(string toLoad)
        {
            string result = string.Empty;

            if(toLoad.StartsWith(GlobalConstants.GACPrefix))
            {
                try
                {
                    var readlLoad = toLoad.Remove(0, GlobalConstants.GACPrefix.Length);
                    Assembly.Load(readlLoad);
                }
                catch(Exception e)
                {
                    this.LogError(e);
                    result = e.Message;
                }
            }
            else if(toLoad.EndsWith(".dll"))
            {
                try
                {
                    Assembly.LoadFile(toLoad);
                }
                catch
                {
                    try
                    {
                        Assembly.UnsafeLoadFrom(toLoad);
                    }
                    catch(Exception e)
                    {
                        this.LogError(e);
                        result = e.Message;
                    }
                }
            }
            else
            {
                //does not start with gac prefix or end with .dll
                result = "Not a Dll file";
            }

            return result;
        }

        /// <summary>
        /// Fetches the name space list object.
        /// </summary>
        /// <param name="pluginSource">The plugin source.</param>
        /// <returns></returns>
        public NamespaceList FetchNamespaceListObject(PluginSource pluginSource)
        {
            // BUG 9500 - 2013.05.31 - TWR : added check to avoid nulling AssemblyLocation/Name in tests 
            if(string.IsNullOrEmpty(pluginSource.AssemblyLocation))
            {
                pluginSource = new PluginSources().Get(pluginSource.ResourceID.ToString(), Guid.Empty, Guid.Empty);
            }
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
        private object AdjustPluginResult(object pluginResult, MethodInfo methodToRun)
        {
            object result = pluginResult;
            // When it returns a primitive or string and it is not XML or JSON, make it so ;)
            if((methodToRun.ReturnType.IsPrimitive || methodToRun.ReturnType.FullName == "System.String")
                && !DataListUtil.IsXml(pluginResult.ToString()) && !DataListUtil.IsJson(pluginResult.ToString()))
            {
                // add our special tags ;)
                result = string.Format("<{0}>{1}</{2}>", GlobalConstants.PrimitiveReturnValueTag, pluginResult, GlobalConstants.PrimitiveReturnValueTag);
            }

            return result;
        }


        /// <summary>
        /// Reads the namespaces.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        private IEnumerable<NamespaceItem> ReadNamespaces(string assemblyLocation, string assemblyName)
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

        /// <summary>
        /// Builds the parameter list.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private object[] BuildParameterList(List<MethodParameter> parameters)
        {

            if(parameters.Count == 0) return new object[] { };
            var parameterValues = new object[parameters.Count];
            int pos = 0;
            parameters.ForEach(parameter =>
            {
                parameterValues[pos] = Convert.ChangeType(parameter.Value, parameter.Type);
                pos++;
            });
            return parameterValues;
        }

        /// <summary>
        /// Builds the type list.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private Type[] BuildTypeList(List<MethodParameter> parameters)
        {

            if(parameters.Count == 0) return new Type[] { };
            var typeList = new Type[parameters.Count];
            int pos = 0;
            parameters.ForEach(parameter =>
            {
                typeList[pos] = parameter.Type;
                pos++;
            });
            return typeList;
        }


        /// <summary>
        /// Tries the load assembly.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="loadedAssembly">The loaded assembly.</param>
        /// <returns></returns>
        private bool TryLoadAssembly(string assemblyLocation, string assemblyName, out Assembly loadedAssembly)
        {
            loadedAssembly = null;

            if(assemblyLocation.StartsWith(GlobalConstants.GACPrefix))
            {

                // Culture=neutral, PublicKeyToken=b77a5c561934e089
                // , Version=2.0.0.0
                try
                {
                    loadedAssembly = Assembly.Load(assemblyName);
                    LoadDepencencies(loadedAssembly, assemblyLocation);
                    return true;
                }
                catch(Exception e)
                {
                    this.LogError(e.Message);
                }
            }
            else
            {
                try
                {
                    loadedAssembly = Assembly.LoadFile(assemblyLocation);
                    LoadDepencencies(loadedAssembly, assemblyLocation);
                    return true;
                }
                catch
                {
                    try
                    {
                        loadedAssembly = Assembly.UnsafeLoadFrom(assemblyLocation);
                        LoadDepencencies(loadedAssembly, assemblyLocation);
                        return true;
                    }
                    catch(Exception e)
                    {
                        this.LogError(e);
                    }
                }
                try
                {
                    var objHAndle = Activator.CreateInstanceFrom(assemblyLocation, assemblyName);
                    var loadedObject = objHAndle.Unwrap();
                    loadedAssembly = Assembly.GetAssembly(loadedObject.GetType());
                    LoadDepencencies(loadedAssembly, assemblyLocation);
                    return true;
                }
                catch(Exception e)
                {
                    this.LogError(e);
                }
            }
            return false;
        }


        /// <summary>
        /// Loads the dependencies.
        /// </summary>
        /// <param name="asm">The asm.</param>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <exception cref="System.Exception">Could not locate Assembly [  + assemblyLocation +  ]</exception>
        private void LoadDepencencies(Assembly asm, string assemblyLocation)
        {
            // load dependencies ;)
            if(asm != null)
            {
                var toLoadAsm = asm.GetReferencedAssemblies();

                foreach(var toLoad in toLoadAsm)
                {
                    // TODO : Detect GAC or File System Load ;)
                    try
                    {
                        Assembly.Load(toLoad);
                    }
                    catch
                    {
                        var path = Path.GetDirectoryName(assemblyLocation);
                        if(path != null)
                        {
                            var myLoad = Path.Combine(path, toLoad.Name + ".dll");
                            Assembly.UnsafeLoadFrom(myLoad);
                        }
                    }
                }
            }
            else
            {
                throw new Exception("Could not locate Assembly [ " + assemblyLocation + " ]");
            }
        }
    }
}

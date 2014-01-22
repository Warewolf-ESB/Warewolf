using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dev2.Common;
using Dev2.Runtime.ServiceModel.Data;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers
{
    public interface IPluginBroker
    {
        NamespaceList GetNamespaces(PluginSource pluginSource);
        List<NamespaceItem> ReadNamespaces(string assemblyLocation, string assemblyName);
        ServiceMethodList GetMethods(string assemblyLocation, string assemblyName, string fullName);
        IOutputDescription TestPlugin(PluginService pluginService);
        bool ValidatePlugin(string toLoad, out string error);
    }

    /// <summary>
    /// Handle interaction with plugins ;)
    /// </summary>
    public class PluginBroker : IPluginBroker
    {
        /// <summary>
        /// Gets the namespaces.
        /// </summary>
        /// <param name="pluginSource">The plugin source.</param>
        /// <returns></returns>
        public NamespaceList GetNamespaces(PluginSource pluginSource)
        {
            // BUG 9500 - 2013.05.31 - TWR : added check to avoid nulling AssemblyLocation/Name in tests 
            if (string.IsNullOrEmpty(pluginSource.AssemblyLocation))
            {
                pluginSource = new PluginSources().Get(pluginSource.ResourceID.ToString(), Guid.Empty, Guid.Empty);
            }
            var interrogatePlugin = ReadNamespaces(pluginSource.AssemblyLocation, pluginSource.AssemblyName);
            var namespacelist = new NamespaceList();
            namespacelist.AddRange(interrogatePlugin);
            return namespacelist;
        }

        /// <summary>
        /// Reads the namespaces.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        public List<NamespaceItem> ReadNamespaces(string assemblyLocation, string assemblyName)
        {
            AppDomain tmpDomain = AppDomain.CreateDomain("FindNamespaces");
            var result = new List<NamespaceItem>();
            var list = GetDetail(assemblyLocation, assemblyName).ToList();
            list.ForEach(fullName =>
                result.Add(new NamespaceItem
                {
                    AssemblyLocation = assemblyLocation,
                    AssemblyName = assemblyName,
                    FullName = fullName
                }));

            AppDomain.Unload(tmpDomain);

            return result;
        }

        /// <summary>
        /// Gets the methods.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        public ServiceMethodList GetMethods(string assemblyLocation, string assemblyName, string fullName)
        {
            Assembly assembly;
            var serviceMethodList = new ServiceMethodList();
            if (TryLoadAssembly(assemblyLocation, assemblyName, out assembly))
            {
                var type = assembly.GetType(fullName);
                var methodInfos = type.GetMethods();

                methodInfos.ToList().ForEach(info =>
                {
                    var serviceMethod = new ServiceMethod();
                    serviceMethod.Name = info.Name;
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

        public bool ValidatePlugin(string toLoad, out string error)
        {
            bool result = true;
            error = null;

            if (toLoad.StartsWith(GlobalConstants.GACPrefix))
            {
                try
                {
                    var readlLoad = toLoad.Remove(0, GlobalConstants.GACPrefix.Length);
                    Assembly.Load(readlLoad);
                }
                catch (Exception e)
                {
                    result = false;
                    this.LogError(e);
                    error = e.Message;
                }
            }
            else if (toLoad.EndsWith(".dll"))
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
                    catch (Exception e)
                    {
                        result = false;
                        this.LogError(e);
                        error = e.Message;
                    }
                }
            }
            else
            {
                //does not start with gac prefix or end with .dll
                result = false;
                error = "Not a Dll file";
            }

            return result;
        }

        /// <summary>
        /// Gets the detail.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        private IEnumerable<string> GetDetail(string assemblyLocation, string assemblyName)
        {
            Assembly loadedAssembly;
            IEnumerable<string> namespaces = new string[0];
            if (TryLoadAssembly(assemblyLocation, assemblyName, out loadedAssembly))
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
        /// Loads the depencencies.
        /// </summary>
        /// <param name="asm">The asm.</param>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <exception cref="System.Exception">Could not locate Assembly [  + assemblyLocation +  ]</exception>
        private void LoadDepencencies(Assembly asm, string assemblyLocation)
        {
            // load depencencies ;)
            if (asm != null)
            {
                var toLoadAsm = asm.GetReferencedAssemblies();

                foreach (var toLoad in toLoadAsm)
                {
                    // TODO : Detect GAC or File System Load ;)
                    try
                    {
                        Assembly.Load(toLoad);
                    }
                    catch
                    {
                        var path = Path.GetDirectoryName(assemblyLocation);
                        var myLoad = Path.Combine(path, toLoad.Name + ".dll");
                        Assembly.UnsafeLoadFrom(myLoad);
                    }
                }
            }
            else
            {
                throw new Exception("Could not locate Assembly [ " + assemblyLocation + " ]");
            }
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
            // System.Diagnostics.SymbolStore.SymVariable - NAME
            // GAC:ISymWrapper - LOCATION

            loadedAssembly = null;
            if (assemblyLocation.StartsWith(GlobalConstants.GACPrefix))
            {

                // Culture=neutral, PublicKeyToken=b77a5c561934e089
                // , Version=2.0.0.0
                try
                {
                    loadedAssembly = Assembly.Load(assemblyName);
                    LoadDepencencies(loadedAssembly, assemblyLocation);
                    return true;
                }
                catch (Exception e)
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
                    catch (Exception e)
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
                catch (Exception e)
                {
                    this.LogError(e);
                }
            }
            return false;
        }

        /// <summary>
        /// Tests the plugin.
        /// </summary>
        /// <param name="pluginService">The plugin service.</param>
        /// <returns></returns>
        public IOutputDescription TestPlugin(PluginService pluginService)
        {
            var assemblyLocation = ((PluginSource)pluginService.Source).AssemblyLocation;
            var assemblyName = ((PluginSource)pluginService.Source).AssemblyName;
            var method = pluginService.Method.Name;
            var fullName = pluginService.Namespace;
            var parameters = BuildParameterList(pluginService.Method.Parameters);
            var typeList = BuildTypeList(pluginService.Method.Parameters);

            // BUG 9626 - 2013.06.11 - TWR: refactored
            var pluginResult = TryInvoke(assemblyLocation, assemblyName, fullName, method, typeList, parameters);
            return TestPluginResult(pluginResult);
        }

        #region TestPluginResult

        // BUG 9626 - 2013.06.11 - TWR: refactored
        public IOutputDescription TestPluginResult(object pluginResult)
        {
            var dataBrowser = DataBrowserFactory.CreateDataBrowser();
            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();

            if (pluginResult != null)
            {
                var tmpData = dataBrowser.Map(pluginResult);
                dataSourceShape.Paths.AddRange(tmpData);
            }

            var result = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            result.DataSourceShapes.Add(dataSourceShape);

            return result;
        }

        #endregion


        #region TryInvoke

        // BUG 9626 - 2013.06.11 - TWR: refactored
        object TryInvoke(string assemblyLocation, string assemblyName, string fullName, string method, Type[] typeList, object[] parameters)
        {
            Assembly loadedAssembly;

            if (!TryLoadAssembly(assemblyLocation, assemblyName, out loadedAssembly))
            {
                return null;
            }

            var type = loadedAssembly.GetType(fullName);
            var methodToRun = type.GetMethod(method, typeList);
            var instance = Activator.CreateInstance(type);
            var pluginResult = methodToRun.Invoke(instance, parameters);

            return pluginResult;
        }

        #endregion


        /// <summary>
        /// Builds the parameter list.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private object[] BuildParameterList(List<MethodParameter> parameters)
        {

            if (parameters.Count == 0) return new object[] { };
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

            if (parameters.Count == 0) return new Type[] { };
            var typeList = new Type[parameters.Count];
            int pos = 0;
            parameters.ForEach(parameter =>
            {
                typeList[pos] = parameter.Type;
                pos++;
            });
            return typeList;
        }



    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Xml.Linq;
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
    }

    public class PluginBroker : IPluginBroker
    {
        public NamespaceList GetNamespaces(PluginSource pluginSource)
        {
            pluginSource = new PluginSources().Get(pluginSource.ResourceID.ToString(),Guid.Empty,Guid.Empty);
            var interrogatePlugin = ReadNamespaces(pluginSource.AssemblyLocation, pluginSource.AssemblyName);
            var namespacelist = new NamespaceList();
            namespacelist.AddRange(interrogatePlugin);
            return namespacelist;
        }

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

        public ServiceMethodList GetMethods(string assemblyLocation, string assemblyName,string fullName)
        {
            Assembly assembly;
            var serviceMethodList = new ServiceMethodList();
            if(TryLoadAssembly(assemblyLocation, assemblyName, out assembly))
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
                                IsRequired =true,
                                Name = parameterInfo.Name,
                                Type = parameterInfo.ParameterType
                            }));
                    serviceMethodList.Add(serviceMethod);
                });
            }
            return serviceMethodList;
        }

        private IEnumerable<string> GetDetail(string assemblyLocation, string assemblyName)
        {
            Assembly loadedAssembly;
            IEnumerable<string> namespaces = new string[0];
            if(TryLoadAssembly(assemblyLocation, assemblyName, out loadedAssembly))
            {
                namespaces = loadedAssembly.GetTypes()
                                         .Select(t => t.FullName)
                                         .Distinct();
            }
            return namespaces;
        }

        private bool TryLoadAssembly(string assemblyLocation, string assemblyName, out Assembly loadedAssembly)
        {
            object loadedObject = null;
            loadedAssembly = null;
            if(assemblyLocation.StartsWith("GAC:"))
            {
                Type t = Type.GetType(assemblyName);
                loadedObject = Activator.CreateInstance(t);
                loadedAssembly = Assembly.GetAssembly(loadedObject.GetType());
            }
            else
            {
                try
                {
                    loadedAssembly = Assembly.LoadFile(assemblyLocation);
                    return true;
                }
                catch(Exception)
                {
                }
                try
                {
                    var objHAndle = Activator.CreateInstanceFrom(assemblyLocation, assemblyName);
                    loadedObject = objHAndle.Unwrap();
                    loadedAssembly = Assembly.GetAssembly(loadedObject.GetType());
                    return true;
                }
                catch(Exception e)
                {
                }
            }
            return false;
        }

        public IOutputDescription TestPlugin(PluginService pluginService)
        {
            var assemblyLocation = pluginService.Source.AssemblyLocation;
            var assemblyName = pluginService.Source.AssemblyName;
            var method = pluginService.Method.Name;
            var fullName = pluginService.Source.FullName;
            var parameters = BuildParameterList(pluginService.Method.Parameters);
            var typeList = BuildTypeList(pluginService.Method.Parameters);

            IOutputDescription result = null;
            IDataSourceShape dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            try
            {
                result = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
                dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
                result.DataSourceShapes.Add(dataSourceShape);
                IDataBrowser dataBrowser = DataBrowserFactory.CreateDataBrowser();
                IList<Dev2TypeConversion> convertedArgs = null;
                ObjectHandle objHAndle = null;
                Assembly loadedAssembly;

                if(!TryLoadAssembly(assemblyLocation, assemblyName, out loadedAssembly)) return null;

                MethodInfo methodToRun = null;
                object pluginResult = null;

                var type = loadedAssembly.GetType(fullName);
                methodToRun = type.GetMethod(method,typeList);
                var instance = Activator.CreateInstance(type);
                pluginResult = methodToRun.Invoke(instance, parameters);

                dataSourceShape.Paths.AddRange(dataBrowser.Map(pluginResult));

            }
            catch (Exception ex)
            {
                IDataBrowser dataBrowser = DataBrowserFactory.CreateDataBrowser();
                XElement errorResult = new XElement("Error");
                errorResult.Add(ex);
                var data = errorResult.ToString();
                dataSourceShape.Paths.AddRange(dataBrowser.Map(data));
            }
            return result;
        }

        private object[] BuildParameterList(List<MethodParameter> parameters)
        {
            
            if(parameters.Count == 0) return new object[]{};
            var parameterValues = new object[parameters.Count];
            int pos = 0;
            parameters.ForEach(parameter =>
            {
                parameterValues[pos] = Convert.ChangeType(parameter.Value,parameter.Type);
                pos++;
            });
            return parameterValues;
        } 
        
        private Type[] BuildTypeList(List<MethodParameter> parameters)
        {
            
            if(parameters.Count == 0) return new Type[]{};
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
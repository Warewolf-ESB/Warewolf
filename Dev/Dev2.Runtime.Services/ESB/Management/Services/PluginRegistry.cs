using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Reflection;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.PathOperations;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find all registered plugins
    /// </summary>
    public class PluginRegistry
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            try
            {

        
            string asmLoc;
            string protectionLevel;
            string nameSpace;
            string methodName;

            values.TryGetValue("AssemblyLocation", out asmLoc);
            values.TryGetValue("ProtectionLevel", out protectionLevel);
            values.TryGetValue("NameSpace", out nameSpace);
            values.TryGetValue("MethodName", out methodName);


            if(string.IsNullOrEmpty(asmLoc) || string.IsNullOrEmpty(nameSpace) || string.IsNullOrEmpty(methodName))
            {
                throw new InvalidDataContractException("AssemblyLoation or NameSpace or MethodName is missing");
            }

            var pluginData = new StringBuilder();

            asmLoc = asmLoc.Replace(@"//", "/");

            // new app domain to avoid security concerns resulting from blinding loading code into Server's space
            AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
            IEnumerable<string> plugins = null;

            AppDomain pluginDomain = AppDomain.CreateDomain("PluginMetaDataDiscoveryDomain", null, setup);

            string baseLocation;
            string gacQualifiedName = String.Empty;

            if(asmLoc == string.Empty || asmLoc.StartsWith("Plugins"))
            {
                setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory + @"Plugins\";

                baseLocation = @"Plugins\";

                plugins = asmLoc == string.Empty ? Directory.EnumerateFiles(pluginDomain.BaseDirectory) : new[] { pluginDomain.BaseDirectory + asmLoc.Replace("/", @"\") };
            }
            else
            {
                if(asmLoc.StartsWith(GlobalConstants.GACPrefix))
                {
                    baseLocation = GlobalConstants.GACPrefix;
                    // we have a plugin loaded into the global assembly cache
                    gacQualifiedName = asmLoc.Substring(4);
                }
                else
                {
                    baseLocation = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(asmLoc);
                    // we have a plugin relative to the file system
                    plugins = new[] { asmLoc };
                }
            }

            const bool IncludePublic = true;
            bool includePrivate = true;

            // default to all if no params
            if(protectionLevel != string.Empty)
            {
                // only include public methods
                if(protectionLevel != null && protectionLevel.ToLower() == "public")
                {
                    includePrivate = false;
                }
            }

            if(plugins != null)
            {
                plugins
                    .ToList()
                    .ForEach(plugin =>
                    {
                        int pos = plugin.LastIndexOf(@"\", StringComparison.Ordinal);
                        pos += 1;
                        string shortName = plugin.Substring(pos, (plugin.Length - pos));

                        // only attempt to load assemblies
                        if(shortName.EndsWith(".dll"))
                        {
                            try
                            {
                                Assembly asm = Assembly.LoadFrom(plugin);

                                // only include matching references
                                InterogatePluginAssembly(pluginData, asm, shortName, baseLocation + shortName,
                                                         IncludePublic, includePrivate, methodName, nameSpace);

                                // remove the plugin
                                try
                                {
                                    Assembly.UnsafeLoadFrom(plugin);
                                }
                                catch(Exception ex)
                                {
                                    Dev2Logger.Log.Error(ex);
                                }
                            }
                            catch(Exception ex)
                            {
                                Dev2Logger.Log.Error(ex);
                                pluginData.Append("<Dev2Plugin><Dev2PluginName>" + shortName + "</Dev2PluginName>");
                                pluginData.Append(
                                    "<Dev2PluginStatus>Error</Dev2PluginStatus><Dev2PluginStatusMessage>");
                                pluginData.Append(ex.Message + "</Dev2PluginStatusMessage>");
                                pluginData.Append("<Dev2PluginSourceNameSpace></Dev2PluginSourceNameSpace>");
                                pluginData.Append("<Dev2PluginSourceLocation>" + baseLocation + shortName +
                                                  "</Dev2PluginSourceLocation>");
                                pluginData.Append("<Dev2PluginExposedMethod></Dev2PluginExposedMethod>");
                                pluginData.Append("</Dev2Plugin>");
                            }
                        }
                    });
            }
            else if(!String.IsNullOrEmpty(gacQualifiedName))
            {
                GACAssemblyName gacName = GAC.TryResolveGACAssembly(gacQualifiedName);

                if(gacName == null)
                    if(GAC.RebuildGACAssemblyCache(true))
                        gacName = GAC.TryResolveGACAssembly(gacQualifiedName);

                if(gacName != null)
                {
                    try
                    {
                        Assembly asm = Assembly.Load(gacName.ToString());
                        InterogatePluginAssembly(pluginData, asm, gacName.Name, baseLocation + gacName, IncludePublic,
                                                 includePrivate, methodName, nameSpace);
                    }
                    catch(Exception ex)
                    {
                        Dev2Logger.Log.Error(ex);
                        pluginData.Append("<Dev2Plugin><Dev2PluginName>" + gacName.Name + "</Dev2PluginName>");
                        pluginData.Append("<Dev2PluginStatus>Error</Dev2PluginStatus><Dev2PluginStatusMessage>");
                        pluginData.Append(ex.Message + "</Dev2PluginStatusMessage>");
                        pluginData.Append("<Dev2PluginSourceNameSpace></Dev2PluginSourceNameSpace>");
                        pluginData.Append("<Dev2PluginSourceLocation>" + baseLocation + gacName +
                                          "</Dev2PluginSourceLocation>");
                        pluginData.Append("<Dev2PluginExposedMethod></Dev2PluginExposedMethod>");
                        pluginData.Append("</Dev2Plugin>");
                    }
                }
            }

            AppDomain.Unload(pluginDomain);

            string theResult = "<Dev2PluginRegistration>" + pluginData + "</Dev2PluginRegistration>";

            return theResult;
            }
            catch (Exception e)
            {
                Dev2Logger.Log.Error(e);
                throw;
            }
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService pluginMetaDataService = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><AssemblyLocation ColumnIODirection=\"Input\"/><ProtectionLevel ColumnIODirection=\"Input\"/><NameSpace ColumnIODirection=\"Input\"/><MethodName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };

            ServiceAction pluginMetaDataAction = new ServiceAction { Name = HandlesType(), SourceMethod = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService };

            pluginMetaDataService.Actions.Add(pluginMetaDataAction);

            return pluginMetaDataService;
        }

        public string HandlesType()
        {
            return "PluginRegistryService";
        }

        #region Private Methods
        private void InterogatePluginAssembly(StringBuilder pluginData, Assembly asm, string shortName,
                                                     string sourceLocation, bool includePublic, bool includePrivate,
                                                     string methodName, string nameSpace)
        {
            Type[] types = asm.GetTypes();

            int pos = 0;
            bool found = false;
            bool defaultNameSpace = nameSpace == string.Empty;
            // take all namespaces 

            while(pos < types.Length && !found)
            {
                Type t = types[pos];
                string classString = t.FullName;
                // ensure no funny xml fragments are present

                if(classString.IndexOf("<", StringComparison.Ordinal) < 0 && (defaultNameSpace || (classString == nameSpace)))
                {
                    var exposedMethodsXml = new StringBuilder();

                    MethodInfo[] methods = t.GetMethods();

                    IList<string> exposedMethods = new List<string>();
                    IList<string> methodSignatures = new List<string>();

                    int pos1 = 0;
                    while(pos1 < methods.Length && !found)
                    {
                        MethodInfo m = methods[pos1];

                        if(m.IsPublic && includePublic)
                        {
                            if(!exposedMethods.Contains(m.Name) && methodName == string.Empty)
                            {
                                exposedMethods.Add(m.Name);
                            }
                            else if(methodName == m.Name)
                            {
                                exposedMethods.Add(m.Name);
                                methodSignatures.Add(BuildMethodSignature(m.GetParameters(), m.Name));
                                found = true;
                            }
                        }
                        else if(m.IsPrivate && includePrivate)
                        {
                            if(!exposedMethods.Contains(m.Name) && methodName == string.Empty)
                            {
                                exposedMethods.Add(m.Name);
                            }
                            else if(methodName == m.Name)
                            {
                                exposedMethods.Add(m.Name);
                                methodSignatures.Add(BuildMethodSignature(m.GetParameters(), m.Name));
                                found = true;
                            }
                        }

                        pos1++;
                    }

                    exposedMethods.ToList().Sort((x, y) => String.Compare(x.ToLower(), y.ToLower(), StringComparison.Ordinal));

                    foreach(string m in exposedMethods)
                    {
                        exposedMethodsXml = exposedMethodsXml.Append("<Dev2PluginExposedMethod>");
                        exposedMethodsXml = exposedMethodsXml.Append(m);
                        exposedMethodsXml = exposedMethodsXml.Append("</Dev2PluginExposedMethod>");
                    }

                    var methodSigsXml = new StringBuilder();

                    foreach(string ms in methodSignatures)
                    {
                        methodSigsXml.Append(ms);
                    }

                    if(!classString.Contains("+"))
                    {
                        pluginData.Append("<Dev2Plugin><Dev2PluginName>" + shortName + "</Dev2PluginName>");
                        pluginData.Append("<Dev2PluginStatus>Registered</Dev2PluginStatus>");
                        pluginData.Append("<Dev2PluginSourceNameSpace>" + classString + "</Dev2PluginSourceNameSpace>");
                        pluginData.Append("<Dev2PluginSourceLocation>" + sourceLocation + "</Dev2PluginSourceLocation>");
                        pluginData.Append(exposedMethodsXml);
                        pluginData.Append("<Dev2PluginSourceExposedMethodSignatures>");
                        if(methodSignatures.Count > 0)
                        {
                            pluginData.Append(methodSigsXml);
                        }
                        pluginData.Append("</Dev2PluginSourceExposedMethodSignatures>");
                        pluginData.Append("</Dev2Plugin>");
                    }
                }

                pos++;
            }
        }

        /// <summary>
        /// Builds the method signature.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
// ReSharper disable ParameterTypeCanBeEnumerable.Local
        private string BuildMethodSignature(ParameterInfo[] args, string methodName)
// ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            // add method signature as well ;)
            var toAdd = new StringBuilder();
            toAdd.Append("<Dev2PluginExposedSignature>");
            toAdd.Append("<Dev2PluginMethod>");
            toAdd.Append(methodName);
            toAdd.Append("</Dev2PluginMethod>");

            foreach(ParameterInfo p in args)
            {
                string t = p.ParameterType.Name;
                string name = p.Name;
                toAdd.Append("<Dev2PluginArg>");
                if(!t.Contains("<"))
                {
                    t = t.Replace("`", "");
                    var r = new Regex("(?<!\\.[0-9a-z]*)[0-9]");
                    t = r.Replace(t, "");

                    toAdd.Append(t + " : " + name);
                }
                toAdd.Append("</Dev2PluginArg>");
            }

            toAdd.Append("</Dev2PluginExposedSignature>");

            return toAdd.ToString();
        }

        #endregion
    }
}

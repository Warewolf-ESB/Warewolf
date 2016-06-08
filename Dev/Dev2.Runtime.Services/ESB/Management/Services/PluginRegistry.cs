
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find all registered plugins
    /// </summary>
    public class PluginRegistry
    {
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

                if(classString.IndexOf("<", StringComparison.Ordinal) < 0 && (defaultNameSpace || classString == nameSpace))
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

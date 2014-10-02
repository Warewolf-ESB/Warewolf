
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CSharp;

namespace HttpFramework.Templates
{
    /// <summary>
    /// Simple template engine.
    /// This engine turns templates into C# code and compiles them at runtime
    /// to .NET assemblies. This makes the templates blazing fast. The templates
    /// are only recompiled if they have been changed on disk.
    /// </summary>
    public class SmartyEngine : TemplateEngine
    {
        private enum TemplateState
        {
            Text = 0,
            If,
            Foreach
        }

        private class TemplateInfo
        {
            public string FileName;
            public DateTime Changed;
            public object CompiledTemplate;
        }

        private IDictionary<string, TemplateInfo> compiledTemplates = new Dictionary<string, TemplateInfo>();

        /// <summary>
        /// Compiles a Smarty-style template file into HTML output
        /// </summary>
        /// <param name="fileName">Path to the Smarty template to compile</param>
        /// <param name="variables">Key/value collection of template variable names and values</param>
        /// <returns>A compiled HTML document</returns>
        public string Render(string fileName, IDictionary<string, object> variables)
        {
            TemplateInfo template = null;
            TemplateInfo ti;

            lock (compiledTemplates)
            {
                if (compiledTemplates.TryGetValue(fileName, out ti))
                {
                    if (ti.Changed < File.GetLastWriteTimeUtc(fileName))
                    {
                        // Template has changed, reload it
                        object templateClass = BuildTemplate(fileName, variables);
                        if (templateClass == null)
                            return null;

                        ti.CompiledTemplate = templateClass;
                        ti.Changed = DateTime.UtcNow;
                    }

                    template = ti;
                }
                else
                {
                    // Template has not been loaded yet, load it
                    object templateClass = BuildTemplate(fileName, variables);
                    if (templateClass == null)
                        return null;

                    template = new TemplateInfo();
                    template.CompiledTemplate = templateClass;
                    template.FileName = fileName;
                    template.Changed = DateTime.UtcNow;

                    compiledTemplates.Add(fileName, template);
                }
            }

            object o = template.CompiledTemplate;
            return (string)o.GetType().InvokeMember("RunTemplate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod,
                null, o, new object[] { variables });
        }

        private object BuildTemplate(string fileName, IDictionary<string, object> variables)
        {
            string text = File.ReadAllText(fileName);
            StringBuilder sb = new StringBuilder(text.Length * 2);

            IList<string> assemblies = new List<string>();
            IList<string> namespaces = new List<string>();

            AddNamespace(namespaces, typeof(IDictionary<string, object>));

            foreach (object obj in variables.Values)
            {
                AddAssembly(assemblies, obj.GetType());
                AddNamespace(namespaces, obj.GetType());
            }

            foreach (string s in namespaces)
                sb.AppendLine("using " + s + ";");

            sb.AppendLine("namespace Templates {");
            sb.AppendLine("class TemplateClass {");

            foreach (KeyValuePair<string, object> variable in variables)
                sb.AppendLine(GetTypeName(variable.Value.GetType()) + " " + variable.Key + ";");

            sb.AppendLine("public string RunTemplate(IDictionary<string, object> variables) {");
            sb.AppendLine("System.Text.StringBuilder sb = new System.Text.StringBuilder();");

            foreach (KeyValuePair<string, object> variable in variables)
                sb.AppendLine("this." + variable.Key + " = (" + GetTypeName(variable.Value.GetType()) + ")variables[\"" + variable.Key + "\"];");

            // HACK: Mono treats unreferenced variables as errors during the compile, so we need to complete this pointless little exercise
            // in case some of the variables are not used in the template
            foreach (string variableName in variables.Keys)
                sb.AppendLine("this." + variableName + ".ToString();");

            //for (int i = 1; i < args.Length; i += 2)
            //    sb.Append("this." + args[i - 1] + " = (" + GetTypeName(args[i].GetType()) + ")args[" + i + "];\r\n");

            BuildTemplateString(sb, text, variables);
            sb.AppendLine();
            sb.AppendLine("return sb.ToString(); }}}");

            return CompileTemplate(assemblies, sb);
        }

        private void AddNamespace(IList<string> namespaces, Type type)
        {
            string ns = type.Namespace;
            bool found = false;
            foreach (string s in namespaces)
            {
                if (s.Equals(ns, StringComparison.InvariantCultureIgnoreCase))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                namespaces.Add(ns);

            foreach (Type argument in type.GetGenericArguments())
                AddNamespace(namespaces, argument);
        }

        private string GetArrayType(Type array)
        {
            if (array.IsArray)
            {
                return array.GetElementType().FullName;
            }
            else if (array.IsGenericType)
            {
                Type[] genericTypes = array.GetGenericArguments();
                if (genericTypes.Length > 0)
                    return genericTypes[0].FullName;
            }

            return "object";
        }

        private void AddAssembly(IList<string> assemblies, Type type)
        {
            if (type.Assembly.ManifestModule.Name != "System.dll")
            {
                string path = type.Assembly.Location;
                bool found = false;
                foreach (string s in assemblies)
                {
                    if (s.Equals(path, StringComparison.InvariantCultureIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    assemblies.Add(path);
            }

            foreach (Type argument in type.GetGenericArguments())
                AddAssembly(assemblies, argument);
        }

        private string BuildTemplateString(StringBuilder sb, string data, IDictionary<string, object> variables)
        {
            int lastStartPos = 0;
            int startTagPos;
            int endPos = 0;

            Stack<TemplateState> stateStack = new Stack<TemplateState>();
            stateStack.Push(TemplateState.Text);

            sb.Append("sb.Append(@\"");

            while ((startTagPos = data.IndexOf("{", endPos)) != -1)
            {
                endPos = data.IndexOf("}", startTagPos);
                // Handle { ... {$var} ... }
                int nextStartPos = data.IndexOf("{", startTagPos + 1);
                if (nextStartPos != -1 && nextStartPos < endPos)
                    endPos = data.IndexOf("{", startTagPos + 1);

                if (data[startTagPos + 1] == '$' ||
                    data.Substring(startTagPos, 5) == "{if $" ||
                    data.Substring(startTagPos, 6) == "{else}" ||
                    data.Substring(startTagPos, 5) == "{/if}" ||
                    data.Substring(startTagPos, 15) == "{foreach from=$" ||
                    data.Substring(startTagPos, 10) == "{/foreach}")
                {
                    //sb.Append("sb.Append(@\"");
                    sb.Append(data.Substring(lastStartPos, startTagPos - lastStartPos).Replace("\"", "\"\""));
                    sb.AppendLine("\");");

                    if (data[startTagPos + 1] == '$')
                    {
                        // Variable, {$var}
                        sb.AppendLine("sb.Append(" + data.Substring(startTagPos + 2, endPos - startTagPos - 2) + ");");
                    }
                    else if (data.Substring(startTagPos, 5) == "{if $")
                    {
                        // If statement, {if $var}
                        stateStack.Push(TemplateState.If);
                        sb.AppendLine("if (" + data.Substring(startTagPos + 5, endPos - startTagPos - 5).Replace("$", "") + ") {");
                    }
                    else if (data.Substring(startTagPos, 6) == "{else}")
                    {
                        // Else statement, {else}
                        sb.AppendLine("} else {");
                    }
                    else if (data.Substring(startTagPos, 5) == "{/if}")
                    {
                        // Closing if statement, {/if}
                        sb.AppendLine("}");
                        stateStack.Pop();
                    }
                    else if (data.Substring(startTagPos, 15) == "{foreach from=$")
                    {
                        // Foreach statement, {foreach from=$array item=foo}
                        stateStack.Push(TemplateState.Foreach);

                        string substring = data.Substring(startTagPos + 15, endPos - startTagPos - 15);
                        string array, item, objectType = null;
                        
                        // Get the array name
                        int fromLen = substring.IndexOf(' ');
                        if (fromLen < 0)
                            fromLen = substring.Length;
                        array = substring.Substring(0, fromLen);

                        // Get the item name
                        int itemStartPos = substring.IndexOf("item=");
                        if (itemStartPos >= 0)
                        {
                            int itemLen = substring.IndexOf(' ', itemStartPos);
                            if (itemLen < 0)
                                itemLen = substring.Length - itemStartPos - 5;

                            itemStartPos += 5;
                            item = substring.Substring(itemStartPos, itemLen);
                        }
                        else
                        {
                            item = "value";
                        }

                        // Get the object type
                        if (variables.ContainsKey(array))
                            objectType = GetArrayType(variables[array].GetType());

                        if (objectType == null)
                            objectType = "object";

                        sb.AppendLine("foreach (" + objectType + " " + item + " in " + array + ") {");
                    }
                    else if (data.Substring(startTagPos, 10) == "{/foreach}")
                    {
                        // Closing foreach statement, {/foreach}
                        sb.AppendLine("}");
                        stateStack.Pop();
                    }

                    sb.Append("sb.Append(@\"");
                    lastStartPos = endPos + 1;
                }
            }

            sb.Append(data.Substring(lastStartPos).Replace("\"", "\"\""));
            sb.AppendLine("\");");
            return sb.ToString();
        }

        private static object CompileTemplate(IList<string> assemblies, StringBuilder classText)
        {
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;
            parameters.TreatWarningsAsErrors = false;
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            foreach (string assembly in assemblies)
                parameters.ReferencedAssemblies.Add(ResolveAssemblyPath(assembly));

            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, classText.ToString());
            if (results.Errors.Count > 0)
            {
                string errs = "";

                foreach (CompilerError CompErr in results.Errors)
                {
                    errs += "Template: " + CompErr.FileName + Environment.NewLine +
                        "Line number: " + CompErr.Line + Environment.NewLine +
                        "Error: " + CompErr.ErrorNumber + " '" + CompErr.ErrorText + "'";
                }
                // TODO: Use the logging engine
                Console.WriteLine(errs);
                return null;
            }
            else
            {
                Assembly generatorAssembly = results.CompiledAssembly;
                object classObj = generatorAssembly.CreateInstance("Templates.TemplateClass", false, BindingFlags.CreateInstance, null, null, null, null);
                return classObj;
            }
        }

        private static string ResolveAssemblyPath(string name)
        {
            if (name.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                return name;

            name = name.ToLower();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (IsDynamicAssembly(assembly))
                {
                    continue;
                }

                if (Path.GetFileNameWithoutExtension(assembly.Location).ToLower().Equals(name))
                {
                    return assembly.Location;
                }
            }

            string foo = name.Substring(name.Length - 4, 1);
            if (!(foo.Equals(".")))
                name += ".dll";

            return Path.GetFullPath(name);
        }

        private static bool IsDynamicAssembly(Assembly assembly)
        {
            return assembly.ManifestModule.Name.StartsWith("<");
        }

        /// <summary>
        /// Used to get correct names for generics.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetTypeName(Type type)
        {
            string typeName = type.Name;
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(typeName.Substring(0, typeName.IndexOf('`')));
                sb.Append("<");
                bool first = true;
                foreach (Type genericArgumentType in type.GetGenericArguments())
                {
                    if (!first)
                        sb.Append(", ");
                    first = false;
                    sb.Append(GetTypeName(genericArgumentType));
                }
                sb.Append(">");
                return sb.ToString();
            }
            else
                return typeName;
        }
    }
}

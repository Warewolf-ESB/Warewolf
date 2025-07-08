using Dev2.Activities.WF;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Utilities;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xaml;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Runtime.ESB.WF
{

    public class WorkflowToJsonMapper
    {
        public static void Process(EsbExecuteRequest request)
        {
            var data = request.ExecuteResult;
            var finalresult = new ExecuteMessage();

            if (data != null)
            {
                var executionResult = JsonConvert.DeserializeObject<ExecuteMessage>(data.ToString());

                if (null != executionResult)
                {
                    var requestInfo = JsonConvert.DeserializeObject<Common.X6.X6RequestInfo>(executionResult.Message.ToString());
                    if (requestInfo != null)
                    {
                        var json = MapToJson(requestInfo);

                        finalresult.Message = new StringBuilder(json);
                    }
                    var serializer = new Dev2JsonSerializer();
                    request.ExecuteResult = serializer.SerializeToBuilder(finalresult);
                }
            }
        }

        public static string MapToJson(Common.X6.X6RequestInfo requestInfo)
        {
            var builder = ReadXamlDefinition(requestInfo.ActivityXaml);
            if (builder == null) { return string.Empty; }

            var graph = new WorkflowToX6Converter().ConvertToX6Json(builder, requestInfo.WorkflowXML);
            return graph;
        }

        //private static bool useV3 = false;
        public static ActivityBuilder ReadXamlDefinition(string xaml)
        {
            //if (useV3) return ReadXamlDefinition3(xaml);

            try
            {
                if (!string.IsNullOrEmpty(xaml))
                {
                    // Get the target assembly we want to control
                    var targetAssembly = typeof(DsfFlowDecisionActivity).Assembly;
                    var targetAssemblyName = targetAssembly.GetName().Name;

                    // Create a selective assembly resolve handler
                    ResolveEventHandler selectiveHandler = (sender, args) =>
                    {
                        var requestedAssembly = new AssemblyName(args.Name);

                        // Only intercept requests for our specific assembly
                        if (requestedAssembly.Name == targetAssemblyName)
                        {
                            Console.WriteLine($"Redirecting assembly load for: {args.Name}");
                            return targetAssembly;
                        }

                        // For all other assemblies, let the default resolution happen
                        return null;
                    };

                    AppDomain.CurrentDomain.AssemblyResolve += selectiveHandler;

                    try
                    {
                        using (var sw = new System.IO.StringReader(xaml))
                        {
                            var xamlXmlWriterSettings = new XamlXmlReaderSettings();

                            // Use default XamlSchemaContext to ensure all standard types are available
                            var xw = ActivityXamlServices.CreateBuilderReader(
                                new XamlXmlReader(sw, new XamlSchemaContext(), xamlXmlWriterSettings));
                            var load = XamlServices.Load(xw);
                            return load as ActivityBuilder;
                        }
                    }
                    finally
                    {
                        AppDomain.CurrentDomain.AssemblyResolve -= selectiveHandler;
                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error loading XAML: ", e, GlobalConstants.WarewolfError);
            }
            return null;
        }

        public class SelectiveAssemblyXamlSchemaContext : XamlSchemaContext
        {
            private readonly Dictionary<string, Assembly> _controlledAssemblies;

            public SelectiveAssemblyXamlSchemaContext(Dictionary<string, Assembly> controlledAssemblies)
                : base(controlledAssemblies.Values)
            {
                _controlledAssemblies = controlledAssemblies;
            }

            protected override XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
            {
                // Extract assembly name from xaml namespace if present
                // Format: clr-namespace:Namespace;assembly=AssemblyName
                if (xamlNamespace.Contains("assembly="))
                {
                    var assemblyPart = xamlNamespace.Split(';').FirstOrDefault(p => p.StartsWith("assembly="));
                    if (assemblyPart != null)
                    {
                        var assemblyName = assemblyPart.Replace("assembly=", "");

                        // Check if this is one of our controlled assemblies
                        if (_controlledAssemblies.TryGetValue(assemblyName, out var controlledAssembly))
                        {
                            var namespacePart = xamlNamespace.Split(';')[0].Replace("clr-namespace:", "");
                            var type = controlledAssembly.GetTypes()
                                .FirstOrDefault(t => t.Name == name && t.Namespace == namespacePart);

                            if (type != null)
                            {
                                return GetXamlType(type);
                            }
                        }
                    }
                }

                // Fallback to default behavior for other types
                return base.GetXamlType(xamlNamespace, name, typeArguments);
            }
        }

        public static ActivityBuilder ReadXamlDefinition3(string xaml)
        {
            try
            {
                if (!string.IsNullOrEmpty(xaml))
                {
                    // Get all assemblies currently loaded that might be needed
                    var currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                    // Create a list of assemblies to include
                    var includedAssemblies = new List<Assembly>
                    {
                        // Add your controlled assembly
                        typeof(DsfFlowDecisionActivity).Assembly,

                        // Add essential workflow assemblies
                        typeof(Activity).Assembly,
                        typeof(ActivityBuilder).Assembly,
                        typeof(XamlServices).Assembly
                    };

                    // Add commonly needed assemblies
                    includedAssemblies.AddRange(currentAssemblies.Where(a =>
                        a.GetName().Name.StartsWith("System.Activities") ||
                        a.GetName().Name.StartsWith("System.Xaml") ||
                        a.GetName().Name.StartsWith("System.ComponentModel") ||
                        a.GetName().Name == "mscorlib" ||
                        a.GetName().Name == "System.Private.CoreLib"));

                    // Remove duplicates
                    var uniqueAssemblies = includedAssemblies.Distinct().ToArray();

                    using (var sw = new System.IO.StringReader(xaml))
                    {
                        var xamlXmlWriterSettings = new XamlXmlReaderSettings();
                        var schemaContext = new XamlSchemaContext(uniqueAssemblies);

                        var xw = ActivityXamlServices.CreateBuilderReader(
                            new XamlXmlReader(sw, schemaContext, xamlXmlWriterSettings));
                        var load = XamlServices.Load(xw);
                        return load as ActivityBuilder;
                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error loading XAML: ", e, GlobalConstants.WarewolfError);
            }
            return null;
        }

        public static ActivityBuilder ReadXamlDefinition_old(string xaml)
        {
            try
            {
                if (!string.IsNullOrEmpty(xaml))
                {
                    using (var sw = new System.IO.StringReader(xaml))
                    {
                        var xamlXmlWriterSettings = new XamlXmlReaderSettings();
                        var xw = ActivityXamlServices.CreateBuilderReader(new XamlXmlReader(sw, new XamlSchemaContext(), xamlXmlWriterSettings));
                        var load = XamlServices.Load(xw);
                        return load as ActivityBuilder;
                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error loading XAML: ", e, GlobalConstants.WarewolfError);
            }
            return null;
        }
    }


}

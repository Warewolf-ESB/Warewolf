/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Serialization;

namespace Dev2.DynamicServices
{
    public class WebServiceInvoker
    {
        private readonly Dictionary<string, Type> availableTypes;
        private readonly List<string> services;
        private readonly Assembly webServiceAssembly;

        /// <summary>
        ///     Creates the service invoker using the specified web service.
        /// </summary>
        /// <param name="webServiceUri"></param>
        public WebServiceInvoker(Uri webServiceUri)
        {
            services = new List<string>(); // available services
            availableTypes = new Dictionary<string, Type>(); // available types

            // create an assembly from the web service description
            webServiceAssembly = BuildAssemblyFromWSDL(webServiceUri);

            // see what service types are available
            Type[] types = webServiceAssembly.GetExportedTypes();


            // and save them
            foreach (Type type in types)
            {
                services.Add(type.FullName);
                availableTypes.Add(type.FullName, type);
            }
        }

        /// <summary>
        ///     Text description of the available services within this web service.
        /// </summary>
        public List<string> AvailableServices
        {
            get { return services; }
        }

        /// <summary>
        ///     Gets a list of all methods available for the specified service.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public List<string> EnumerateServiceMethods(string serviceName)
        {
            var methods = new List<string>();

            if (!availableTypes.ContainsKey(serviceName))
                throw new Exception("Service Not Available");

            Type type = availableTypes[serviceName];

            // only find methods of this object type (the one we generated)
            // we don't want inherited members (this type inherited from SoapHttpClientProtocol)
            methods.AddRange(
                type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Select(minfo => minfo.Name));

            return methods;
        }

        /// <summary>
        ///     Invokes the specified method of the named service.
        /// </summary>
        /// <typeparam name="T">The expected return type.</typeparam>
        /// <param name="serviceName">The name of the service to use.</param>
        /// <param name="methodName">The name of the method to call.</param>
        /// <param name="args">The arguments to the method.</param>
        /// <returns>The return value from the web service method.</returns>
        public T InvokeMethod<T>(string serviceName, string methodName, params object[] args)
        {
            // create an instance of the specified service
            // and invoke the method
            object obj = webServiceAssembly.CreateInstance(serviceName);

            if (obj != null)
            {
                Type type = obj.GetType();

                var typedArgs = new List<object>();

                type.GetMethod(methodName).GetParameters().ToList().ForEach(par =>
                {
                    Type paramType = par.ParameterType;
                    if (paramType.IsEnum)
                    {
                        typedArgs.Add(Enum.Parse(paramType, args[0].ToString()));
                    }
                });

                return (T) type.InvokeMember(methodName, BindingFlags.InvokeMethod, null, obj, args);
            }

            return default(T);
        }

        /// <summary>
        ///     Builds the web service description importer, which allows us to generate a proxy class based on the
        ///     content of the WSDL described by the XmlTextReader.
        /// </summary>
        /// <param name="xmlreader">The WSDL content, described by XML.</param>
        /// <returns>A ServiceDescriptionImporter that can be used to create a proxy class.</returns>
        private ServiceDescriptionImporter BuildServiceDescriptionImporter(XmlTextReader xmlreader)
        {
            // make sure xml describes a valid wsdl
            if (!ServiceDescription.CanRead(xmlreader))
                throw new Exception("Invalid Web Service Description");

            // parse wsdl
            ServiceDescription serviceDescription = ServiceDescription.Read(xmlreader);

            // build an importer, that assumes the SOAP protocol, client binding, and generates properties
            var descriptionImporter = new ServiceDescriptionImporter {ProtocolName = "Soap"};
            descriptionImporter.AddServiceDescription(serviceDescription, null, null);
            descriptionImporter.Style = ServiceDescriptionImportStyle.Client;
            descriptionImporter.CodeGenerationOptions = CodeGenerationOptions.GenerateProperties;

            return descriptionImporter;
        }

        /// <summary>
        ///     Compiles an assembly from the proxy class provided by the ServiceDescriptionImporter.
        /// </summary>
        /// <param name="descriptionImporter"></param>
        /// <returns>An assembly that can be used to execute the web service methods.</returns>
        private Assembly CompileAssembly(ServiceDescriptionImporter descriptionImporter)
        {
            // a namespace and compile unit are needed by importer
            var codeNamespace = new CodeNamespace();
            var codeUnit = new CodeCompileUnit();

            codeUnit.Namespaces.Add(codeNamespace);

            ServiceDescriptionImportWarnings importWarnings = descriptionImporter.Import(codeNamespace, codeUnit);

            if (importWarnings == 0) // no warnings
            {
                // create a c# compiler
                CodeDomProvider compiler = CodeDomProvider.CreateProvider("CSharp");

                // include the assembly references needed to compile
                string[] references = {"System.Web.Services.dll", "System.Xml.dll"};

                var parameters = new CompilerParameters(references);

                // compile into assembly
                CompilerResults results = compiler.CompileAssemblyFromDom(parameters, codeUnit);

                if (results.Errors.Cast<CompilerError>().Any())
                {
                    throw new Exception("Compilation Error Creating Assembly");
                }

                // all done....

                return results.CompiledAssembly;
            }

            // warnings issued from importers, something wrong with WSDL
            throw new Exception("Invalid WSDL");
        }

        /// <summary>
        ///     Builds an assembly from a web service description.
        ///     The assembly can be used to execute the web service methods.
        /// </summary>
        /// <param name="webServiceUri">Location of WSDL.</param>
        /// <returns>
        ///     A web service assembly.
        /// </returns>
        /// <exception cref="System.Exception">Web Service Not Found</exception>
        private Assembly BuildAssemblyFromWSDL(Uri webServiceUri)
        {
            if (String.IsNullOrEmpty(webServiceUri.ToString()))
                throw new Exception("Web Service Not Found");

            using (var xmlreader = new XmlTextReader(webServiceUri + "?wsdl"))
            {
                ServiceDescriptionImporter descriptionImporter = BuildServiceDescriptionImporter(xmlreader);

                return CompileAssembly(descriptionImporter);
            }
        }
    }
}
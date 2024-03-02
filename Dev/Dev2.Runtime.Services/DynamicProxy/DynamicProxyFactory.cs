#pragma warning disable

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
#if NETFRAMEWORK
using System.Data.Design;
using System.Web.Services.Discovery;
#else
using System.Diagnostics;
using System.Data.Design;
using System.Linq;
using System.Threading;
using System.Web.Services.Description;
#endif







namespace Dev2.Runtime.DynamicProxy
{
#if !NETFRAMEWORK
    using Binding = System.ServiceModel.Channels.Binding;
#endif
    using WsdlNS = System.Web.Services.Description;


    public class DynamicProxyFactory
    {
#if NETFRAMEWORK
        readonly string wsdlUri;
#else
        readonly string wsdlUrl;
#endif
        readonly DynamicProxyFactoryOptions options;

        CodeCompileUnit codeCompileUnit;
        CodeDomProvider codeDomProvider;
#if NETFRAMEWORK
        ServiceContractGenerator contractGenerator;
        Collection<MetadataSection> metadataCollection;
#endif

        IEnumerable<Binding> bindings;
#if NETFRAMEWORK
        IEnumerable<ContractDescription> contracts;
        ServiceEndpointCollection endpoints;
        IEnumerable<MetadataConversionError> importWarnings;
        IEnumerable<MetadataConversionError> codegenWarnings;
#else
        IEnumerable<System.ServiceModel.Description.ContractDescription> contracts;
        IEnumerable<System.ServiceModel.Description.ServiceEndpoint> endpoints;
#endif
        IEnumerable<CompilerError> compilerWarnings;

        Assembly proxyAssembly;
        string proxyCode;

#if !NETFRAMEWORK
        string fileDirectory = "C:/ProgramData/Warewolf/Temp/WCFReference";
        string fileName = "Reference.cs";
#endif

        public DynamicProxyFactory(string wsdlUri, DynamicProxyFactoryOptions options)
        {
            if (wsdlUri == null)
            {
                throw new ArgumentNullException("wsdlUri");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

#if !NETFRAMEWORK
            if (!wsdlUri.Contains("?wsdl"))
            {
                wsdlUri = wsdlUri + "?wsdl";
            }
#endif

#if NETFRAMEWORK
            this.wsdlUri = wsdlUri;
#else
            this.wsdlUrl = wsdlUri;
#endif
            this.options = options;

#if NETFRAMEWORK
            DownloadMetadata();
            ImportMetadata();
            CreateProxy();
            WriteCode();
#else
            CreateFilePath(fileDirectory);
            ExecuteCommand("dotnet-svcutil " + wsdlUri + " --outputDir " + fileDirectory);
            CreateCodeDomProvider();
            WriteCodeFromFile();
#endif
            CompileProxy();

#if !NETFRAMEWORK
            contracts = GetAllContracts();
            bindings = GetAllBindings();
            endpoints = GetAllEndpoints();
            CleanUpReferenceFiles();
#endif
        }

        public DynamicProxyFactory(string wsdlUri)
            : this(wsdlUri, new DynamicProxyFactoryOptions())
        {
		}
		
#if NETFRAMEWORK
        void DownloadMetadata()
        {
            var epr = new EndpointAddress(wsdlUri);

            var disco = new DiscoveryClientProtocol
            {
                AllowAutoRedirect = true,
                UseDefaultCredentials = true
            };
            disco.DiscoverAny(wsdlUri);
            disco.ResolveAll();

            var results = new Collection<MetadataSection>();
            if (disco.Documents.Values != null)
            {
                foreach (var document in disco.Documents.Values)
                {
                    AddDocumentToResults(document, results);
                }
            }

            metadataCollection = results;
        }

        void AddDocumentToResults(object document, Collection<MetadataSection> results)
        {
            var wsdl = document as WsdlNS.ServiceDescription;
            var schema = document as XmlSchema;
            var xmlDoc = document as XmlElement;

            if (wsdl != null)
            {
                results.Add(MetadataSection.CreateFromServiceDescription(wsdl));
            }
            else if (schema != null)
            {
                results.Add(MetadataSection.CreateFromSchema(schema));
            }
            else if (xmlDoc != null && xmlDoc.LocalName == "Policy")
            {
                results.Add(MetadataSection.CreateFromPolicy(xmlDoc, null));
            }
            else
            {
                var mexDoc = new MetadataSection();
                mexDoc.Metadata = document;
                results.Add(mexDoc);
            }
        }


        void ImportMetadata()
        {
            codeCompileUnit = new CodeCompileUnit();
            CreateCodeDomProvider();

            var importer = new WsdlImporter(new MetadataSet(metadataCollection));
            AddStateForDataContractSerializerImport(importer);
            AddStateForXmlSerializerImport(importer);

            bindings = importer.ImportAllBindings();
            contracts = importer.ImportAllContracts();
            endpoints = importer.ImportAllEndpoints();
            importWarnings = importer.Errors;

            var success = true;
            if (importWarnings != null)
            {
                foreach (var error in importWarnings)
                {
                    if (!error.IsWarning)
                    {
                        success = false;
                        break;
                    }
                }
            }

            if (!success)
            {
                var exception = new DynamicProxyException(
                    Constants.ErrorMessages.ImportError);
                exception.MetadataImportErrors = importWarnings;
                throw exception;
            }
        }

        void AddStateForXmlSerializerImport(WsdlImporter importer)
        {
            var importOptions =
                new XmlSerializerImportOptions(codeCompileUnit);
            importOptions.CodeProvider = codeDomProvider;

            importOptions.WebReferenceOptions = new WsdlNS.WebReferenceOptions();
            importOptions.WebReferenceOptions.CodeGenerationOptions =
                CodeGenerationOptions.GenerateProperties |
                CodeGenerationOptions.GenerateOrder;

            importOptions.WebReferenceOptions.SchemaImporterExtensions.Add(
                typeof(TypedDataSetSchemaImporterExtension).AssemblyQualifiedName);
            importOptions.WebReferenceOptions.SchemaImporterExtensions.Add(
                typeof(DataSetSchemaImporterExtension).AssemblyQualifiedName);

            importer.State.Add(typeof(XmlSerializerImportOptions), importOptions);
        }

        void AddStateForDataContractSerializerImport(WsdlImporter importer)
        {
            var xsdDataContractImporter =
                new XsdDataContractImporter(codeCompileUnit);
            xsdDataContractImporter.Options = new ImportOptions();
            xsdDataContractImporter.Options.ImportXmlType =
                (options.FormatMode ==
                    DynamicProxyFactoryOptions.FormatModeOptions.DataContractSerializer);

            xsdDataContractImporter.Options.CodeProvider = codeDomProvider;
            importer.State.Add(typeof(XsdDataContractImporter),
                    xsdDataContractImporter);

            foreach (var importExtension in importer.WsdlImportExtensions)
            {

                if (importExtension is DataContractSerializerMessageContractImporter dcConverter)
                {
                    dcConverter.Enabled = options.FormatMode ==
                        DynamicProxyFactoryOptions.FormatModeOptions.XmlSerializer ? false : true;
                }

            }
        }

        void CreateProxy()
        {
            CreateServiceContractGenerator();

            foreach (var contract in contracts)
            {
                contractGenerator.GenerateServiceContractType(contract);
            }

            var success = true;
            codegenWarnings = contractGenerator.Errors;
            if (codegenWarnings != null)
            {
                foreach (var error in codegenWarnings)
                {
                    if (!error.IsWarning)
                    {
                        success = false;
                        break;
                    }
                }
            }

            if (!success)
            {
                var exception = new DynamicProxyException(
                 Constants.ErrorMessages.CodeGenerationError);
                exception.CodeGenerationErrors = codegenWarnings;
                throw exception;
            }
        }
#endif


        void CompileProxy()
        {
            // reference the required assemblies with the correct path.
            var compilerParams = new CompilerParameters();

            AddAssemblyReference(
                typeof(ServiceContractAttribute).Assembly,
                compilerParams.ReferencedAssemblies);

            AddAssemblyReference(
                typeof(WsdlNS.ServiceDescription).Assembly,
                compilerParams.ReferencedAssemblies);

            AddAssemblyReference(
                typeof(DataContractAttribute).Assembly,
                compilerParams.ReferencedAssemblies);

            AddAssemblyReference(typeof(XmlElement).Assembly,
                compilerParams.ReferencedAssemblies);

            AddAssemblyReference(typeof(Uri).Assembly,
                compilerParams.ReferencedAssemblies);

            AddAssemblyReference(typeof(DataSet).Assembly,
                compilerParams.ReferencedAssemblies);

            var results =
                codeDomProvider.CompileAssemblyFromSource(
                    compilerParams,
                    proxyCode);

            if ((results.Errors != null) && (results.Errors.HasErrors))
            {
                var exception = new DynamicProxyException(
                    Constants.ErrorMessages.CompilationError);
                exception.CompilationErrors = ToEnumerable(results.Errors);

                throw exception;
            }

            compilerWarnings = ToEnumerable(results.Errors);
            proxyAssembly = Assembly.LoadFile(results.PathToAssembly);
        }
		
#if NETFRAMEWORK
        void WriteCode()
        {
            using (var writer = new StringWriter())
            {
                var codeGenOptions = new CodeGeneratorOptions();
                codeGenOptions.BracingStyle = "C";
                codeDomProvider.GenerateCodeFromCompileUnit(
                        codeCompileUnit, writer, codeGenOptions);
                writer.Flush();
                proxyCode = writer.ToString();
            }

            // use the modified proxy code, if code modifier is set.
            if (options.CodeModifier != null)
            {
                proxyCode = options.CodeModifier(proxyCode);
            }
        }
#endif

        void AddAssemblyReference(Assembly referencedAssembly,
            StringCollection refAssemblies)
        {
            var path = Path.GetFullPath(referencedAssembly.Location);
            var name = Path.GetFileName(path);
            if (!(refAssemblies.Contains(name) ||
                  refAssemblies.Contains(path)))
            {
                refAssemblies.Add(path);
            }
        }


        public ServiceEndpoint GetEndpoint(string contractName,
                string contractNamespace)
        {
            ServiceEndpoint matchingEndpoint = null;

            foreach (var endpoint in Endpoints)
            {
                if (ContractNameMatch(endpoint.Contract, contractName) &&
                    ContractNsMatch(endpoint.Contract, contractNamespace))
                {
                    matchingEndpoint = endpoint;
                    break;
                }
            }

            if (matchingEndpoint == null)
            {
                throw new ArgumentException(string.Format(
                    Constants.ErrorMessages.EndpointNotFound,
                    contractName, contractNamespace));
            }

            return matchingEndpoint;
        }

        bool ContractNameMatch(ContractDescription cDesc, string name) => (string.Compare(cDesc.Name, name, true) == 0);

        bool ContractNsMatch(ContractDescription cDesc, string ns) => ((ns == null) ||
                    (string.Compare(cDesc.Namespace, ns, true) == 0));

        public DynamicProxy CreateProxy(string contractName) => CreateProxy(contractName, null);

        public DynamicProxy CreateProxy(string contractName,
                string contractNamespace)
        {
            var endpoint = GetEndpoint(contractName,
                    contractNamespace);

            return CreateProxy(endpoint);
        }

        public DynamicProxy CreateProxy(ServiceEndpoint endpoint)
        {
            var contractType = GetContractType(endpoint.Contract.Name,
                endpoint.Contract.Namespace);

            var proxyType = GetProxyType(contractType);

            return new DynamicProxy(proxyType, endpoint.Binding,
                    endpoint.Address);
        }

        Type GetContractType(string contractName,
                string contractNamespace)
        {
            var allTypes = proxyAssembly.GetTypes();
            Type contractType = null;
            foreach (var type in allTypes)
            {
                // Is it an interface?
                if (!type.IsInterface)
                {
                    continue;
                }

                // Is it marked with ServiceContract attribute?
                var attrs = type.GetCustomAttributes(
                    typeof(ServiceContractAttribute), false);
                if ((attrs == null) || (attrs.Length == 0))
                {
                    continue;
                }

                // is it the required service contract?
                var scAttr = (ServiceContractAttribute)attrs[0];
                var cName = GetContractName(type, scAttr.Name, scAttr.Namespace);

                if (string.Compare(cName.Name, contractName, true) != 0)
                {
                    continue;
                }

                if (string.Compare(cName.Namespace, contractNamespace,
                            true) != 0)
                {
                    continue;
                }

                contractType = type;
                break;
            }

            if (contractType == null)
            {
                throw new ArgumentException(
                    Constants.ErrorMessages.UnknownContract);
            }

            return contractType;
        }

        internal const string DefaultNamespace = "http://tempuri.org/";
        internal static XmlQualifiedName GetContractName(Type contractType,
            string name, string ns)
        {
            if (String.IsNullOrEmpty(name))
            {
                name = contractType.Name;
            }

            ns = ns == null ? DefaultNamespace : Uri.EscapeUriString(ns);

            return new XmlQualifiedName(name, ns);
        }

        Type GetProxyType(Type contractType)
        {
            var clientBaseType = typeof(ClientBase<>).MakeGenericType(
                    contractType);

            var allTypes = ProxyAssembly.GetTypes();
            Type proxyType = null;

            foreach (var type in allTypes)
            {
                // Look for a proxy class that implements the service 
                // contract and is derived from ClientBase<service contract>
                if (type.IsClass && contractType.IsAssignableFrom(type)
                    && type.IsSubclassOf(clientBaseType))
                {
                    proxyType = type;
                    break;
                }
            }

            if (proxyType == null)
            {
                throw new DynamicProxyException(string.Format(
                            Constants.ErrorMessages.ProxyTypeNotFound,
                            contractType.FullName));
            }

            return proxyType;
        }


        void CreateCodeDomProvider()
        {
            codeDomProvider = CodeDomProvider.CreateProvider(options.Language.ToString());
        }

#if NETFRAMEWORK
        void CreateServiceContractGenerator()
        {
            contractGenerator = new ServiceContractGenerator(
                codeCompileUnit);
            contractGenerator.Options |= ServiceContractGenerationOptions.ClientClass;
        }

        public IEnumerable<MetadataSection> Metadata => metadataCollection;
#endif
        public IEnumerable<Binding> Bindings => bindings;

#if NETFRAMEWORK
        public IEnumerable<ContractDescription> Contracts => contracts;
#else
        public IEnumerable<System.ServiceModel.Description.ContractDescription> Contracts => contracts;
#endif

        public IEnumerable<ServiceEndpoint> Endpoints => endpoints;

        public Assembly ProxyAssembly => proxyAssembly;

        public string ProxyCode => proxyCode;

#if NETFRAMEWORK
        public IEnumerable<MetadataConversionError> MetadataImportWarnings => importWarnings;

        public IEnumerable<MetadataConversionError> CodeGenerationWarnings => codegenWarnings;
#endif
        public IEnumerable<CompilerError> CompilationWarnings => compilerWarnings;


#if NETFRAMEWORK
        public static string ToString(IEnumerable<MetadataConversionError>
#else
        public static string ToString(IEnumerable<CoreWCF.Description.MetadataConversionError>
#endif
            importErrors)
        {
            if (importErrors != null)
            {
                var importErrStr = new StringBuilder();

                foreach (var error in importErrors)
                {
                    importErrStr.AppendLine(error.IsWarning ? "Warning : " + error.Message : "Error : " + error.Message);
                }

                return importErrStr.ToString();
            }
            return string.Empty;
        }

        public static string ToString(IEnumerable<CompilerError> compilerErrors)
        {
            if (compilerErrors != null)
            {
                var builder = new StringBuilder();
                foreach (var error in compilerErrors)
                {
                    builder.AppendLine(error.ToString());
                }

                return builder.ToString();
            }
            return string.Empty;
        }

        static IEnumerable<CompilerError> ToEnumerable(
                CompilerErrorCollection collection)
        {
            if (collection == null)
            {
                return null;
            }

            var errorList = new List<CompilerError>();
            foreach (CompilerError error in collection)
            {
                errorList.Add(error);
            }

            return errorList;
        }

#if !NETFRAMEWORK
        void ExecuteCommand(string Command)
        {
            ProcessStartInfo ProcessInfo;
            Process Process;
            const int minutes = 1;
            const int timeoutInMilliseconds = 1000 * 25 * minutes;
            const int milliSeconds = 1000;

            ProcessInfo = new ProcessStartInfo("cmd.exe", "/K " + Command);
            ProcessInfo.CreateNoWindow = false;
            ProcessInfo.UseShellExecute = false;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Process = Process.Start(ProcessInfo);

            int timeSpent = milliSeconds;
            while (!Process.HasExited && timeSpent <= timeoutInMilliseconds)
            {
                Thread.Sleep(milliSeconds);//Process.WaitForExit(timeSpent);
                timeSpent = timeSpent + milliSeconds;
            }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        void WriteCodeFromFile()
        {
            var codeGenOptions = new CodeGeneratorOptions();
            codeGenOptions.BracingStyle = "C";
            var fileReference = fileDirectory + "/" + fileName;

            if (File.Exists(fileReference))
            {
                // Read entire file content in one string    
                proxyCode = File.ReadAllText(fileReference);
                // Need to replace the private methods with public methods in order access the default binding and defaulf endpoint methods. 
                proxyCode = proxyCode.Replace("private static System", "public static System");
            }
            else
            {
                proxyCode = null;
            }

            // use the modified proxy code, if code modifier is set.
            if (options.CodeModifier != null)
            {
                proxyCode = options.CodeModifier(proxyCode);
            }
        }

        Collection<ContractDescription> GetAllContracts()
        {
            var allTypes = proxyAssembly.GetTypes();
            Type contractType = null;

            Collection<ContractDescription> collection = new Collection<ContractDescription>();

            foreach (var type in allTypes)
            {
                // Is it an interface?
                if (!type.IsInterface)
                {
                    continue;
                }

                XmlTextReader myReader = new XmlTextReader(wsdlUrl);
                if (WsdlNS.ServiceDescription.CanRead(myReader))
                {
                    WsdlNS.ServiceDescription myDescription = WsdlNS.ServiceDescription.Read(myReader);
                    foreach (WsdlNS.PortType portType in myDescription.PortTypes)
                    {
                        contractType = type;
                        ContractDescription contractDescription = ContractDescription.GetContract(contractType);
                        if (contractDescription != null)
                        {
                            collection.Add(contractDescription);
                        }
                    }
                }
                break;

            }

            if (contractType == null)
            {
                CleanUpReferenceFiles();
                throw new ArgumentException(
                    Constants.ErrorMessages.UnknownContract);
            }

            return collection;
        }

        Collection<System.ServiceModel.Channels.Binding> GetAllBindings()
        {
            Collection<System.ServiceModel.Channels.Binding> collection = new Collection<System.ServiceModel.Channels.Binding>();

            var allTypes = proxyAssembly.GetTypes();
            Type contractType = null;

            foreach (var type in allTypes)
            {
                if (!type.IsInterface && type.Name.Contains("Client"))
                {
                    var methods = type.GetMethods();
                    foreach (var method in methods)
                    {
                        if (method.Name == "GetDefaultBinding")
                        {
                            contractType = type;
                            var bindingMethod = method.Invoke(contractType, null);
                            if (bindingMethod != null)
                            {
                                collection.Add((System.ServiceModel.Channels.Binding)bindingMethod);
                            }
                            break;
                        }
                    }

                    //The Reference file does not contain GetDefaultBinding method.
                    if (collection.Count() == 0)
                    {
                        XmlTextReader myReader = new XmlTextReader(wsdlUrl);
                        if (WsdlNS.ServiceDescription.CanRead(myReader))
                        {
                            WsdlNS.ServiceDescription myDescription = WsdlNS.ServiceDescription.Read(myReader);
                            foreach (WsdlNS.Port port in myDescription.Services[0].Ports)
                            {
                                if (port.Binding.Name.Contains("WSHttpBinding"))
                                {
                                    WSHttpBinding binding = new WSHttpBinding();
                                    contractType = type;
                                    collection.Add(binding);
                                }
                                else if (port.Binding.Name.Contains("BasicHttpBinding"))
                                {
                                    BasicHttpBinding binding = new BasicHttpBinding();
                                    contractType = type;
                                    collection.Add(binding);
                                }
                                else if (port.Binding.Name.Contains("CustomBinding"))
                                {
                                    CustomBinding binding = new CustomBinding();
                                    contractType = type;
                                    collection.Add(binding);
                                }
                                else if (port.Binding.Name.Contains("NetHttpBinding"))
                                {
                                    NetHttpBinding binding = new NetHttpBinding();
                                    contractType = type;
                                    collection.Add(binding);
                                }
                                else if (port.Binding.Name.Contains("NetTcpBinding"))
                                {
                                    NetTcpBinding binding = new NetTcpBinding();
                                    contractType = type;
                                    collection.Add(binding);
                                }
                            }
                        }
                    }
                    break;
                }
                else
                {
                    continue;
                }
            }

            if (contractType == null)
            {
                CleanUpReferenceFiles();
                throw new ArgumentException(
                    Constants.ErrorMessages.BindingError);
            }

            return collection;
        }

        Collection<ServiceEndpoint> GetAllEndpoints()
        {
            Collection<ServiceEndpoint> serviceEndpointCollection = new Collection<ServiceEndpoint>();

            var allTypes = proxyAssembly.GetTypes();
            Type contractType = null;

            foreach (var type in allTypes)
            {
                if (!type.IsInterface && type.Name.Contains("Client"))
                {
                    var methods = type.GetMethods();
                    foreach (var method in methods)
                    {
                        if (method.Name == "GetDefaultEndpointAddress")
                        {
                            var endpointMethod = method.Invoke(contractType, null);
                            if (endpointMethod != null)
                            {
                                XmlTextReader myReader = new XmlTextReader(wsdlUrl);
                                if (WsdlNS.ServiceDescription.CanRead(myReader))
                                {
                                    WsdlNS.ServiceDescription myDescription = WsdlNS.ServiceDescription.Read(myReader);
                                    foreach (WsdlNS.Port port in myDescription.Services[0].Ports)
                                    {
                                        contractType = type;
                                        ContractDescription contractDescription = ContractDescription.GetContract(contractType);
                                        if (contractDescription != null)
                                        {
                                            ServiceEndpoint serviceEndpoint = new ServiceEndpoint(contractDescription, bindings.FirstOrDefault(), (EndpointAddress)endpointMethod);
                                            if (serviceEndpoint != null)
                                            {
                                                serviceEndpointCollection.Add(serviceEndpoint);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    }

                    //The Reference file does not contain GetDefaultEndpointAddress method.
                    if (serviceEndpointCollection.Count() == 0)
                    {
                        XmlTextReader myReader = new XmlTextReader(wsdlUrl);
                        if (WsdlNS.ServiceDescription.CanRead(myReader))
                        {
                            WsdlNS.ServiceDescription myDescription = WsdlNS.ServiceDescription.Read(myReader);
                            foreach (WsdlNS.Port port in myDescription.Services[0].Ports)
                            {
                                contractType = type;
                                ContractDescription contractDescription = ContractDescription.GetContract(contractType);
                                if (contractDescription != null)
                                {
                                    EndpointAddress endpointAddress = new EndpointAddress(wsdlUrl);
                                    ServiceEndpoint serviceEndpoint = new ServiceEndpoint(contractDescription, bindings.FirstOrDefault(), endpointAddress);
                                    if (serviceEndpoint != null)
                                    {
                                        serviceEndpointCollection.Add(serviceEndpoint);
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
                else
                {
                    continue;
                }
            }

            if (contractType == null)
            {
                CleanUpReferenceFiles();
                throw new ArgumentException(
                    Constants.ErrorMessages.EndpointNotFound);
            }

            return serviceEndpointCollection;
        }

        void CleanUpReferenceFiles()
        {
            DirectoryInfo directory = new DirectoryInfo(fileDirectory);
            FileInfo[] files = directory.GetFiles();

            if (files.Length > 0)
            {
                Directory.Delete(fileDirectory, true);
            }
        }

        void CreateFilePath(string directory)
        {
            string pathString = System.IO.Path.Combine(directory, Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(pathString);

            fileDirectory = pathString;
        }
#endif
    }
}

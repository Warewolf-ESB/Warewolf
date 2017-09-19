
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Data.Design;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Web.Services.Discovery;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;







namespace Dev2.Runtime.DynamicProxy
{
    using WsdlNS = System.Web.Services.Description;


    public class DynamicProxyFactory
    {
        private readonly string wsdlUri;
        private readonly DynamicProxyFactoryOptions options;

        private CodeCompileUnit codeCompileUnit;
        private CodeDomProvider codeDomProvider;
        private ServiceContractGenerator contractGenerator;

        private Collection<MetadataSection> metadataCollection;
        private IEnumerable<Binding> bindings;
        private IEnumerable<ContractDescription> contracts;
        private ServiceEndpointCollection endpoints;
        private IEnumerable<MetadataConversionError> importWarnings;
        private IEnumerable<MetadataConversionError> codegenWarnings;
        private IEnumerable<CompilerError> compilerWarnings;

        private Assembly proxyAssembly;
        private string proxyCode;

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

            this.wsdlUri = wsdlUri;
            this.options = options;

            DownloadMetadata();
            ImportMetadata();
            CreateProxy();
            WriteCode();
            CompileProxy();
        }

        public DynamicProxyFactory(string wsdlUri)
            : this(wsdlUri, new DynamicProxyFactoryOptions())
        {
        }

        private void DownloadMetadata()
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


        private void ImportMetadata()
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
                var dcConverter =
                    importExtension as DataContractSerializerMessageContractImporter;

                if (dcConverter != null)
                {
                    if (options.FormatMode ==
                        DynamicProxyFactoryOptions.FormatModeOptions.XmlSerializer)
                    {
                        dcConverter.Enabled = false;
                    }
                    else
                    {
                        dcConverter.Enabled = true;
                    }
                }

            }
        }

        private void CreateProxy()
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

        private void CompileProxy()
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

        private void WriteCode()
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

        private bool ContractNameMatch(ContractDescription cDesc, string name)
        {
            return (string.Compare(cDesc.Name, name, true) == 0);
        }

        private bool ContractNsMatch(ContractDescription cDesc, string ns)
        {
            return ((ns == null) ||
                    (string.Compare(cDesc.Namespace, ns, true) == 0));
        }

        public DynamicProxy CreateProxy(string contractName)
        {
            return CreateProxy(contractName, null);
        }

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

        private Type GetContractType(string contractName,
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

            if (ns == null)
            {
                ns = DefaultNamespace;
            }
            else
            {
                ns = Uri.EscapeUriString(ns);
            }

            return new XmlQualifiedName(name, ns);
        }

        private Type GetProxyType(Type contractType)
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


        private void CreateCodeDomProvider()
        {
            codeDomProvider = CodeDomProvider.CreateProvider(options.Language.ToString());
        }

        private void CreateServiceContractGenerator()
        {
            contractGenerator = new ServiceContractGenerator(
                codeCompileUnit);
            contractGenerator.Options |= ServiceContractGenerationOptions.ClientClass;
        }

        public IEnumerable<MetadataSection> Metadata => metadataCollection;

        public IEnumerable<Binding> Bindings => bindings;

        public IEnumerable<ContractDescription> Contracts => contracts;

        public IEnumerable<ServiceEndpoint> Endpoints => endpoints;

        public Assembly ProxyAssembly => proxyAssembly;

        public string ProxyCode => proxyCode;

        public IEnumerable<MetadataConversionError> MetadataImportWarnings => importWarnings;

        public IEnumerable<MetadataConversionError> CodeGenerationWarnings => codegenWarnings;

        public IEnumerable<CompilerError> CompilationWarnings => compilerWarnings;

        public static string ToString(IEnumerable<MetadataConversionError>
            importErrors)
        {
            if (importErrors != null)
            {
                var importErrStr = new StringBuilder();

                foreach (var error in importErrors)
                {
                    if (error.IsWarning)
                    {
                        importErrStr.AppendLine("Warning : " + error.Message);
                    }
                    else
                    {
                        importErrStr.AppendLine("Error : " + error.Message);
                    }
                }

                return importErrStr.ToString();
            }
            return null;
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
            return null;
        }

        private static IEnumerable<CompilerError> ToEnumerable(
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
    }
}

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
    using Binding = System.ServiceModel.Channels.Binding;
    using WsdlNS = System.Web.Services.Description;


    public class DynamicProxyFactory
    {
        readonly string wsdlUrl;
        readonly DynamicProxyFactoryOptions options;

        CodeCompileUnit codeCompileUnit;
        CodeDomProvider codeDomProvider;

        IEnumerable<Binding> bindings;
        IEnumerable<System.ServiceModel.Description.ContractDescription> contracts;
        IEnumerable<System.ServiceModel.Description.ServiceEndpoint> endpoints;
        IEnumerable<CompilerError> compilerWarnings;

        Assembly proxyAssembly;
        string proxyCode;

        string fileDirectory = "C:/ProgramData/Warewolf/Temp/WCFReference";
        string fileName = "Reference.cs";

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

            if (!wsdlUri.Contains("?wsdl"))
            {
                wsdlUri = wsdlUri + "?wsdl";
            }

            this.wsdlUrl = wsdlUri;
            this.options = options;

            CreateFilePath(fileDirectory);
            ExecuteCommand("dotnet-svcutil " + wsdlUri + " --outputDir " + fileDirectory);
            CreateCodeDomProvider();
            WriteCodeFromFile();
            CompileProxy();
            contracts = GetAllContracts();
            bindings = GetAllBindings();
            endpoints = GetAllEndpoints();
            CleanUpReferenceFiles();
        }

        public DynamicProxyFactory(string wsdlUri)
            : this(wsdlUri, new DynamicProxyFactoryOptions())
        {
        }

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

        public IEnumerable<Binding> Bindings => bindings;

        public IEnumerable<System.ServiceModel.Description.ContractDescription> Contracts => contracts;

        public IEnumerable<ServiceEndpoint> Endpoints => endpoints;

        public Assembly ProxyAssembly => proxyAssembly;

        public string ProxyCode => proxyCode;

        public IEnumerable<CompilerError> CompilationWarnings => compilerWarnings;


        public static string ToString(IEnumerable<CoreWCF.Description.MetadataConversionError>
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
    }
}

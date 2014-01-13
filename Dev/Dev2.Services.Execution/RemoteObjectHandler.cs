using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting;
using System.Xml;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.DataList.Contract;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Services.Execution
{
    /// <summary>
    /// Private class used to convert string method data into an internal TO
    /// </summary>
    public class Dev2TypeConversion
    {
        // Travis.Frisinger : 31-08-2012
        private readonly Type _t;
        private readonly string _val;

        internal Dev2TypeConversion(Type t, string val)
        {
            _t = t;
            _val = val;
        }

        internal Type FetchType()
        {
            return _t;
        }

        internal string FetchVal()
        {
            return _val;
        }
    }

    public class RemoteObjectHandler : MarshalByRefObject
    {

        public ErrorResultTO Errors { get; private set; }

        public RemoteObjectHandler()
        {
            Errors = new ErrorResultTO();
        }

        /// <summary>
        /// Execute a plugin to extracts is output for mapping/conversion to XML
        /// </summary>
        /// <param name="assemblyLocation"></param>
        /// <param name="assemblyName"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string InterrogatePlugin(string assemblyLocation, string assemblyName, string method, string args)
        {
            // Travis.Frisinger : 31-08-2012 - Change this method to intelligently find a method signature
            string result;
            try
            {
                // the way this is invoked so as to consider the arg order and type

                var pluginResult = GetPluginResult(assemblyLocation, assemblyName, method, args);

                IOutputDescription ouputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
                IDataSourceShape dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();

                ouputDescription.DataSourceShapes.Add(dataSourceShape);

                IDataBrowser dataBrowser = DataBrowserFactory.CreateDataBrowser();
                dataSourceShape.Paths.AddRange(dataBrowser.Map(pluginResult));

                IOutputDescriptionSerializationService outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();
                result = outputDescriptionSerializationService.Serialize(ouputDescription);
            }
            catch(Exception ex)
            {
                XElement errorResult = new XElement("Error");
                errorResult.Add(ex);
                result = errorResult.ToString();
            }

            return result;
        }

        object GetPluginResult(string assemblyLocation, string assemblyName, string method, string args)
        {
            object loadedAssembly = null;
            IList<Dev2TypeConversion> convertedArgs = null;
            if(args != string.Empty)
            {
                convertedArgs = ConvertXMLToConcrete(args);
            }
            var asm = LoadAssembly(ref assemblyLocation, assemblyName, ref loadedAssembly);

            // load deps ;)
            LoadDepencencies(asm, assemblyLocation);

            MethodInfo methodToRun;
            object pluginResult = null;

            if(convertedArgs != null && convertedArgs.Count == 0)
            {
                if(loadedAssembly != null)
                {
                    methodToRun = loadedAssembly.GetType().GetMethod(method);
                    pluginResult = methodToRun.Invoke(loadedAssembly, null);
                }
            }
            else
            {
                if(convertedArgs != null)
                {
                    Type[] targs = new Type[convertedArgs.Count];
                    object[] invokeArgs = new object[convertedArgs.Count];
                    // build the args array now ;)
                    int pos = 0;
                    foreach(Dev2TypeConversion tc in convertedArgs)
                    {
                        var conversionType = tc.FetchType();
                        var fetchVal = tc.FetchVal();
                        if(!conversionType.IsValueType && fetchVal == GlobalConstants.NullPluginValue)
                        {
                            fetchVal = null;
                        }
                        targs[pos] = conversionType;
                        invokeArgs[pos] = Convert.ChangeType(fetchVal, conversionType);
                        pos++;
                    }

                    // find method with correct signature ;)
                    if(loadedAssembly != null)
                    {
                        methodToRun = loadedAssembly.GetType().GetMethod(method, targs);
                        pluginResult = methodToRun.Invoke(loadedAssembly, invokeArgs);
                    }
                }
            }
            return pluginResult;
        }

        static Assembly LoadAssembly(ref string assemblyLocation, string assemblyName, ref object loadedAssembly)
        {
            Assembly asm;
            if(assemblyLocation.StartsWith("GAC:"))
            {
                asm = Assembly.Load(assemblyLocation);

                assemblyLocation = assemblyLocation.Remove(0, "GAC:".Length);
                Type t = Type.GetType(assemblyName);
                if(t != null)
                {
                    loadedAssembly = Activator.CreateInstance(t);
                }
            }
            else
            {
                asm = Assembly.LoadFrom(assemblyLocation);
                ObjectHandle objHAndle = Activator.CreateInstanceFrom(assemblyLocation, assemblyName);
                loadedAssembly = objHAndle.Unwrap();
            }
            return asm;
        }

        /// <summary>
        /// Invoke a plugin and return its results
        /// </summary>
        /// <param name="assemblyLocation"></param>
        /// <param name="assemblyName"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <param name="outputDescription"></param>
        /// <returns></returns>
        public string RunPlugin(string assemblyLocation, string assemblyName, string method, string args, IOutputDescription outputDescription)
        {
            // BUG 9619 - 2013.06.05 - TWR - Changed return type
            string result;

            try
            {
                var pluginResult = Run(assemblyLocation, assemblyName, method, args);
                result = FormatResult(pluginResult, outputDescription);
            }
            catch(Exception ex)
            {
                Errors.AddError(ex.Message);

                var errorResult = new XElement("Error");
                errorResult.Add(ex);
                result = errorResult.ToString();
            }

            return result;
        }

        /// <summary>
        /// Invoke a plugin and return its results
        /// </summary>
        /// <param name="assemblyLocation"></param>
        /// <param name="assemblyName"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <param name="outputDescription"></param>
        /// <returns></returns>
        public string RunPlugin(string assemblyLocation, string assemblyName, string method, string args, string outputDescription)
        {
            // BUG 9619 - 2013.06.05 - TWR - Changed return type
            string result;

            try
            {
                var pluginResult = Run(assemblyLocation, assemblyName, method, args);
                result = FormatResult(pluginResult, outputDescription);
            }
            catch(Exception ex)
            {
                var errorResult = new XElement("Error");
                errorResult.Add(ex);
                result = errorResult.ToString();
            }

            return result;
        }

        object Run(string assemblyLocation, string assemblyName, string method, string args)
        {

            // the way this is invoked so as to consider the arg order and type
            object pluginResult = GetPluginResult(assemblyLocation, assemblyName, method, args);
            return pluginResult;
        }

        //2013.06.12: Ashley Lewis for bug 9618 - small refacter
        public static string FormatResult(object result, string outputDescription)
        {
            var od = outputDescription.Replace("<Dev2XMLResult>", "").Replace("</Dev2XMLResult>", "").Replace("<JSON />", "").Replace("<InterrogationResult>", "").Replace("</InterrogationResult>", "");

            var outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();
            var outputDescriptionInstance = outputDescriptionSerializationService.Deserialize(od);
            return FormatResult(result, outputDescriptionInstance);
        }

        public static string FormatResult(object result, IOutputDescription outputDescription)
        {

            if(outputDescription != null)
            {
                var outputFormatter = OutputFormatterFactory.CreateOutputFormatter(outputDescription);
                // BUG 9618 - 2013.06.12 - TWR: fix for void return types
                return outputFormatter.Format(result ?? string.Empty).ToString();
            }
            // BUG 9619 - 2013.06.05 - TWR - Added
            var errorResult = new XElement("Error");
            errorResult.Add("Output format in service action is invalid");
            return errorResult.ToString();
        }

        #region Private Method

        /// <summary>
        /// Loads the depencencies.
        /// </summary>
        /// <param name="asm">The asm.</param>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <exception cref="System.Exception">Could not locate Assembly [  + assemblyLocation +  ]</exception>
        private void LoadDepencencies(Assembly asm, string assemblyLocation)
        {
            // load depencencies ;)
            if(asm != null)
            {
                var toLoadAsm = asm.GetReferencedAssemblies();

                foreach(var toLoad in toLoadAsm)
                {
                    // TODO : Detect GAC or File System Load ;)
                    Assembly.Load(toLoad);
                }
            }
            else
            {
                throw new Exception("Could not locate Assembly [ " + assemblyLocation + " ]");
            }
        }

        /// <summary>
        /// Used to iterate over the method argument payload and convert to concrete
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        private IList<Dev2TypeConversion> ConvertXMLToConcrete(string payload)
        {
            // Travis.Frisinger : 31-08-2012

            IList<Dev2TypeConversion> result = new List<Dev2TypeConversion>();

            /*
             * <Args>
             *  <Arg>
             *     <Value></Value>
             *     <TypeOf></TypeOf>
             *  </Arg>
             * </Args>
             */

            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(payload);

                // //Args/Args/Arg"
                XmlNodeList nl = xDoc.SelectNodes("//Args/Arg");
                if(nl != null)
                {
                    foreach(XmlNode n in nl)
                    {
                        XmlNodeList cnl = n.ChildNodes;
                        Type t = null;
                        string val = string.Empty;

                        foreach(XmlNode cn in cnl)
                        {
                            if(cn.Name == "TypeOf")
                            {
                                t = CreateType(cn.InnerText);
                            }
                            else if(cn.Name == "Value")
                            {
                                val = cn.InnerXml;
                            }
                        }

                        // add to the list
                        if(t != null)
                        {
                            result.Add(new Dev2TypeConversion(t, val));
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                // trapped because if it fails we assume no input into method 
                ServerLogger.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// Used to extract primative types from strings
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Type CreateType(string type)
        {
            // Travis.Frisinger : 31-08-2012

            Type result; // default to string

            type = type.Replace(Environment.NewLine, "");
            type = type.ToLower().Trim();

            if(type == "int" || type == "int32")
            {
                result = typeof(int);
            }
            else if(type == "char")
            {
                result = typeof(char);
            }
            else if(type == "double")
            {
                result = typeof(double);
            }
            else if(type == "byte")
            {
                result = typeof(byte);
            }
            else if(type == "uint" || type == "uint32")
            {
                result = typeof(uint);
            }
            else if(type == "short" || type == "int16")
            {
                result = typeof(short);
            }
            else if(type == "ushort" || type == "uint16")
            {
                result = typeof(ushort);
            }
            else if(type == "long" || type == "int64")
            {
                result = typeof(long);
            }
            else if(type == "ulong" || type == "uint64")
            {
                result = typeof(ulong);
            }
            else if(type == "float")
            {
                result = typeof(float);
            }
            else if(type == "bool")
            {
                result = typeof(bool);
            }
            else if(type == "decimal")
            {
                result = typeof(decimal);
            }
            else if(type == "string")
            {
                result = typeof(string);
            }
            else if(type == "object")
            {
                result = typeof(object);
            }
            else
            {
                throw new Exception("FATAL : Type [ " + type + " ] is not understood by the RemoteObjectHandler");
            }

            return result;
        }

        #endregion

    }
}
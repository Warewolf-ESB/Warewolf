using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting;
using System.Xml;
using System.Xml.Linq;
using Dev2.Common;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2
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
        public RemoteObjectHandler() { }

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
            string result = "";
            try
            {
                IList<Dev2TypeConversion> convertedArgs = null;
                ObjectHandle objHAndle = null;
                object loadedAssembly;
                if(args != string.Empty)
                {
                    convertedArgs = ConvertXMLToConcrete(args);
                }
                if(assemblyLocation.StartsWith("GAC:"))
                {
                    assemblyLocation = assemblyLocation.Remove(0, "GAC:".Length);
                    Type t = Type.GetType(assemblyName);
                    loadedAssembly = Activator.CreateInstance(t);
                }
                else
                {
                    objHAndle = Activator.CreateInstanceFrom(assemblyLocation, assemblyName);
                    loadedAssembly = objHAndle.Unwrap();
                }


                // the way this is invoked so as to consider the arg order and type
                MethodInfo methodToRun = null;
                object pluginResult = null;

                if(convertedArgs.Count == 0)
                {
                    methodToRun = loadedAssembly.GetType().GetMethod(method);
                    pluginResult = methodToRun.Invoke(loadedAssembly, null);
                }
                else
                {
                    Type[] targs = new Type[convertedArgs.Count];
                    object[] invokeArgs = new object[convertedArgs.Count];
                    // build the args array now ;)
                    int pos = 0;
                    foreach(Dev2TypeConversion tc in convertedArgs)
                    {
                        targs[pos] = tc.FetchType();
                        invokeArgs[pos] = Convert.ChangeType(tc.FetchVal(), tc.FetchType());
                        pos++;
                    }

                    // find method with correct signature ;)
                    methodToRun = loadedAssembly.GetType().GetMethod(method, targs);
                    pluginResult = methodToRun.Invoke(loadedAssembly, invokeArgs);

                }

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

        /// <summary>
        /// Invoke a plugin and return its results
        /// </summary>
        /// <param name="assemblyLocation"></param>
        /// <param name="assemblyName"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <param name="outputDescription"></param>
        /// <param name="formatOutput"></param>
        /// <returns></returns>
        public object RunPlugin(string assemblyLocation, string assemblyName, string method, string args, string outputDescription, bool formatOutput)
        {
            // BUG 9619 - 2013.06.05 - TWR - Changed return type
            object result = null;

            try
            {
                IList<Dev2TypeConversion> convertedArgs = null;
                ObjectHandle objHAndle = null;
                object loadedAssembly;

                if(args != string.Empty)
                {
                    convertedArgs = ConvertXMLToConcrete(args);
                }

                if(assemblyLocation.StartsWith("GAC:"))
                {
                    assemblyLocation = assemblyLocation.Remove(0, "GAC:".Length);
                    Type t = Type.GetType(assemblyName);
                    loadedAssembly = Activator.CreateInstance(t);
                }
                else
                {
                    objHAndle = Activator.CreateInstanceFrom(assemblyLocation, assemblyName);
                    loadedAssembly = objHAndle.Unwrap();
                }


                // the way this is invoked so as to consider the arg order and type
                MethodInfo methodToRun = null;
                object pluginResult = null;

                if(convertedArgs.Count == 0)
                {
                    methodToRun = loadedAssembly.GetType().GetMethod(method);
                    pluginResult = methodToRun.Invoke(loadedAssembly, null);
                }
                else
                {
                    Type[] targs = new Type[convertedArgs.Count];
                    object[] invokeArgs = new object[convertedArgs.Count];
                    // build the args array now ;)
                    int pos = 0;
                    foreach(Dev2TypeConversion tc in convertedArgs)
                    {
                        targs[pos] = tc.FetchType();
                        invokeArgs[pos] = AdjustType(tc.FetchVal(), tc.FetchType());
                        pos++;
                    }

                    // find method with correct signature ;)
                    methodToRun = loadedAssembly.GetType().GetMethod(method, targs);
                    pluginResult = methodToRun.Invoke(loadedAssembly, invokeArgs);

                }

                if(formatOutput)
                {
                    string od = outputDescription.Replace("<Dev2XMLResult>", "").Replace("</Dev2XMLResult>", "").Replace("<JSON />", "").Replace("<InterrogationResult>", "").Replace("</InterrogationResult>", "");

                    IOutputDescriptionSerializationService outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();
                    IOutputDescription outputDescriptionInstance = outputDescriptionSerializationService.Deserialize(od);

                    if(outputDescriptionInstance != null)
                    {
                        IOutputFormatter outputFormatter = OutputFormatterFactory.CreateOutputFormatter(outputDescriptionInstance);
                        result = outputFormatter.Format(pluginResult).ToString();
                    }
                }
                // BUG 9619 - 2013.06.05 - TWR - Added
                else
                {
                    result = pluginResult;
                }
            }
            catch(Exception ex)
            {
                XElement errorResult = new XElement("Error");
                errorResult.Add(ex);
                result = errorResult.ToString();
            }

            return result;
        }

        #region Private Method


        /// <summary>
        /// Change the argument type for invoke
        /// </summary>
        /// <param name="val"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private object AdjustType(string val, Type t)
        {
            object result = null;

            if(val != "NULL")
            {
                result = Convert.ChangeType(val, t);
            }
            else
            {
                // check if type is nullable, else return default value
                if(t.IsValueType) // ref type == nullable, value type != nullable
                {
                    // create a default value
                    result = Activator.CreateInstance(t);
                }
            }

            return result;
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

            Type result = typeof(object); // default to string

            type = type.Replace(Environment.NewLine, "");
            type = type.ToLower().Trim();

            if(type == "int")
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
            else if(type == "uint")
            {
                result = typeof(uint);
            }
            else if(type == "short")
            {
                result = typeof(short);
            }
            else if(type == "ushort")
            {
                result = typeof(ushort);
            }
            else if(type == "long")
            {
                result = typeof(long);
            }
            else if(type == "ulong")
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

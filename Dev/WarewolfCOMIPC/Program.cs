using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WarewolfCOMIPC.Client;



namespace WarewolfCOMIPC
{
    
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                return;
            }

            string token = args[0];

            // Create new named pipe with token from client
            Console.WriteLine("Starting Server Pipe Stream");
            using (var pipe = new NamedPipeServerStream(token, PipeDirection.InOut, 253, PipeTransmissionMode.Message))
            {
                Console.WriteLine("Waiting Server Pipe Stream");
                pipe.WaitForConnection();
                AcceptMessagesFromPipe(pipe);
            }
        }

        private static void AcceptMessagesFromPipe(NamedPipeServerStream pipe)
        {


            // Receive CallData from client
            //var formatter = new BinaryFormatter();
            Console.WriteLine("IpcClient Connected to Server Pipe Stream");
            var serializer = new JsonSerializer();
            var sr = new StreamReader(pipe);
            var jsonTextReader = new JsonTextReader(sr);
            string callData;
            try
            {
                callData = serializer.Deserialize<string>(jsonTextReader);
            }
            catch 
            {
                callData = null;
                //
            }
            if (callData != null)
            {
                Console.WriteLine("IpcClient Data read and Deserialized to Server Pipe Stream");
                Console.WriteLine(callData.GetType());
                var data = JsonConvert.DeserializeObject<CallData>(callData);

                while (data.Status != KeepAliveStatus.Close)
                {
                    Console.WriteLine("Executing");
                    try
                    {
                        LoadLibrary(data, serializer, pipe);
                    }
                    catch(Exception e)
                    {
                        var newException = new Exception("Error executing COM",e);
                        var sw = new StreamWriter(pipe);
                        try
                        {
                            serializer.Serialize(sw, newException);
                        }
                        catch
                        {
                            Console.WriteLine("IpcClient Data not read nor Deserialized to Server Pipe Stream");
                        } 
                        sw.Flush();
                        Console.WriteLine("Execution errored " + data.MethodToCall);
                    }
                    AcceptMessagesFromPipe(pipe);
                }
            }
            else
            {
                Console.WriteLine("IpcClient Data not read nor Deserialized to Server Pipe Stream");
            }
        }

        private static void LoadLibrary(CallData data, JsonSerializer formatter, NamedPipeServerStream pipe)
        {
            var execute = data.Execute;
            if (!string.IsNullOrEmpty(data.ExecuteType))
            {
                Enum.TryParse(data.ExecuteType, true, out execute);
            }
            var sw = new StreamWriter(pipe);
            switch (execute)
            {
                case Execute.GetType:
                    {
                        Console.WriteLine("Executing GetType for:" + data.CLSID);
                        var type = Type.GetTypeFromCLSID(data.CLSID, true);
                        var objectInstance = Activator.CreateInstance(type);
                        Type dispatchedtype = DispatchUtility.GetType(objectInstance, false);
                        Console.WriteLine("Got Type:" + dispatchedtype.FullName);
                        
                        Console.WriteLine("Serializing and sending:" + dispatchedtype.FullName);
                        formatter.Serialize(sw, dispatchedtype);
                        sw.Flush();
                        Console.WriteLine("Sent:" + dispatchedtype.FullName);
                    }
                    break;
                case Execute.GetMethods:
                    {
                        Console.WriteLine("Executing GeMethods for:" + data.CLSID);
                        var type = Type.GetTypeFromCLSID(data.CLSID, true);
                        var objectInstance = Activator.CreateInstance(type);
                        var dispatchedtype = DispatchUtility.GetType(objectInstance, false);
                        MethodInfo[] methods = dispatchedtype.GetMethods();

                        List<MethodInfoTO> methodInfos = methods
                            .Select(info => new MethodInfoTO
                            {
                                Name = info.Name,
                                Parameters = info
                                                 .GetParameters()
                                                 .Select(parameterInfo => new ParameterInfoTO
                                                 {
                                                     Name = parameterInfo.Name,
                                                     DefaultValue = parameterInfo.DefaultValue,
                                                     IsRequired = parameterInfo.IsOptional,
                                                     TypeName = parameterInfo.ParameterType.AssemblyQualifiedName
                                                 }).ToList()
                            }).ToList();
                        Console.WriteLine($"Got {methods.Count()} mrthods");
                        Console.WriteLine("Serializing and sending methods for:" + dispatchedtype.FullName);
                        var json = JsonConvert.SerializeObject(methodInfos);
                        formatter.Serialize(sw, json);
                        sw.Flush();
                        Console.WriteLine("Sent methods for:" + dispatchedtype.FullName);
                    }
                    break;
                case Execute.ExecuteSpecifiedMethod:
                {
                    Console.WriteLine("Executing GeMethods for:" + data.CLSID);
                    var type = Type.GetTypeFromCLSID(data.CLSID, true);
                    var objectInstance = Activator.CreateInstance(type);
                    var paramsObjects = BuildValuedTypeParams(data.Parameters);
                    try
                    {
                            var result = DispatchUtility.Invoke(objectInstance, data.MethodToCall, paramsObjects);
                            if (result != null && result.ToString() == "System.__ComObject")
                            {
                                var retType = DispatchUtility.GetType(result, false);
                                var props = retType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                                var retObj = new JObject();
                                foreach(var propertyInfo in props)
                                {
                                    var propValue = retType.InvokeMember(propertyInfo.Name, BindingFlags.Instance | BindingFlags.GetProperty, null, result, null);
                                    retObj.Add(propertyInfo.Name,new JValue(propValue.ToString()));
                                }
                                formatter.Serialize(sw, retObj);
                                sw.Flush();
                            }
                            else
                            {
                                if (result!=null && result.ToString() == "0")
                                {
                                    result = "0";
                                }
                                formatter.Serialize(sw, result ?? "Success");
                                sw.Flush();
                            }
                        Console.WriteLine("Execution completed " + data.MethodToCall);
                    }
                    catch(Exception ex)
                    {
                        if(ex.InnerException != null)
                        {
                            throw new COMException(ex.InnerException?.Message);
                        }
                    }
                }
                    break;
                case Execute.GetNamespaces:
                {
                        var type = Type.GetTypeFromCLSID(data.CLSID, true);
                        var loadedAssembly = type.Assembly;
                        // ensure we flush out the rubbish that GAC brings ;)
                        var namespaces = loadedAssembly.GetTypes()
                            .Select(t => t.FullName)
                            .Distinct()
                            .Where(q => q.IndexOf("`", StringComparison.Ordinal) < 0
                                        && q.IndexOf("+", StringComparison.Ordinal) < 0
                                        && q.IndexOf("<", StringComparison.Ordinal) < 0
                                        && !q.StartsWith("_")).ToList();
                        Console.WriteLine($"Got {namespaces.Count} namespaces");
                        formatter.Serialize(sw, namespaces);
                        sw.Flush();
                        Console.WriteLine("Sent methods for:" + type.FullName);
                    }
                    break;
            }
        }

        private static object[] BuildValuedTypeParams(ParameterInfoTO[] setupInfo)
        {
            var valuedTypeList = new object[setupInfo.Length];

            for (int index = 0; index < setupInfo.Length; index++)
            {
                var methodParameter = setupInfo[index];

                var methodParameterTypeName = methodParameter.TypeName;
                var type = Type.GetTypeFromProgID(methodParameterTypeName.Substring(0,methodParameterTypeName.IndexOf("&,",StringComparison.InvariantCultureIgnoreCase)));
                if (type != null)
                {
                    BuildObjectType(type, methodParameter, valuedTypeList, index);
                }
                else
                {
                    BuildPrimitiveType(methodParameterTypeName, methodParameter, valuedTypeList, index);
                }
            }
            return valuedTypeList;
        }

        private static void BuildPrimitiveType(string methodParameterTypeName, ParameterInfoTO methodParameter, object[] valuedTypeList, int index)
        {
            var type = Type.GetType(methodParameterTypeName);
            if(type != null)
            {
                try
                {
                    var provider = TypeDescriptor.GetConverter(type);
                    var convertFrom = provider.ConvertFrom(methodParameter.DefaultValue);
                    valuedTypeList[index] = convertFrom;
                }
                catch (Exception)
                {
                    var typeConverter = TypeDescriptor.GetConverter(methodParameter.DefaultValue);
                    var convertFrom = typeConverter.ConvertFrom(methodParameter.DefaultValue);
                    valuedTypeList[index] = convertFrom;
                }
            }
        }

        private static void BuildObjectType(Type type, ParameterInfoTO methodParameter, object[] valuedTypeList, int index)
        {
            var obj = Activator.CreateInstance(type);
            if (JsonConvert.DeserializeObject(methodParameter.DefaultValue.ToString()) is JObject anonymousType)
            {
                var props = anonymousType.Properties().ToList();
                foreach (var prop in props)
                {
                    var valueForProp = prop.Value.ToString();
                    type.InvokeMember(prop.Name, BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.Public, null, obj, new object[] { valueForProp });
                }
                valuedTypeList[index] = obj;
            }
        }
    }
}

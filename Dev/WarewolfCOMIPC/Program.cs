﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using WarewolfCOMIPC.Client;
// ReSharper disable NonLocalizedString

namespace WarewolfCOMIPC
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1) return;

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
            Console.WriteLine("Client Connected to Server Pipe Stream");
            var serializer = new JsonSerializer();
            var sr = new StreamReader(pipe);
            var jsonTextReader = new JsonTextReader(sr);
            object callData = new object();
            try
            {
                callData = serializer.Deserialize(jsonTextReader, typeof(CallData));
            }
            catch (Exception)
            {
                //
            }
            Console.WriteLine("Client Data read and Deserialized to Server Pipe Stream");
            Console.WriteLine(callData.GetType());
            var data = (CallData)callData;

            while (data.Status != KeepAliveStatus.Close)
            {
                Console.WriteLine("Executing");
                LoadLibrary(data, serializer, pipe);
                AcceptMessagesFromPipe(pipe);
            }
        }

        private static void LoadLibrary(CallData data, JsonSerializer formatter, NamedPipeServerStream pipe)
        {
            var execute = data.Execute;
            if (!string.IsNullOrEmpty(data.ExecuteType))
            {
                Enum.TryParse(data.ExecuteType, true, out execute);
            }

            switch (execute)
            {
                case Execute.GetType:
                    {
                        Console.WriteLine("Executing GetType for:" + data.CLSID);
                        var type = Type.GetTypeFromCLSID(data.CLSID, true);
                        var objectInstance = Activator.CreateInstance(type);
                        Type dispatchedtype = DispatchUtility.GetType(objectInstance, false);
                        Console.WriteLine("Got Type:" + dispatchedtype.FullName);
                        var sw = new StreamWriter(pipe);
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
                        var sw = new StreamWriter(pipe);
                        Console.WriteLine("Serializing and sending methods for:" + dispatchedtype.FullName);
                        formatter.Serialize(sw, methodInfos);
                        sw.Flush();
                        Console.WriteLine("Sent methods for:" + dispatchedtype.FullName);
                    }
                    break;
                case Execute.ExecuteSpecifiedMethod:
                    {
                        Console.WriteLine("Executing GeMethods for:" + data.CLSID);
                        var type = Type.GetTypeFromCLSID(data.CLSID, true);
                        var objectInstance = Activator.CreateInstance(type);
                        var paramsObjects = data.Parameters.ToList();
                        object[] correctValues = new object[paramsObjects.Count];
                        for (int index = 0; index < paramsObjects.Count; index++)
                        {
                            var o = paramsObjects[index];
                            var cleanValue = o.ToString()
                                .Replace("]", "")
                                .Replace("[", "")
                                .Replace("\r", "")
                                .Replace(Environment.NewLine, "")
                                .Replace("\n", "")
                                .Trim();
                            correctValues[index] = cleanValue;
                        }
                        try
                        {
                            var result = DispatchUtility.Invoke(objectInstance, data.MethodToCall, correctValues);
                            var sw = new StreamWriter(pipe);
                            formatter.Serialize(sw, result ?? "Success");
                            sw.Flush();
                            Console.WriteLine("Execution completed " + data.MethodToCall);
                        }
                        catch (Exception ex)
                        {
                            if (ex.InnerException != null)
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
                        var sw = new StreamWriter(pipe);
                        formatter.Serialize(sw, namespaces);
                        sw.Flush();
                        Console.WriteLine("Sent methods for:" + type.FullName);
                    }
                    break;
            }
        }

    }
}

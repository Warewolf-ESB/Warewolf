#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using Newtonsoft.Json;
using System.Reflection;

namespace WarewolfCOMIPC.Client
{
    public interface IIpcClient : IDisposable
    {
        IIpcClient GetIPCExecutor(INamedPipeClientStreamWrapper clientStreamWrapper);
        object Invoke(Guid clsid, string function, Execute execute, ParameterInfoTO[] args);
    }

    public static class IpcClient
    {
        static IpcClientImpl _ipcClient;
        static readonly object _lock = new object();

        public static IIpcClient GetIPCExecutor() => Instance;
        public static IIpcClient GetIPCExecutor(INamedPipeClientStreamWrapper clientStreamWrapper) => Instance.GetIPCExecutor(clientStreamWrapper);
        
        public static IIpcClient Instance
        {
            get
            {
                if (_ipcClient == null)
                {
                    lock (_lock)
                    {
                        if (_ipcClient == null)
                        {
                            _ipcClient = new IpcClientImpl();
                        }
                    }
                }
                return _ipcClient;
            }
        }

    }

    public class IpcClientImpl : IIpcClient, IDev2IpcClient
    {
        bool _disposed;
        readonly INamedPipeClientStreamWrapper _pipeWrapper;
        readonly Process _process;


        internal IpcClientImpl()
        {
            var token = Guid.NewGuid().ToString();
            var currentAssemblyPath = Assembly.GetExecutingAssembly().Location;
            var currentAssemblyDirectoryPath = Path.GetDirectoryName(currentAssemblyPath);
            var clientPath = Path.Combine(currentAssemblyDirectoryPath, "Warewolf.COMIPC.exe");
            var psi = new ProcessStartInfo(clientPath, token)
            {
                Verb = "runas",
                UseShellExecute = false,
                ErrorDialog = false,
                RedirectStandardOutput = false,
                CreateNoWindow = true
            };
            _process = Process.Start(psi);
            _pipeWrapper = new NamedPipeClientStreamWrapper(".", token, PipeDirection.InOut);
            _pipeWrapper.Connect();
        }

        public IpcClientImpl(INamedPipeClientStreamWrapper clientStreamWrapper) => _pipeWrapper = clientStreamWrapper;

       
        public  IIpcClient GetIPCExecutor(INamedPipeClientStreamWrapper clientStreamWrapper)
        {
            if (clientStreamWrapper != null)
            {
                return new IpcClientImpl(clientStreamWrapper);
            }
            return IpcClient.Instance;
        }

        public object Invoke(Guid clsid, string function, Execute execute, ParameterInfoTO[] args)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(IpcClient));
            }

            var info = new CallData
            {
                CLSID = clsid,
                MethodToCall = function,
                Parameters = args,
                ExecuteType = execute.ToString(),
                Execute = execute
            };

            // Write request to server
            var serializer = new JsonSerializer();
            var sw = new StreamWriter(_pipeWrapper.GetInternalStream());
            serializer.Serialize(sw, JsonConvert.SerializeObject(info));
            sw.Flush();

            var sr = new StreamReader(_pipeWrapper.GetInternalStream());
            var jsonTextReader = new JsonTextReader(sr);

            object result;
            switch (info.Execute)
            {

                case Execute.GetType:
                    {

                        result = serializer.Deserialize(jsonTextReader, typeof(string));
                        if (result is Exception exception)
                        {
                            throw exception;
                        }
                        var ipCreturn = result as string;
                        var reader = new StringReader(ipCreturn ?? "");

                        try
                        {
                           return serializer.Deserialize(reader, typeof(Type));
                        }
                        catch (Exception ex)
                        {
                            // Do nothing was not an exception
                            var baseException = ex.GetBaseException();
                            return new KeyValuePair<bool, string>(true, baseException.Message);
                        }
                        

                    }
                case Execute.GetMethods:
                    {
                        result = serializer.Deserialize(jsonTextReader, typeof(string));
                        if (result is Exception exception)
                        {
                            throw exception;
                        }

                        var value = result?.ToString();
                        return value == null ? new List<MethodInfoTO>() : JsonConvert.DeserializeObject<List<MethodInfoTO>>(value);
                    }
                case Execute.GetNamespaces:
                    {
                        result = serializer.Deserialize(jsonTextReader, typeof(List<string>));
                        if (result is Exception exception)
                        {
                            throw exception;
                        }
                        return result;

                    }
                case Execute.ExecuteSpecifiedMethod:
                    {

                        try
                        {
                            var obj = serializer.Deserialize(jsonTextReader);
                            result = obj.ToString();
                            var exception = JsonConvert.DeserializeObject<Exception>(result.ToString());
                            if (exception != null)
                            {
                                throw exception;
                            }
                        }
                        catch (Exception ex)
                        {
                            // Do nothing was not an exception
                            var baseException = ex.GetBaseException();
                            return new KeyValuePair<bool,string>(true, baseException.Message);
                        }
                        return result;
                    }

                default:
                    return null;
            }

        }
        
        protected void Close()
        {
            _pipeWrapper.Close();
            _process?.Kill();
        }

        ~IpcClientImpl()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Close();
                _pipeWrapper.Dispose();
            }

            // Free any unmanaged objects here.

            _disposed = true;
        }
    }
}

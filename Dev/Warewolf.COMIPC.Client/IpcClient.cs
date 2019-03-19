#pragma warning disable
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

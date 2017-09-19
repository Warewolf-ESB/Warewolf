using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using Newtonsoft.Json;
using System.Reflection;



namespace WarewolfCOMIPC.Client
{
    public class IpcClient : IDisposable, IDev2IpcClient
    {
        private bool _disposed;
        private readonly INamedPipeClientStreamWrapper _pipeWrapper;
        private readonly Process _process;
        private static IpcClient _ipcClient;
        private static readonly object padlock = new object();

        private IpcClient()
        {
            string token = Guid.NewGuid().ToString();

            // Pass token to child process
            string currentAssemblyPath = Assembly.GetExecutingAssembly().Location;
            string currentAssemblyDirectoryPath = Path.GetDirectoryName(currentAssemblyPath);
            string clientPath = Path.Combine(currentAssemblyDirectoryPath, "WarewolfCOMIPC.exe");
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

            _pipeWrapper.ReadMode = PipeTransmissionMode.Message;
        }

        public IpcClient(INamedPipeClientStreamWrapper clientStreamWrapper)
        {
            _pipeWrapper = clientStreamWrapper;
        }


        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static IpcClient GetIPCExecutor(INamedPipeClientStreamWrapper clientStreamWrapper = null)
        {
            if (clientStreamWrapper != null)
            {
                return new IpcClient(clientStreamWrapper);
            }
            lock (padlock)
            {
                return _ipcClient ?? (_ipcClient = new IpcClient());
            }
        }


        /// <summary>
        /// Executes a call to a library.
        /// </summary>
        /// <param name="clsid"></param>
        /// <param name="function">Name of the function to call.</param>
        /// <param name="execute"></param>
        /// <param name="args">Array of args to pass to the function.</param>
        /// <returns>Result object returned by the library.</returns>
        /// <exception cref="Exception">This Method will rethrow all exceptions thrown by the wrapper.</exception>
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
                        var exception = result as Exception;
                        if (exception != null)
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
                        var exception = result as Exception;
                        if (exception != null)
                        {
                            throw exception;
                        }

                        var value = result?.ToString();
                        return value == null ? new List<MethodInfoTO>() : JsonConvert.DeserializeObject<List<MethodInfoTO>>(value);
                    }
                case Execute.GetNamespaces:
                    {
                        result = serializer.Deserialize(jsonTextReader, typeof(List<string>));
                        var exception = result as Exception;
                        if (exception != null)
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

        /// <summary>
        /// Gracefully close connection to server
        /// </summary>
        protected void Close()
        {
            _pipeWrapper.Close();
            _process?.Kill();
        }

        ~IpcClient()
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

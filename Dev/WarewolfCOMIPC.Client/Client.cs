﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

// ReSharper disable NonLocalizedString
namespace WarewolfCOMIPC.Client
{
    public class Client : IDisposable
    {
        private bool _disposed;
        private readonly NamedPipeClientStream _pipe;
        private readonly Process _process;
        private static Client _client;
        private static readonly object padlock = new object();

        private Client()
        {
            string token = Guid.NewGuid().ToString();

            // Pass token to child process
            var psi = new ProcessStartInfo("WarewolfCOMIPC.exe", token)
            {
                Verb = "runas",
                UseShellExecute = false,
                ErrorDialog = false,
                RedirectStandardOutput = false,
                CreateNoWindow = true
            };
            _process = Process.Start(psi);
            _pipe = new NamedPipeClientStream(".", token, PipeDirection.InOut);
            _pipe.Connect();
            
            _pipe.ReadMode = PipeTransmissionMode.Message;
        }
        
        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static Client IPCExecutor
        {
            get
            {
                lock (padlock)
                {
                    return _client ?? (_client = new Client());
                }
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
                throw new ObjectDisposedException(nameof(Client));
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
            var sw = new StreamWriter(_pipe);
            serializer.Serialize(sw, JsonConvert.SerializeObject(info));
            sw.Flush();

            var sr = new StreamReader(_pipe);
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
                        var IPCreturn = result as string;
                        var reader = new StringReader(IPCreturn);
                        return serializer.Deserialize(reader, typeof(Type));

                    }
                case Execute.GetMethods:
                    {
                        result = serializer.Deserialize(jsonTextReader, typeof(string));
                        var exception = result as Exception;
                        if (exception != null)
                        {
                            throw exception;
                        }
                        
                        return JsonConvert.DeserializeObject<List<MethodInfoTO>>(result.ToString());
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
                        var obj = serializer.Deserialize(jsonTextReader);
                        result = obj.ToString();
                        try
                        {
                            var exception = JsonConvert.DeserializeObject<Exception>(result.ToString());
                            if (exception != null)
                            {
                                throw exception;
                            }
                        }
                        catch(Exception)
                        {
                            // Do nothing was not an exception
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
        protected virtual void Close()
        {
            _pipe.Close();
            _process.Kill();
        }

        ~Client()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Close();
                _pipe.Dispose();
            }

            // Free any unmanaged objects here.

            _disposed = true;
        }
    }
}

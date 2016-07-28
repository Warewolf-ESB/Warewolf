using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using Newtonsoft.Json;

// ReSharper disable NonLocalizedString
namespace WarewolfCOMIPC.Client
{
    public class Client : IDisposable
    {
        private bool _disposed;
        private readonly NamedPipeClientStream _pipe;
        private Process _process;

        public Client()
        {
            string token = Guid.NewGuid().ToString();

            // Pass token to child process

            _process = Process.Start("WarewolfCOMIPC.exe", token);

            _pipe = new NamedPipeClientStream(".", token, PipeDirection.InOut);
            _pipe.Connect();
            _pipe.ReadMode = PipeTransmissionMode.Message;
        }

        /// <summary>
        /// Executes a call to a library.
        /// </summary>
        /// <param name="clsid"></param>
        /// <param name="function">Name of the function to call.</param>
        /// <param name="executeType"></param>
        /// <param name="args">Array of args to pass to the function.</param>
        /// <returns>Result object returned by the library.</returns>
        /// <exception cref="Exception">This Method will rethrow all exceptions thrown by the wrapper.</exception>
        public object Invoke(Guid clsid, string function, string executeType, object[] args)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Client));


            var info = new CallData
            {
                CLSID = clsid,
                MethodToCall = function,
                Parameters = args,
                ExecuteType = executeType,
            };
            // Write request to server
            var serializer = new JsonSerializer();
            var sw = new StreamWriter(_pipe,Encoding.UTF8,256,true);
            serializer.Serialize(sw, info);
            sw.Flush();

            var sr = new StreamReader(_pipe);
            var jsonTextReader = new JsonTextReader(sr);
            var result = serializer.Deserialize(jsonTextReader,typeof(Type));
            var exception = result as Exception;
            if (exception != null)
            {
                throw exception;
            }            
            return result;
            

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

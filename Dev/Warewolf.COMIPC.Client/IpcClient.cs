#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using Warewolf.COMIPC.Client;

namespace WarewolfCOMIPC.Client
{
    public interface IIpcClient : IDisposable
    {
        IIpcClient GetIpcExecutor(INamedPipeClientStreamWrapper clientStreamWrapper);
        object Invoke(Guid clsid, string function, Execute execute, ParameterInfoTO[] args);
    }

    public static class IpcClient
    {
        static IpcClientImpl _ipcClient;
        static readonly object _lock = new object();

        public static IIpcClient GetIpcExecutor() => Instance;
        public static IIpcClient GetIpcExecutor(INamedPipeClientStreamWrapper clientStreamWrapper) => Instance.GetIpcExecutor(clientStreamWrapper);
        
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
        
        public  IIpcClient GetIpcExecutor(INamedPipeClientStreamWrapper clientStreamWrapper)
        {
            if (clientStreamWrapper != null)
            {
                return new IpcClientImpl(clientStreamWrapper);
            }
            return IpcClient.Instance;
        }
        
        protected void Close()
        {
            _pipeWrapper.Close();
            if (_process != null)
            {
                while (!_process.HasExited)
                {
                    if (!_process.WaitForExit(1000))
                    {
                        try
                        {
                            _process?.Kill();
                        }
                        catch { }
                    }
                }
            }
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

        public object Invoke(Guid clsid, string function, Execute execute, ParameterInfoTO[] args)
        {
           return new IpcClientHelper(_disposed, _pipeWrapper).Invoke(clsid, function, execute, args);
        }
    }
}

using Dev2.Common;
using System;
using System.IO;
using System.IO.Pipes;

namespace WarewolfCOMIPC.Client
{
    public class NamedPipeClientStreamWrapper : INamedPipeClientStreamWrapper, IDisposable
    {
        readonly NamedPipeClientStream _pipeClientStream;
        public NamedPipeClientStreamWrapper(string v, string token, PipeDirection inOut)
        {
            _pipeClientStream = new NamedPipeClientStream(v, token, inOut);
        }

        public Stream GetInternalStream()
        {
            return _pipeClientStream;
        }

        public void Connect()
        {
            try
            {
                _pipeClientStream.Connect(30000);
            }
            catch (TimeoutException e)
            {
                Dev2Logger.Info("Error connecting to COM Named Pipe Client Stream:\n" + e.Message, GlobalConstants.WarewolfInfo);
            }
        }

        public void Close()
        {
            _pipeClientStream.Close();
        }

        public void Dispose()
        {
            _pipeClientStream.Dispose();
        }

        public PipeTransmissionMode ReadMode
        {
            get
            {
                return _pipeClientStream.ReadMode;
            }
            set
            {
                _pipeClientStream.ReadMode = value;
            }
        }
    }
}